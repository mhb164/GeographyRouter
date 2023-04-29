using GeographyModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Geography.IO
{
    public enum GeographyType
    {
        Point = 1,
        Polyline = 2,
        Polygon = 3,
    }

    [DataContract]
    public class Layer
    {
        [DataMember(Order = 01)]
        public string Code { get; set; }

        [DataMember(Order = 02)]
        public string Displayname { get; set; }

        [DataMember(Order = 03)]
        public GeographyType GeographyType { get; set; }

        [DataMember(Order = 04)]
        public List<Field> Fields { get; set; }

        [DataMember(Order = 05)]
        public string ElementDisplaynameFormat { get; set; }

        [DataMember(Order = 06)]
        public bool IsRoutingSource { get; set; }

        [DataMember(Order = 07)]
        public bool IsElectrical { get; set; }

        [DataMember(Order = 08)]
        public bool IsDisconnector { get; set; }
    }

    [DataContract]
    public class Field
    {
        [DataMember(Order = 01)]
        public int Index { get; set; }
        [DataMember(Order = 02)]
        public string Code { get; set; }
        [DataMember(Order = 03)]
        public string Displayname { get; set; }
    }

    public enum ElementStatus
    {
        Open = 0,
        Close = 1,
    }

    [DataContract]
    public class Element
    {
        [DataMember(Order = 01)]
        public string Code { get; set; }

        [DataMember(Order = 02)]
        public double[] Points { get; set; }

        [DataMember(Order = 03)]
        public bool Connected { get; set; }
    }

}

public partial class GeographyRepository
{
    public void Save(string root, bool lite, Action<string> logAction)
    {
        if (lite) SaveLite(this, root, logAction);
        else Save(this, root, logAction);
    }

    private static void SaveLite(GeographyRepository repository, string root, Action<string> logAction)
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
        Directory.CreateDirectory(root);

        var layersFilename = Path.Combine(root, "Layers.json");
        var liteLayers = new List<Geography.IO.Layer>();
        foreach (var layer in repository._layers.Values)
        {
            if (!layer.IsElectrical) continue;
            var liteLayer = new Geography.IO.Layer()
            {
                Code = layer.Code,
                Displayname = layer.Displayname,
                GeographyType = (Geography.IO.GeographyType)(int)layer.GeographyType,
                Fields = new List<Geography.IO.Field>(),
                ElementDisplaynameFormat = layer.IsElectrical ? "{LAYERNAME} ({CONNECTED}, {CODE})" : "{LAYERNAME} ({CODE})",
                IsRoutingSource = layer.IsRoutingSource,
                IsElectrical = layer.IsElectrical,
                IsDisconnector = layer.IsDisconnector,
            };

            liteLayers.Add(liteLayer);
        }
        File.WriteAllText(layersFilename, JsonSerializer.Serialize(liteLayers));

