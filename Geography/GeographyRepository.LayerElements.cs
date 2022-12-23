using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class GeographyRepository : GeographyRouter.IGeoRepository
{
    public long ElementsCount => ReadByLock(() => elements.Count);
    Dictionary<string, LayerElement> elements = new Dictionary<string, LayerElement>();
    Dictionary<Guid, LayerElement> elementsById = new Dictionary<Guid, LayerElement>();
    Dictionary<Guid, List<LayerElement>> elementsByLayerId = new Dictionary<Guid, List<LayerElement>>();

    public void Initial(string layerCode, List<LayerElement> layerElements)
    {
        if (layers.ContainsKey(layerCode) == false) return;//TODO: Register Error
        var layer = layers[layerCode];
        foreach (var item in layerElements) update(layer, item, false);
    }

    public UpdateResult Update(Layer layer, LayerElement input) => WriteByLock(() => update(layer, input));
    private UpdateResult update(Layer layer, LayerElement input, bool logVersion = true)
    {
        var element = default(LayerElement);
        if (elements.ContainsKey(input.Code))
        {
            element = elements[input.Code];
            if (element.Layer.Id != layer.Id) return UpdateResult.Failed($"[{layer.Code}-{input.Code}] UpdateElement(Layer mismatch!)");
            if (element.Version > input.Version) return UpdateResult.Failed($"[{layer.Code}-{input.Code}] UpdateElement(Version passed!)");
            //layersMatrix[element.Layer.Id].Remove(element);
            ElecricalMatrix.Remove(element);
        }
        else
        {
            element = new LayerElement()
            {
                Activation = true,
                Id = input.Id,
                Code = input.Code,
                Points = new double[] { },
                FieldValuesText = "",
                Version = 0,
            };
            element.Reset(layer);
            if (element.Id == Guid.Empty) element.Id = Guid.NewGuid();
            elements.Add(element.Code, element);
            elementsById.Add(element.Id, element);
            if (elementsByLayerId.ContainsKey(element.Layer.Id) == false) elementsByLayerId.Add(element.Layer.Id, new List<LayerElement>());
            elementsByLayerId[element.Layer.Id].Add(element);
        }
        //------------------
        element.Activation = input.Activation;
        element.Points = input.Points;
        element.FieldValuesText = input.FieldValuesText;
        element.Version = input.Version;
        updateVersion(element.Version, logVersion);

        if (element.Activation)
        {
            // layersMatrix[element.Layer.Id].Add(element);
            if (element.Layer.IsElectrical && (element.Layer.GeographyType == LayerGeographyType.Point || element.Layer.GeographyType == LayerGeographyType.Polyline))
                ElecricalMatrix.Add(element);
        }

        element.ResetDisplayname();
        Save(element);
        return UpdateResult.Success($"[{layer.Code}-{input.Code}] updated.");
    }

    public UpdateResult RemoveElement(string layercode, string elementcode, long requestVersion) => WriteByLock(() =>
    {
        if (layers.ContainsKey(layercode) == false) return UpdateResult.Failed($"RemoveElement(LayerCode:{layercode}) not exists!");
        var layer = layers[layercode];
        if (elements.ContainsKey(elementcode) == false) return UpdateResult.Failed($"RemoveElement(LayerCode:{layercode},ElementCode:{elementcode}) not exists!");
        var element = elements[elementcode];
        if (element.Layer.Id != layer.Id) return UpdateResult.Failed($"RemoveElement(Layer mismatch!)");
        if (element.Version > requestVersion) return UpdateResult.Failed($"RemoveElement(Version passed!)");
        if (element.Activation == false) return UpdateResult.Failed($"RemoveElement(Already removed!)");

        //layersMatrix[element.Layer.Id].Remove(element);
        element.Activation = false;
        Save(element);
        return UpdateResult.Success();
    });

    public LayerElement this[string code] => GetElement(code);

    public LayerElement GetElement(string code)
    {

        if (elements.ContainsKey(code)) return elements[code];
        else return null;
    }

    public List<LayerElement> this[IEnumerable<string> codes] => GetElements(codes);

    public List<LayerElement> GetElements(IEnumerable<string> codes)
    {
        var result = new List<LayerElement>();
        foreach (var code in codes.Distinct())
        {
            if (elements.ContainsKey(code))
                result.Add(elements[code]);

        }
        return result;
    }

    public LayerElement GetElement(Guid id)
    {
        if (elementsById.ContainsKey(id)) return elementsById[id];
        else return null;
    }

    public IEnumerable<LayerElement> GetElements(IEnumerable<Layer> owners, long version)
    {
        foreach (var owner in owners)
        {
            if (elementsByLayerId.ContainsKey(owner.Id) == false) continue;
            foreach (var item in elementsByLayerId[owner.Id])
            {
                if (version > item.Version) continue;
                yield return item;
            }
        }

    }

    public IEnumerable<LayerElement> GetElements(Layer owner)
    {
        if (elementsByLayerId.ContainsKey(owner.Id) == false) return new List<LayerElement>();
        return elementsByLayerId[owner.Id];
    }

    public int GetElementsCount(Layer owner)
    {
        if (elementsByLayerId.ContainsKey(owner.Id) == false) return 0;
        return elementsByLayerId[owner.Id].Count;
    }
    //internal IEnumerable<LayerElement> HitTest(IEnumerable<Guid> LayerIds, double Latitude, double Longitude)
    //{
    //    var result = new List<LayerElement>();
    //    foreach (var matrix in layersMatrix.Where(x => LayerIds.Contains(x.Key)))
    //        matrix.Value.HitTest(Latitude, Longitude, ref result);
    //    return result;
    //}

}