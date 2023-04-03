using GeographyModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
        var liteLayers = new List<Layer>();
        foreach (var layer in repository._layers.Values)
        {
            if (!layer.IsElectrical) continue;
            var liteLayer = new Layer()
            {
                Code = layer.Code,
                Displayname = layer.Displayname,
                GeographyType = layer.GeographyType,
                Fields = new List<LayerField>(),
                ElementDisplaynameFormat = "{LAYERNAME} ({CODE})",
                IsRoutingSource = layer.IsRoutingSource,
                IsElectrical = layer.IsElectrical,
                IsDisconnector = layer.IsDisconnector,
                OperationStatusFieldCode = "",
                OperationStatusAbnormalValues = new List<string>(),
                IsNormalOpen = layer.IsNormalOpen
            };

            if (layer.OperationStatusField != null)
            {
                liteLayer.ElementDisplaynameFormat = "{LAYERNAME} ({CONNECTED}, {CODE})";
                liteLayer.Fields.Add(new LayerField()
                {
                    Index = 0,
                    Code = layer.OperationStatusField.Code,
                    Displayname = layer.OperationStatusField.Displayname,
                });
                liteLayer.OperationStatusFieldCode = layer.OperationStatusFieldCode;
                liteLayer.OperationStatusAbnormalValues = layer.OperationStatusAbnormalValues;
            }
            liteLayers.Add(liteLayer);
        }
        File.WriteAllText(layersFilename, JsonSerializer.Serialize(liteLayers));

        foreach (var layer in repository._layers.Values)
        {
            if (!layer.IsElectrical) continue;
            var layerElements = repository.getLayerElements(layer.Code);
            if(!layerElements.Any()) continue;

            var counter = 0;
            foreach (var splited in SplitList(layerElements))
            {
                counter++;
                var layerElementsFilename = Path.Combine(root, $"Layer_{layer.Code}_{counter}.json");

                var liteLayerElements = new List<LayerElement>();
                foreach (var item in splited)
                {
                    var liteLayerElement = new LayerElement()
                    {
                        Code = item.Code,
                        Version = item.Version,
                        Points = item.Points,
                        FieldValuesText = layer.OperationStatusField is null ? "" : layer.OperationStatusField.GetValue(item.FieldValues),
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
            var layerElements = repository.getLayerElements(layer.Code);
            if(!layerElements.Any()) continue;

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
        var layers = default(List<Layer>);
        var layerElementsPackage = new Dictionary<string, List<LayerElement>>();

        InvokeByStopwatch(
           $"Load data",
           logAction,
           () =>
           {
               layers = DeSerializeEntity<List<Layer>>(Path.Combine(root, "Layers.json"));

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
                   var layerElements = DeSerializeEntity<List<LayerElement>>(refinedItem.Filename);
                   lock (layerElementsPackage)
                   {
                       layerElementsPackage[refinedItem.LayerCode].AddRange(layerElements);
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
                    logAction, () => repository.Initial(layers));

                foreach (var layerElementsPackageItem in layerElementsPackage)
                {
                    InvokeByStopwatch(
                        $"Initial Repository> elements of {layerElementsPackageItem.Key}",
                        logAction, () => repository.Initial(layerElementsPackageItem.Key, layerElementsPackageItem.Value));
                }

                repository.EndInitial(null);
            });

        return instance;
    }

    private static void InvokeByStopwatch(string title, Action<string> logAction, Action action)
    {
        logAction?.Invoke($"{title} begin...");
        var stopwatch = Stopwatch.StartNew();
        action.Invoke();
        stopwatch.Stop();
        logAction?.Invoke($"{title} end ({stopwatch.ElapsedMilliseconds:N0} ms)");
    }
}
