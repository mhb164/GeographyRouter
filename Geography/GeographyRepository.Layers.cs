using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public partial class GeographyRepository
{
    public void Initial(IEnumerable<Layer> layers)
    {
        foreach (var item in layers)
        {
            if (_layers.TryGetValue(item.Code, out var layer))
                continue;

            _layers.Add(item.Code, item);
            _elementsByLayerCode.Add(item.Code, new Dictionary<string, LayerElement>());
        }
    }

    public UpdateResult Excecute(List<CreateLayerCommand> commands) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        foreach (var command in commands)
        {
            var updateResult = ExcecuteWithoutLock(command);
            if (!updateResult.Result) return updateResult;
        }
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(CreateLayerCommand command) => WriteByLock(() => ExcecuteWithoutLock(command));

    private UpdateResult ExcecuteWithoutLock(CreateLayerCommand command)
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        if (_layers.TryGetValue(command.Code, out var layer))
            return UpdateResult.Failed($"لایه {layer} وجود دارد!");


        layer = new Layer(command.Code, command.Displayname, command.GeographyType);
        foreach (var field in command.Fields)
            layer.AddFiled(field.Code, field.Displayname);

        _layers.Add(layer.Code, layer);
        _elementsByLayerCode.Add(layer.Code, new Dictionary<string, LayerElement>());

        Save(layer);
        return UpdateResult.Success();
    }

    public UpdateResult Excecute(CreateLayerFieldCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        if (!_layers.TryGetValue(command.LayerCode, out var layer))
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        var existingField = layer.GetField(command.Code);
        if (existingField != null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} قبلا ثبت شده است!");

        layer.AddFiled(command.Code, command.Displayname);

        Save(layer);

        return UpdateResult.Success();
    });

    public UpdateResult Excecute(UpdateLayerFieldCommand command) => WriteByLock(() =>
    {
        if (!_layers.TryGetValue(command.LayerCode, out var layer))
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        var existingField = layer.GetField(command.Code);
        if (existingField == null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} پیدا نشد!");

        var changed = false;
        changed |= command.Displayname != existingField.Displayname;

        if (!changed)
            return UpdateResult.Failed("در موارد درخواست شده تغییر داده نشده!");

        existingField.UpdateDisplayname(command.Displayname);

        Save(layer);
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(DeleteLayerFieldCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        if (!_layers.TryGetValue(command.LayerCode, out var layer))
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        var existingField = layer.GetField(command.Code);
        if (existingField == null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} پیدا نشد!");

        layer.RemoveFiled(command.Code);

        Save(layer);
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(DeleteLayerCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        if (!_layers.TryGetValue(command.LayerCode, out var layer))
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        _layers.Remove(layer.Code);
        _elementsByLayerCode.Remove(layer.Code);

        Delete(layer);
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(DeleteAllLayersCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        _layers.Clear();
        _elementsByLayerCode.Clear();

        DeleteAllLayers();
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(MakeLayerAsRoutingSourceCommand command)
    {
        var routingChanged = false;

        var updateResult = WriteByLock(() =>
        {
            if (!_layers.TryGetValue(command.LayerCode, out var layer))
                return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

            if (layer.IsRoutingSource)
                return UpdateResult.Failed("این لایه هم اکنون به عنوان منبع مسیریابی است!");

            if (!layer.IsElectrical)
                return UpdateResult.Failed("لایه منبع مسیر یابی باید در مسیریابی فعال باشد!");

            foreach (var item in _layers.Values)
            {
                var isRoutingSource = item.Code == command.LayerCode;
                if (item.IsRoutingSource == isRoutingSource)
                {
                    continue;
                }

                routingChanged |= true;
                item.UpdateIsRoutingSource(isRoutingSource);
                Save(item);
            }

            return UpdateResult.Success();
        });

        if (updateResult.Result && routingChanged)
            FireRoutingChangeDetected();

        return updateResult;
    }

    public UpdateResult Excecute(UpdateLayerCommand command)
    {
        var routingChanged = false;
        var elementDisplaynameFormatChanged = false;

        var updateResult = WriteByLock(() =>
        {
            if (!_layers.TryGetValue(command.LayerCode, out var layer))
                return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

            var changed = false;
            changed |= command.Displayname != layer.Displayname;

            var elementDisplaynameFormatChanged = command.DisplaynameFormat != layer.ElementDisplaynameFormat;
            changed |= elementDisplaynameFormatChanged;

            routingChanged |= command.UseInRouting != layer.IsElectrical;
            routingChanged |= command.Disconnectable != layer.IsDisconnector;
            changed |= routingChanged;

            if (!changed)
                return UpdateResult.Failed("در موارد درخواست شده تغییر داده نشده!");

            if (!layer.CheckDisplaynameFormat(command.DisplaynameFormat, out var errorMessage))
                return UpdateResult.Failed($"قالب نمایش المان ها صحیح نیست ({errorMessage})!");

            layer.Update(command.Displayname, command.DisplaynameFormat, command.UseInRouting, command.Disconnectable);

            Save(layer);

            return UpdateResult.Success();
        });

        if (updateResult.Result && routingChanged)
            FireRoutingChangeDetected();

        if (updateResult.Result && elementDisplaynameFormatChanged)
            ResetElementsDisplayname(command.LayerCode);

        return updateResult;
    }

    private void ResetElementsDisplayname(string layerCode) => new Task(() =>
    {
        WriteByLock(() =>
        {
            if (!_elementsByLayerCode.TryGetValue(layerCode, out var layerElements))
                return;

            foreach (var item in layerElements.Values)
            {
                item.MakeDisplaynameNull();
            }
        });
    }).Start();

    public List<Layer> Layers => ReadByLock(() => _layers.Values.ToList());
    public Layer GetLayer(string layercode) => ReadByLock(() =>
    {
        if (_layers.TryGetValue(layercode, out var layer))
            return layer;

        return null;
    });

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