using GeographyModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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

        var serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        var layersFilename = Path.Combine(root, "Layers.json");
        var liteLayers = new List<Layer>();
        foreach (var layer in repository.layers.Values)
        {
            if (layer.IsElectrical == false) continue;
            var liteLayer = new Layer()
            {
                Id = layer.Id,
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
                liteLayer.Fields.Add(layer.OperationStatusField);
                liteLayer.OperationStatusFieldCode = layer.OperationStatusFieldCode;
                liteLayer.OperationStatusAbnormalValues = layer.OperationStatusAbnormalValues;
            }
            liteLayers.Add(liteLayer);
        }
        File.WriteAllText(layersFilename, serializer.Serialize(liteLayers));

        foreach (var layer in repository.layers.Values)
        {
            if (layer.IsElectrical == false) continue;

            if (!repository.elementsByLayerId.ContainsKey(layer.Id)) continue;
            var counter = 0;
            foreach (var splited in SplitList(repository.elementsByLayerId[layer.Id]))
            {
                counter++;
                var layerElementsFilename = Path.Combine(root, $"Layer_{layer.Code}_{counter}.json");

                var liteLayerElements = new List<LayerElement>();
                foreach (var item in splited)
                {
                    var liteLayerElement = new LayerElement()
                    {
                        Id = item.Id,
                        Activation = item.Activation,
                        Code = item.Code,
                        Version = item.Version,
                        Points = item.Points,
                        FieldValuesText = ""
                    };

                    if (layer.OperationStatusField != null)
                    {
                        liteLayerElement.FieldValuesText = layer.OperationStatusField.GetValue(item.FieldValues);
                    }
                    liteLayerElements.Add(liteLayerElement);
                }
                File.WriteAllText(layerElementsFilename, serializer.Serialize(liteLayerElements));
            }
        }
    }

    public static void Save(GeographyRepository repository, string root, Action<string> logAction)
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
        Directory.CreateDirectory(root);

        var serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        var layersFilename = Path.Combine(root, "Layers.json");
        File.WriteAllText(layersFilename, serializer.Serialize(repository.layers.Values.ToList()));

        foreach (var layer in repository.layers.Values)
        {
            if (!repository.elementsByLayerId.ContainsKey(layer.Id)) continue;
            var counter = 0;
            foreach (var splited in SplitList(repository.elementsByLayerId[layer.Id]))
            {
                counter++;
                var layerElementsFilename = Path.Combine(root, $"Layer_{layer.Code}_{counter}.json");
                File.WriteAllText(layerElementsFilename, serializer.Serialize(splited));
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

    public static T Load<T>(string root, Action<string> logAction) where T : GeographyRepository
    {
        var instance = (T)Activator.CreateInstance(typeof(T), new object[] { logAction });
        var repository = (GeographyRepository)instance;
        var serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };

        logAction?.Invoke($"Initial Repository start");
        repository.BeginInitial();

        logAction?.Invoke($"Load layers & fields");
        var layersFilename = Path.Combine(root, "Layers.json");
        var layers = serializer.Deserialize<List<Layer>>(File.ReadAllText(layersFilename));
        logAction?.Invoke($"Initial Repository> layers & fields");
        repository.Initial(layers);

        var layerfilenames = Directory.GetFiles(root, "Layer_*.json").ToList();
        foreach (var layerfilename in layerfilenames)
        {
            var layerCode = Path.GetFileNameWithoutExtension(layerfilename).Replace("Layer_", "");
            layerCode = layerCode.Substring(0, layerCode.LastIndexOf('_'));
            logAction?.Invoke($"Load elements {layerfilenames.IndexOf(layerfilename) + 1} of {layerfilenames.Count} ({Path.GetFileNameWithoutExtension(layerfilename)})");
            var layerElements = serializer.Deserialize<List<LayerElement>>(File.ReadAllText(layerfilename));
            logAction?.Invoke($"Initial Repository> elements of {layerCode}");
            repository.Initial(layerCode, layerElements);

        }
        repository.EndInitial(null);

        return instance;
    }


}
