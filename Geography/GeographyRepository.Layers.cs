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
    Dictionary<string, Layer> layers = new Dictionary<string, Layer>();
    //Dictionary<Guid, LayerElementsMatrix> layersMatrix = new Dictionary<Guid, LayerElementsMatrix>();
    LayerElementsMatrix ElecricalMatrix;
    public void Initial(List<Layer> layers)
    {
        foreach (var item in layers) update(item);
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
        if (layers.ContainsKey(layercode) == false) return UpdateResult.Failed($"UpdateLayer(Code:{layercode}) not exists!");
        var layer = layers[layercode];
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
        var layer = default(Layer);
        if (layers.ContainsKey(input.Code)) layer = layers[input.Code];
        else
        {
            layer = new Layer()
            {
                Id = input.Id,
                Code = input.Code,
                GeographyType = input.GeographyType,
                Fields = new List<LayerField>(),
            };
            if (layer.Id == Guid.Empty) layer.Id = Guid.NewGuid();
            layers.Add(layer.Code, layer);

            //if (layer.GeographyType == LayerGeographyType.Point || layer.GeographyType == LayerGeographyType.Polyline)
            //    layersMatrix.Add(layer.Id, new LayerElementsMatrixByPoint(GetElement));
            //else if (layer.GeographyType == LayerGeographyType.Polygon)
            //    layersMatrix.Add(layer.Id, new LayerElementsMatrixByPoint(GetElement));
            //else
            //{

            //}

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

    public List<Layer> Layers => ReadByLock(() => layers.Values.ToList());
    public Layer GetLayer(string layercode) => ReadByLock(() => getLayerWithoutLock(layercode));
    public Layer GetLayer(Guid layerId) => ReadByLock(() => getLayerWithoutLock(layerId));

    Layer getLayerWithoutLock(string layercode)
    {
        if (layers.ContainsKey(layercode)) return layers[layercode];
        else return null;
    }
    Layer getLayerWithoutLock(Guid layerId)
    {
        return layers.Values.FirstOrDefault(x => x.Id == layerId);//TODO: add map and reduce
    }

    public List<Layer> GetLayers(IEnumerable<string> layercodes) => ReadByLock(() =>
    {
        var result = new List<Layer>();
        foreach (var layercode in layercodes)
        {
            if (layers.ContainsKey(layercode))
                result.Add(layers[layercode]);
        }
        return result;
    });
    public List<string> LayersCodes => ReadByLock(() => layers.Keys.ToList());
    public List<string> DisconnectorLayersCodes => ReadByLock(() => layers.Where(x => x.Value.IsDisconnector).Select(x => x.Key).ToList());

    public long GetLayerElementCount(string layerCode) => ReadByLock(() =>
    {
        if (layers.ContainsKey(layerCode) == false) return -1;
        else
        {
            var layer = layers[layerCode];
            if (elementsByLayerId.ContainsKey(layer.Id)) return elementsByLayerId[layer.Id].Count;
            else return 0;
        }
    });

}