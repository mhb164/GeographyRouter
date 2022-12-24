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
    public UpdateResult Excecute(List<CreateLayerCommand> commands) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        foreach (var command in commands)
        {
            var updateResult = update(command.Createlayer());
            if (updateResult.Result == false) return updateResult;
        }
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(CreateLayerCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        return update(command.Createlayer());
    });

    public UpdateResult Excecute(DeleteLayerCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");


        _layers.Remove(layer.Code);
        Delete(layer);
        return UpdateResult.Success();

    });

    public UpdateResult Excecute(DeleteAllLayersCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        _layers.Clear();
        DeleteAllLayers();
        return UpdateResult.Success();
    });

    public UpdateResult Excecute(MakeLayerAsRoutingSourceCommand command) => WriteByLock(() =>
    {
        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");
        if (layer.IsRoutingSource)
            return UpdateResult.Failed("این لایه هم اکنون به عنوان منبع مسیریابی است!");

        if (!layer.IsElectrical)
            return UpdateResult.Failed("لایه منبع مسیر یابی باید در مسیریابی فعال باشد!");

        foreach (var item in _layers.Values)
        {
            var isRoutingSource = item.Id == command.LayerId;
            if (item.IsRoutingSource == isRoutingSource)
            {
                continue;
            }

            item.IsRoutingSource = isRoutingSource;
            Save(item);
            update(item);
        }

        return UpdateResult.Success();
    });

    public UpdateResult Excecute(UpdateLayerCommand command) => WriteByLock(() =>
    {
        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");

        //------------------------
        var changed = false;
        changed |= command.Displayname != layer.Displayname;
        changed |= command.DisplaynameFormat != layer.ElementDisplaynameFormat;

        if (changed == false)
            return UpdateResult.Failed("در موارد درخواست شده تغییر داده نشده!");
        //------------------------
        if (!layer.CheckDisplaynameFormat(command.DisplaynameFormat, out var errorMessage))
            return UpdateResult.Failed($"قالب نمایش المان ها صحیح نیست ({errorMessage})!");
        //------------------------
        layer.Displayname = command.Displayname;
        layer.ElementDisplaynameFormat = command.DisplaynameFormat;

        Save(layer);

        return update(layer);
    });

    public UpdateResult Excecute(UpdateLayerRoutingCommand command) => WriteByLock(() =>
    {
        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");

        //------------------------
        var changed = false;
        changed |= command.UseInRouting != layer.IsElectrical;
        changed |= command.ConnectivityStateFieldCode != layer.OperationStatusFieldCode;
        changed |= command.Disconnectable != layer.IsDisconnector;
        changed |= !Equals(command.ConnectivityStateAbnormalValues, layer.OperationStatusAbnormalValues);        
        changed |= command.NormalOpen != layer.IsNormalOpen;

        if (changed == false)
            return UpdateResult.Failed("در موارد درخواست شده تغییر داده نشده!");
        //------------------------
        if (layer.IsRoutingSource)
        {
            if (!command.UseInRouting)
                return UpdateResult.Failed("لایه منبع مسیر یابی باید در مسیریابی فعال باشد!");
        }

        if (!string.IsNullOrWhiteSpace(command.ConnectivityStateFieldCode))
        {
            var connectivityStateField = layer.Fields.FirstOrDefault(x => x.Code == command.ConnectivityStateFieldCode);
            if (connectivityStateField == null)
                return UpdateResult.Failed($"فیلدی با کُدِ {command.ConnectivityStateFieldCode} پیدا نشد!");
        }

        //------------------------
        layer.IsElectrical = command.UseInRouting;
        layer.OperationStatusFieldCode = command.ConnectivityStateFieldCode;
        layer.IsDisconnector = command.Disconnectable;
        layer.OperationStatusAbnormalValues = command.ConnectivityStateAbnormalValues;
        layer.IsNormalOpen = command.NormalOpen;

        Save(layer);

        return update(layer);
    });

    private bool Equals(List<string> a, List<string> b)
    {
        if (a is null && b is null) 
            return true;

        if (a is null) 
            return false;

        if (b is null) 
            return false;

        if (a.Count != b.Count) 
            return false;

        foreach (var item in a)
        {
            if (!b.Contains(item))
                return false;
        }
        return true;
    }

    public UpdateResult Excecute(CreateLayerFieldCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");

        var existingField = layer.Fields.FirstOrDefault(x => x.Code == command.Code);
        if (existingField != null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} قبلا ثبت شده است!");

        layer.Fields.Add(new LayerField()
        {
            Code = command.Code,
            Displayname = command.Displayname,
            Index = layer.Fields.Count(),
        });

        Save(layer);
        return update(layer);
    });

    public UpdateResult Excecute(UpdateLayerFieldCommand command) => WriteByLock(() =>
    {
        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");

        var existingField = layer.Fields.FirstOrDefault(x => x.Code == command.Code);
        if (existingField == null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} پیدا نشد!");

        //------------------------
        var changed = false;
        changed |= command.Displayname != existingField.Displayname;

        if (changed == false)
            return UpdateResult.Failed("در موارد درخواست شده تغییر داده نشده!");
        //------------------------

        existingField.Displayname = command.Displayname;

        Save(layer);
        return update(layer);
    });

    public UpdateResult Excecute(DeleteLayerFieldCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        var layer = getLayerWithoutLock(command.LayerId);
        if (layer == null)
            return UpdateResult.Failed("لایه با شناسه درخواست شده وجود ندارد!");

        var existingField = layer.Fields.FirstOrDefault(x => x.Code == command.Code);
        if (existingField == null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} پیدا نشد!");

        if (layer.OperationStatusFieldCode == existingField.Code)
            return UpdateResult.Failed($"امکان حذف فیلد تشخیص وضعیت نیست!");

        layer.Fields.Remove(existingField);
        layer.ReIndexFields();

        Save(layer);
        return update(layer);
    });

    public List<UpdateResult> Excecute(UpdateElementPackageCommand command)
    {
        var result = WriteByLock(() =>
        {
            var layer = getLayerWithoutLock(command.LayerCode);
            if (layer == null)
                return new List<UpdateResult>() { UpdateResult.Failed("لایه با کد درخواست شده وجود ندارد!") };

            var updateResults = new List<UpdateResult>();
            foreach (var item in command.Items)
            {
                var elementFieldValues = new List<string>();
                foreach (var layerField in layer.Fields.OrderBy(x => x.Index))
                {
                    if (command.Descriptors.Contains(layerField.Code))
                    {
                        var index = Array.IndexOf(command.Descriptors, layerField.Code);
                        elementFieldValues.Add(item.DescriptorValues[index]);
                    }
                    else
                        elementFieldValues.Add(string.Empty);
                }

                var element = new LayerElement()
                {
                    Activation = true,
                    Id = Guid.Empty,
                    Code = item.ElementCode,
                    Points = item.Points,
                    FieldValuesText = LayerElement.TranslateFieldValues(elementFieldValues),
                    Version = item.Timetag.Ticks,
                };

                updateResults.Add(update(layer, element));
            }

            return updateResults;

        });

        WaitFlush();
        return result;
    }
}
