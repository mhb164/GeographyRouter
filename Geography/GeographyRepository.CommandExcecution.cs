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
            if (!updateResult.Result) return updateResult;
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

        var layer = getLayerWithoutLock(command.LayerCode);
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
        var layer = getLayerWithoutLock(command.LayerCode);
        if (layer == null)
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

            item.IsRoutingSource = isRoutingSource;
            Save(item);
            update(item);
        }

        return UpdateResult.Success();
    });

    public UpdateResult Excecute(UpdateLayerCommand command) => WriteByLock(() =>
    {
        var layer = getLayerWithoutLock(command.LayerCode);
        if (layer == null)
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        //------------------------
        var changed = false;
        changed |= command.Displayname != layer.Displayname;
        changed |= command.DisplaynameFormat != layer.ElementDisplaynameFormat;
        changed |= command.UseInRouting != layer.IsElectrical;
        changed |= command.Disconnectable != layer.IsDisconnector;

        if (!changed)
            return UpdateResult.Failed("در موارد درخواست شده تغییر داده نشده!");
        //------------------------
        if (!layer.CheckDisplaynameFormat(command.DisplaynameFormat, out var errorMessage))
            return UpdateResult.Failed($"قالب نمایش المان ها صحیح نیست ({errorMessage})!");
        //------------------------
        layer.Displayname = command.Displayname;
        layer.ElementDisplaynameFormat = command.DisplaynameFormat;
        layer.IsElectrical = command.UseInRouting;
        layer.IsDisconnector = command.Disconnectable;

        Save(layer);

        return update(layer);
    });
  
    public UpdateResult Excecute(CreateLayerFieldCommand command) => WriteByLock(() =>
    {
        if (isStructureLocked)
            return UpdateResult.Failed(StructureLockedErrorMessage);

        var layer = getLayerWithoutLock(command.LayerCode);
        if (layer == null)
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        var existingField = layer.Fields.FirstOrDefault(x => x.Code == command.Code);
        if (existingField != null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} قبلا ثبت شده است!");

        layer.Fields.Add(new LayerField()
        {
            Code = command.Code,
            Displayname = command.Displayname,
            Index = layer.Fields.Count,
        });

        Save(layer);
        return update(layer);
    });

    public UpdateResult Excecute(UpdateLayerFieldCommand command) => WriteByLock(() =>
    {
        var layer = getLayerWithoutLock(command.LayerCode);
        if (layer == null)
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        var existingField = layer.Fields.FirstOrDefault(x => x.Code == command.Code);
        if (existingField == null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} پیدا نشد!");

        //------------------------
        var changed = false;
        changed |= command.Displayname != existingField.Displayname;

        if (!changed)
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

        var layer = getLayerWithoutLock(command.LayerCode);
        if (layer == null)
            return UpdateResult.Failed("لایه با کُدِ درخواست شده وجود ندارد!");

        var existingField = layer.Fields.FirstOrDefault(x => x.Code == command.Code);
        if (existingField == null)
            return UpdateResult.Failed($"فیلد با کُدِ {command.Code} پیدا نشد!");

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

    public List<UpdateResult> Excecute(DeleteElementsCommand command)
    {
        var result = WriteByLock(() =>
        {
            var updateResults = new List<UpdateResult>();
            foreach (var elementCode in command.ElementCodes)
            {
                updateResults.Add(deleteElement(command.LayerCode, elementCode, command.Timetag.Ticks));
            }

            return updateResults;
        });

        WaitFlush();
        return result;
    }
}
