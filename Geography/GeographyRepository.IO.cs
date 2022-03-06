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

public partial class GeographyRepository : GeographyRouter.IGeoRepository
{
    public void Save(string root, Action<string> logAction) => Save(this, root, logAction);
    public static void Save(GeographyRepository repository, string root, Action<string> logAction)
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
        Directory.CreateDirectory(root);

        var serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        var layersFilename = Path.Combine(root, "Layers.json");
        File.WriteAllText(layersFilename, serializer.Serialize(repository.layers.Values.ToList()));

        var domainsFilename = Path.Combine(root, "Domains.json");
        File.WriteAllText(domainsFilename, serializer.Serialize(repository.domains.Values.SelectMany(x => x.Values).ToList()));
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

        logAction?.Invoke("Initial Repository start");
        repository.BeginInitial();

        logAction?.Invoke("Load layers & fields");
        var layersFilename = Path.Combine(root, "Layers.json");
        var layers = serializer.Deserialize<List<Layer>>(File.ReadAllText(layersFilename));
        logAction?.Invoke("Initial Repository> layers & fields");
        repository.Initial(layers);

        logAction?.Invoke("Load domains");
        var domainsFilename = Path.Combine(root, "Domains.json");
        var domainValues = serializer.Deserialize<List<DomainValue>>(File.ReadAllText(domainsFilename));
        logAction?.Invoke("Initial Repository> domains");
        repository.Initial(domainValues);

        foreach (var layerfilename in Directory.GetFiles(root, "Layer_*.json"))
        {
            var layerCode = Path.GetFileNameWithoutExtension(layerfilename).Replace("Layer_", "");
            layerCode = layerCode.Substring(0, layerCode.LastIndexOf('_'));
            logAction?.Invoke($"Load elements ({Path.GetFileNameWithoutExtension(layerfilename)})");
            var layerElements = serializer.Deserialize<List<LayerElement>>(File.ReadAllText(layerfilename));
            logAction?.Invoke($"Initial Repository> elements of {layerCode}");
            repository.Initial(layerCode, layerElements);

        }
        repository.EndInitial(null, null, null);

        return instance;
    }


}
