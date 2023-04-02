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
    public long ElementsCount => ReadByLock(() => _elements.Count);

    public void Initial(string layerCode, List<LayerElement> layerElements)
    {
        if (!_layers.TryGetValue(layerCode, out var layer))
            return;//TODO: Register Error

        foreach (var item in layerElements)
            update(layer, item, false);
    }

    public UpdateResult Update(Layer layer, LayerElement input) => WriteByLock(() => update(layer, input));
    private UpdateResult update(Layer layer, LayerElement input, bool logVersion = true)
    {
        if (_elements.TryGetValue(input.Code, out var element))
        {
            if (element.Layer.Code != layer.Code) return UpdateResult.Failed($"[{layer.Code}-{input.Code}] UpdateElement(Layer mismatch!)");
            if (element.Version > input.Version) return UpdateResult.Failed($"[{layer.Code}-{input.Code}] UpdateElement(Version passed!)");
            ElecricalMatrix.Remove(element);
        }
        else
        {
            element = new LayerElement()
            {
                Code = input.Code,
                Points = new double[] { },
                FieldValuesText = "",
                Version = 0,
            };
            element.Reset(layer);

            _elements.Add(element.Code, element);
            _elementsByLayerCode[element.Layer.Code].Add(element);
        }
        //------------------

        element.Points = input.Points;
        element.FieldValuesText = input.FieldValuesText;
        element.Version = input.Version;
        updateVersion(element.Version, logVersion);

        if (element.Layer.IsElectrical && (element.Layer.GeographyType == LayerGeographyType.Point || element.Layer.GeographyType == LayerGeographyType.Polyline))
            ElecricalMatrix.Add(element);

        element.ResetDisplayname();
        Save(element);
        return UpdateResult.Success($"[{layer.Code}-{input.Code}] updated.");
    }

    private UpdateResult deleteElement(string layerCode, string elementCode, long requestVersion, bool logVersion = false)
    {
        if (!_layers.TryGetValue(layerCode, out var layer))
            return UpdateResult.Failed($"[{layerCode}-{elementCode}] DeleteElement(Layer not found!)");

        if (!_elements.TryGetValue(elementCode, out var element))
            return UpdateResult.Failed($"[{layerCode}-{elementCode}] DeleteElement(elementnot found!)");

        if (element.Layer.Code != layer.Code)
            return UpdateResult.Failed($"RemoveElement(Layer mismatch!)");

        if (element.Version > requestVersion)
            return UpdateResult.Failed($"[{layerCode}-{elementCode}] DeleteElement(Version passed!)");

        _elements.Remove(element.Code);
        _elementsByLayerCode[element.Layer.Code].Remove(element);
        ElecricalMatrix.Remove(element);

        updateVersion(element.Version, logVersion);
        Delete(element);
        return UpdateResult.Success();
    }

    public LayerElement this[string code] => GetElement(code);

    public LayerElement GetElement(string code)
    {
        if (_elements.TryGetValue(code, out var element))
            return element;
        else
            return null;
    }

    public List<LayerElement> this[IEnumerable<string> codes] => GetElements(codes);

    public List<LayerElement> GetElements(IEnumerable<string> codes)
    {
        var result = new List<LayerElement>();
        foreach (var code in codes.Distinct())
        {
            if (_elements.TryGetValue(code, out var element))
                result.Add(element);
        }
        return result;
    }

    public IEnumerable<LayerElement> GetElements(IEnumerable<Layer> owners, long version)
    {
        foreach (var owner in owners)
        {
            if (!_elementsByLayerCode.TryGetValue(owner.Code, out var layerElements))
                continue;

            foreach (var item in layerElements)
            {
                if (version > item.Version) continue;
                yield return item;
            }
        }

    }

    public IEnumerable<LayerElement> GetElements(Layer owner)
    {
        if (!_elementsByLayerCode.TryGetValue(owner.Code, out var layerElements))
            return LayerElement.EmptyList;

        return layerElements;
    }

    public int GetElementsCount(Layer owner)
    {
        if (!_elementsByLayerCode.TryGetValue(owner.Code, out var layerElements))
            return 0;

        return layerElements.Count;
    }

}