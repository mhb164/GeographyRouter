using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public interface IGeographyRepositoryStorage
{
    bool Save(Layer item);
    bool Delete(Layer item);
    bool DeleteAllLayers();
    bool Save(LayerElement item);

    void WaitFlush();
}

public partial class GeographyRepository : GeographyRouter.IGeoRepository
{
    readonly Action<string> LogAction;
    public GeographyRepository(Action<string> logAction)
    {
        LogAction = logAction;
        Log("Created");
    }
    protected void Log(string message) => LogAction?.Invoke(message);

    IGeographyRepositoryStorage Storage;
    private void Save(Layer item) => Storage?.Save(item);
    private void Delete(Layer item) => Storage?.Delete(item);
    private void DeleteAllLayers() => Storage?.DeleteAllLayers();
    private void Save(LayerElement item) => Storage?.Save(item);
    private void WaitFlush() => Storage?.WaitFlush();

    public void BeginInitial()
    {
        layers.Clear();
        //layersMatrix.Clear();
        ElecricalMatrix = new LayerElementsMatrixByPoint(GetElement);
        //---------------------
        elements.Clear();
        elementsById.Clear();
        elementsByLayerId.Clear();
        //---------------------
        version = 0;
        versionChangeRequestStopwatch.Restart();
        //---------------------
        Storage = null;
    }
    public void EndInitial(IGeographyRepositoryStorage storage)
    {
        Storage = storage;
        AfterInitialFinished();
    }

    protected virtual void AfterInitialFinished() { }

    #region Version
    long version = 0;
    long versionChangeRequestd = 0;
    Stopwatch versionChangeRequestStopwatch = Stopwatch.StartNew();

    public long Version => ReadByLock(() => version);
    public string VersionAsTimeText => $"{new DateTime(Version):yyyy-MM-dd HH:mm:ss.fff}";
    private void updateVersion(long versionRequestd, bool log = true)
    {
        var newVersion = Math.Max(version, versionRequestd);
        versionChangeRequestd = versionRequestd;
        versionChangeRequestStopwatch.Restart();
        if (version == newVersion) return;
        version = newVersion;
        if (log) Log($"Repository Version Changed: {version} == {new DateTime(version):yyyy-MM-dd HH:mm:ss.fff}");
    }
    public void GetVersion(out long version, out long versionRequested, out long changeElapsedMilliseconds)
    {
        long versionMiror = 0;
        long versionRequestedMiror = 0;
        long changeElapsedMillisecondsMiror = 0;
        ReadByLock(() =>
        {
            versionMiror = this.version;
            versionRequestedMiror = versionChangeRequestd;
            changeElapsedMillisecondsMiror = versionChangeRequestStopwatch.ElapsedMilliseconds;
        });
        version = versionMiror;
        versionRequested = versionRequestedMiror;
        changeElapsedMilliseconds = changeElapsedMillisecondsMiror;
    }
    #endregion Version

    const string StructureLockedErrorMessage = "اطلاعات المان‌ها ثبت شده و امکان تغییر ساختاری وجود ندارد!";

    public bool StructureLocked => ReadByLock(() => isStructureLocked);
    private bool isStructureLocked => elements.Count > 0;

    #region Layers
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
    #endregion Layers

    #region Elements
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
    #endregion Elements
}
