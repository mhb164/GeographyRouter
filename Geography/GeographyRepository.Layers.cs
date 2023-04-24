using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public partial class GeographyRepository
{
    public void Initial(List<Layer> layers)
    {
        foreach (var item in layers)
            update(item);
    }

    public UpdateResult Update(Layer input) => WriteByLock(() => update(input));
    public UpdateResult Update(List<Layer> input) => WriteByLock(() =>
    {
        foreach (var item in input)
        {
            var updateResult = update(item);
            if (!updateResult.Result) return updateResult;
        }
        return UpdateResult.Success();
    });

    public UpdateResult Update(string layercode, LayerField inputField) => WriteByLock(() =>
    {
        if (!_layers.TryGetValue(layercode, out var layer))
        {
            return UpdateResult.Failed($"UpdateLayer(Code:{layercode}) not exists!");
        }

        var field = layer.Fields.FirstOrDefault(x => x.Code == inputField.Code);
        if (field != null)
        {
            field.Displayname = inputField.Displayname;
        }
        else
        {
            field = new LayerField()
            {
                Code = inputField.Code,
                Displayname = inputField.Displayname,
                Index = layer.Fields.Count,
            };
            layer.Fields.Add(field);
        }

        Save(layer);
        return UpdateResult.Success();
    });

    private UpdateResult update(Layer input)
    {
        if (!_layers.TryGetValue(input.Code, out var layer))
        {
            layer = new Layer()
            {
                Code = input.Code,
                GeographyType = input.GeographyType,
                Fields = new List<LayerField>(),
            };
            _layers.Add(layer.Code, layer);
            _elementsByLayerCode.Add(layer.Code, new Dictionary<string, LayerElement>());
        }

        layer.Displayname = input.Displayname;
        layer.ElementDisplaynameFormat = input.ElementDisplaynameFormat;
        layer.IsRoutingSource = input.IsRoutingSource;
        layer.IsElectrical = input.IsElectrical;
        layer.IsDisconnector = input.IsDisconnector;

        if (input.Fields != null)
        {
            foreach (var inputField in input.Fields)
            {
                var field = layer.Fields.FirstOrDefault(x => x.Code == inputField.Code);
                if (field != null)
                {
                    field.Displayname = inputField.Displayname;
                }
                else
                {
                    field = new LayerField()
                    {
                        Code = inputField.Code,
                        Displayname = inputField.Displayname,
                        Index = layer.Fields.Count,
                    };
                    layer.Fields.Add(field);
                }
            }
        }

        Save(layer);
        return UpdateResult.Success();
    }

    public List<Layer> Layers => ReadByLock(() => _layers.Values.ToList());
    public Layer GetLayer(string layercode) => ReadByLock(() => getLayerWithoutLock(layercode));

    Layer getLayerWithoutLock(string layercode)
    {
        if (_layers.TryGetValue(layercode, out var layer))
            return layer;
        else return null;
    }

    public List<Layer> GetLayers(IEnumerable<string> layerCodes) => ReadByLock(() =>
    {
        var result = new List<Layer>();
        foreach (var layerCode in layerCodes)
        {
            if (_layers.TryGetValue(layerCode, out var layer))
                result.Add(layer);
        }
        return result;
    });
    public List<string> LayersCodes => ReadByLock(() => _layers.Keys.ToList());
    public List<string> DisconnectorLayersCodes => ReadByLock(() => _layers.Where(x => x.Value.IsDisconnector).Select(x => x.Key).ToList());

    public long GetLayerElementCount(string layerCode) => ReadByLock(() => getLayerElementCount(layerCode));
    public long GetElementsCount(Layer layer) => ReadByLock(() => getLayerElementCount(layer.Code));
    public long getLayerElementCount(string layerCode)
    {
        if (!_layers.TryGetValue(layerCode, out var layer)) return -1;
        if (!_elementsByLayerCode.TryGetValue(layer.Code, out var layerElements)) return 0;
        return layerElements.Count;
    }

    public IEnumerable<LayerElement> GetLayerElements(string layerCode) => ReadByLock(() => getLayerElements(layerCode));
    public IEnumerable<LayerElement> GetElements(Layer layer) => ReadByLock(() => getLayerElements(layer.Code));
    public List<LayerElement> getLayerElements(string layerCode)
    {
        if (!_elementsByLayerCode.TryGetValue(layerCode, out var layerElements))
            return LayerElement.EmptyList;

        return layerElements.Values.ToList();
    }

    public List<string> GetLayerElementCodes(string layerCode) => ReadByLock(() => getLayerElementCodes(layerCode));
    public List<string> getLayerElementCodes(string layerCode)
    {
        if (!_elementsByLayerCode.TryGetValue(layerCode, out var layerElements))
            return new List<string>();

        return layerElements.Keys.ToList();
    }
}