        foreach (var layer in repository._layers.Values)
        {
            if (!layer.IsElectrical) continue;
            var layerElements = repository.GetLayerElementsWithoutLock(layer.Code);
            if (!layerElements.Any()) continue;

            var counter = 0;
            foreach (var splited in SplitList(layerElements))
            {
                counter++;
                var layerElementsFilename = Path.Combine(root, $"Layer_{layer.Code}_{counter}.json");

                var liteLayerElements = new List<Geography.IO.Element>();
                foreach (var item in splited)
                {
                    var liteLayerElement = new Geography.IO.Element()
                    {
                        Code = item.Code,
                        Points = item.Points,
                        Connected = item.Connected,
                    };

                    liteLayerElements.Add(liteLayerElement);
                }
                File.WriteAllText(layerElementsFilename, JsonSerializer.Serialize(liteLayerElements));
            }
        }
    }

    public static void Save(GeographyRepository repository, string root, Action<string> logAction)
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
        Directory.CreateDirectory(root);

        var layersFilename = Path.Combine(root, "Layers.json");
        File.WriteAllText(layersFilename, JsonSerializer.Serialize(repository._layers.Values.ToList()));

        foreach (var layer in repository._layers.Values)
        {
            var layerElements = repository.GetLayerElementsWithoutLock(layer.Code);
            if (!layerElements.Any()) continue;

            var counter = 0;
            foreach (var splited in SplitList(layerElements))
            {
                counter++;
                var layerElementsFilename = Path.Combine(root, $"Layer_{layer.Code}_{counter}.json");
                File.WriteAllText(layerElementsFilename, JsonSerializer.Serialize(splited));
            }
        }
    }
    private static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 5000)
    {
        for (int i = 0; i < locations.Count; i += nSize)
        {
            yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
        }
    }


    public static T DeSerializeEntity<T>(string filename)
    {
        using (var stream = File.OpenRead(filename))
        {
            return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
        }
    }

    private static readonly Regex LayerFilePattern = new Regex(@"Layer_(?<layerCode>\D+)_(?<num>\d+)");
    public static T Load<T>(string root, Action<string> logAction) where T : GeographyRepository
    {
        var layers = default(Dictionary<string, Layer>);
        var layerElementsPackage = new Dictionary<string, List<LayerElement>>();

        InvokeByStopwatch(
           $"Load data",
           logAction,
           () =>
           {
               var layersFromDisk = DeSerializeEntity<List<Geography.IO.Layer>>(Path.Combine(root, "Layers.json"));
               layers = layersFromDisk.Select(x => ToDomain(x))
                                      .ToDictionary(x => x.Code);

               var refined = Directory.GetFiles(root, "Layer_*.json").Select(layerfilename =>
               {
                   var name = Path.GetFileNameWithoutExtension(layerfilename);
                   var layerCode = LayerFilePattern.Match(name).Groups["layerCode"].Value;
                   var layerFileNumber = int.Parse(LayerFilePattern.Match(name).Groups["num"].Value);

                   return (Filename: layerfilename,
                           Name: name,
                           LayerCode: layerCode,
                           LayerFileNumber: layerFileNumber);
               });

               foreach (var item in refined.GroupBy(x => x.LayerCode))
               {
                   layerElementsPackage.Add(item.Key, new List<LayerElement>(item.Count() * 5000));
               }

               Parallel.ForEach(refined, (refinedItem) =>
               {
                   var layer = layers[refinedItem.LayerCode];
                   var layerElements = DeSerializeEntity<List<Geography.IO.Element>>(refinedItem.Filename);
                   lock (layerElementsPackage)
                   {
                       layerElementsPackage[refinedItem.LayerCode].AddRange(layerElements.Select(x => ToDomain(layer, x)).ToList());
                   }
               });

           });

        var instance = (T)Activator.CreateInstance(typeof(T), new object[] { logAction });
        var repository = (GeographyRepository)instance;

        InvokeByStopwatch(
            $"Initial Repository",
            logAction,
            () =>
            {
                repository.BeginInitial();

                InvokeByStopwatch(
                    "Initial Repository> layers & fields",
                    logAction, () => repository.Initial(layers.Values));

                foreach (var layerElementsPackageItem in layerElementsPackage)
                {
                    InvokeByStopwatch(
                        $"Initial Repository> elements of {layerElementsPackageItem.Key}",
                        logAction, () => repository.Initial(layerElementsPackageItem.Value));
                }

                repository.EndInitial(null);
            });

        return instance;
    }

    private static Layer ToDomain(Geography.IO.Layer input)
        => new Layer(input.Code,
                     input.Displayname,
                     (LayerGeographyType)(int)input.GeographyType,
                     input.ElementDisplaynameFormat,
                     input.IsRoutingSource,
                     input.IsElectrical,
                     input.IsDisconnector);

    private static LayerElement ToDomain(Layer layer, Geography.IO.Element input)
        => new LayerElement(layer,
                            input.Code,
                            input.Points,
                            Array.Empty<string>(),
                            0,
                            input.Connected ? LayerElementStatus.Close : LayerElementStatus.Open,
                            input.Connected ? LayerElementStatus.Close : LayerElementStatus.Open,
                            0);



    private static void InvokeByStopwatch(string title, Action<string> logAction, Action action)
    {
        logAction?.Invoke($"{title} begin...");
        var stopwatch = Stopwatch.StartNew();
        action.Invoke();
        stopwatch.Stop();
        logAction?.Invoke($"{title} end ({stopwatch.ElapsedMilliseconds:N0} ms)");
    }
}
