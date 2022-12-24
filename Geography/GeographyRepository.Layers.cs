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
            if (updateResult.Result == false) return updateResult;
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
                Index = layer.Fields.Count(),
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
                Id = input.Id,
                Code = input.Code,
                GeographyType = input.GeographyType,
                Fields = new List<LayerField>(),
            };

            if (layer.Id == Guid.Empty) layer.Id = Guid.NewGuid();
            _layers.Add(layer.Code, layer);
            _elementsByLayerId.Add(layer.Id, new List<LayerElement>());
        }

        layer.Displayname = input.Displayname;
        layer.IsRoutingSource = input.IsRoutingSource;
        layer.IsElectrical = input.IsElectrical;
        layer.IsDisconnector = input.IsDisconnector;
        layer.OperationStatusFieldCode = input.OperationStatusFieldCode;
        layer.OperationStatusAbnormalValues = input.OperationStatusAbnormalValues;
        layer.IsNormalOpen = input.IsNormalOpen;
        layer.ElementDisplaynameFormat = input.ElementDisplaynameFormat;

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
                        Index = layer.Fields.Count(),
                    };
                    layer.Fields.Add(field);
                }
            }
        }

        layer.Reset();
        Save(layer);
        return UpdateResult.Success();
    }

    public List<Layer> Layers => ReadByLock(() => _layers.Values.ToList());
    public Layer GetLayer(string layercode) => ReadByLock(() => getLayerWithoutLock(layercode));
    public Layer GetLayer(Guid layerId) => ReadByLock(() => getLayerWithoutLock(layerId));

    Layer getLayerWithoutLock(string layercode)
    {
        if (_layers.TryGetValue(layercode, out var layer))
            return layer;
        else return null;
    }
    Layer getLayerWithoutLock(Guid layerId)
    {
        return _layers.Values.FirstOrDefault(x => x.Id == layerId);//TODO: add map and reduce
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

    public long GetLayerElementCount(string layerCode) => ReadByLock(() =>
    {
        if (!_layers.TryGetValue(layerCode, out var layer)) return -1;
        if (!_elementsByLayerId.TryGetValue(layer.Id, out var layerElements)) return 0;
        return layerElements.Count;
    });

}