using GeographyModel;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

public partial class GeographyRepository : GeographyRouter.IGeoRepository
{
    public long ElementsCount => ReadByLock(() => _elements.Count);

    public void Initial(IEnumerable<LayerElement> layerElements)
    {
        foreach (var element in layerElements)
        {
            if (_elements.TryGetValue(element.Code, out var existing))
                continue;

            _elements.Add(element.Code, element);
            _elementsByLayerCode[element.Layer.Code].Add(element.Code, element);

            updateVersion(element.DataVersion, element.StatusVersion, false);

            if (element.Layer.IsElectrical && (element.Layer.GeographyType == LayerGeographyType.Point || element.Layer.GeographyType == LayerGeographyType.Polyline))
                ElecricalMatrix.Add(element);
        }
    }

    public UpdateElementResult Excecute(CreateUpdateElementCommand command)
    {
        var result = WriteByLock(() => ExcecuteWithoutLock(command));

        if (result.Result)
            WaitFlush();

        return result;
    }

    public UpdateElementResult ExcecuteWithoutLock(CreateUpdateElementCommand command)
    {
        if (!_layers.TryGetValue(command.LayerCode, out var layer))
            return UpdateElementResult.Failed("لایه با کد درخواست شده وجود ندارد!");

        var elementFieldValues = new string[layer.Fields.Count()];
        foreach (var layerField in layer.Fields.OrderBy(x => x.Index))
        {
            if (command.LayerDescriptors.Contains(layerField.Code))
            {
                var index = Array.IndexOf(command.LayerDescriptors, layerField.Code);
                elementFieldValues[layerField.Index] = command.DescriptorValues[index];
            }
            else
                elementFieldValues[layerField.Index] = string.Empty;
        }

        if (_elements.TryGetValue(command.ElementCode, out var existingElement))
            return UpdateWithoutLock(layer,
                                     existingElement,
                                     command.Timetag.Ticks,
                                     in command.Points,
                                     in elementFieldValues,
                                     command.NormalStatus,
                                     command.ActualStatus,
                                     command.StatusSource);
        else
            return CreateWithoutLock(layer,
                                     command.ElementCode,
                                     command.Timetag.Ticks,
                                     in command.Points,
                                     in elementFieldValues,
                                     command.NormalStatus,
                                     command.ActualStatus,
                                     command.StatusSource);
    }

    private UpdateElementResult UpdateWithoutLock(Layer layer,
                                           LayerElement element,
                                           long updatedVersion,
                                           in double[] points,
                                           in string[] elementFieldValues,
                                           LayerElementStatus normalStatus,
                                           LayerElementStatus actualStatus,
                                           LayerElementStatusSource statusSource)
    {
        if (element.Layer.Code != layer.Code)
            return UpdateElementResult.Failed($"[{layer.Code}-{element.Code}] UpdateElement(Layer mismatch!)");

        if (element.DataVersion > updatedVersion && element.StatusVersion > updatedVersion)
            return UpdateElementResult.Failed($"[{layer.Code}-{element.Code}] UpdateElement(Version passed!)");

        var pointsChanged = element.CheckPointsChange(points);
        var fieldValuesChanged = element.CheckFieldValuesChange(elementFieldValues);
        var statusChanged = element.CheckStatusChange(normalStatus, actualStatus, element.StatusComment);

        var changed = pointsChanged || fieldValuesChanged || statusChanged;

        if (!changed)
            return UpdateElementResult.Failed("در موارد درخواست شده تغییر داده نشده!");

        ElecricalMatrix.Remove(element);

        if (pointsChanged || fieldValuesChanged && element.DataVersion <= updatedVersion)
            element.UpdateData(points, elementFieldValues, updatedVersion);

        if (statusChanged && element.StatusVersion <= updatedVersion)
            element.UpdateStatus(normalStatus, actualStatus, statusSource, string.Empty, updatedVersion);

        if (element.Layer.IsElectrical && (element.Layer.GeographyType == LayerGeographyType.Point || element.Layer.GeographyType == LayerGeographyType.Polyline))
            ElecricalMatrix.Add(element);

        updateVersion(element.DataVersion, element.StatusVersion);
        Save(element);
        return UpdateElementResult.Success(pointsChanged, fieldValuesChanged, statusChanged, $"[{layer.Code}-{element.Code}] updated.");

    }

    private UpdateElementResult CreateWithoutLock(Layer layer,
                                           string elementCode,
                                           long createVersion,
                                           in double[] points,
                                           in string[] elementFieldValues,
                                           LayerElementStatus normalStatus,
                                           LayerElementStatus actualStatus,
                                           LayerElementStatusSource statusSource)
    {
        var element = new LayerElement(layer,
                                       elementCode,
                                       points,
                                       elementFieldValues,
                                       createVersion,
                                       normalStatus,
                                       actualStatus,
                                       statusSource,
                                       string.Empty,
                                       createVersion);

        _elements.Add(element.Code, element);
        _elementsByLayerCode[element.Layer.Code].Add(element.Code, element);

        if (element.Layer.IsElectrical && (element.Layer.GeographyType == LayerGeographyType.Point || element.Layer.GeographyType == LayerGeographyType.Polyline))
            ElecricalMatrix.Add(element);

        updateVersion(element.DataVersion, element.StatusVersion);
        Save(element);
        return UpdateElementResult.Success(true, true, true, $"[{layer.Code}-{element.Code}] created.");
    }

    public UpdateElementResult Excecute(UpdateElementDataCommand command)
    {
        var result = WriteByLock(() => ExcecuteWithoutLock(command));

        if (result.Result)
            WaitFlush();

        return result;
    }

    public UpdateElementResult ExcecuteWithoutLock(UpdateElementDataCommand command)
    {
        if (!_layers.TryGetValue(command.LayerCode, out var layer))
            return UpdateElementResult.Failed("لایه با کد درخواست شده وجود ندارد!");

        if (!_elements.TryGetValue(command.ElementCode, out var element))
            return UpdateElementResult.Failed("المان با کُدِ درخواست شده وجود ندارد!");

        if (element.Layer.Code != layer.Code)
            return UpdateElementResult.Failed($"[{layer.Code}-{element.Code}] UpdateElement(Layer mismatch!)");

        var updatedVersion = command.Timetag.Ticks;
        if (element.DataVersion > updatedVersion)
            return UpdateElementResult.Failed($"[{layer.Code}-{element.Code}] UpdateElement(DataVersion passed!)");

        var elementFieldValues = new string[layer.Fields.Count()];
        foreach (var layerField in layer.Fields.OrderBy(x => x.Index))
        {
            if (command.LayerDescriptors.Contains(layerField.Code))
            {
                var index = Array.IndexOf(command.LayerDescriptors, layerField.Code);
                elementFieldValues[layerField.Index] = command.DescriptorValues[index];
            }
            else
                elementFieldValues[layerField.Index] = string.Empty;
        }

        var pointsChanged = element.CheckPointsChange(command.Points);
        var fieldValuesChanged = element.CheckFieldValuesChange(elementFieldValues);

        var changed = pointsChanged || fieldValuesChanged;

        if (!changed)
            return UpdateElementResult.Failed("در موارد درخواست شده تغییر داده نشده!");

        ElecricalMatrix.Remove(element);

        element.UpdateData(command.Points, elementFieldValues, updatedVersion);

        if (element.Layer.IsElectrical && (element.Layer.GeographyType == LayerGeographyType.Point || element.Layer.GeographyType == LayerGeographyType.Polyline))
            ElecricalMatrix.Add(element);

        updateVersion(element.DataVersion);
        Save(element);
        return UpdateElementResult.Success(pointsChanged, fieldValuesChanged, false, $"[{layer.Code}-{element.Code}] Data updated.");

    }

    public UpdateElementResult Excecute(UpdateElementStatusCommand command)
    {
        var result = WriteByLock(() => ExcecuteWithoutLock(command));

        if (result.Result)
            WaitFlush();

        return result;
    }

    public UpdateElementResult ExcecuteWithoutLock(UpdateElementStatusCommand command)
    {
        if (!_elements.TryGetValue(command.ElementCode, out var element))
            return UpdateElementResult.Failed("المان با کُدِ درخواست شده وجود ندارد!");

        var updatedVersion = command.Timetag.Ticks;
        if (element.StatusVersion > updatedVersion)
            return UpdateElementResult.Failed($"[{element.Layer.Code}-{element.Code}] UpdateElement(StatusVersion passed!)");

        var statusChanged = element.CheckStatusChange(command.NormalStatus, command.ActualStatus, command.StatusComment);

        if (!statusChanged)
            return UpdateElementResult.Failed("در موارد درخواست شده تغییر داده نشده!");

        element.UpdateStatus(command.NormalStatus, command.ActualStatus, command.StatusSource, command.StatusComment, updatedVersion);

        updateVersion(element.StatusVersion);
        Save(element);

        return UpdateElementResult.Success(false, false, true, $"[{element.Layer.Code}-{element.Code}] Data updated.");

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

    private UpdateResult deleteElement(string layerCode, string elementCode, long requestVersion, bool logVersion = false)
    {
        if (!_layers.TryGetValue(layerCode, out var layer))
            return UpdateResult.Failed($"[{layerCode}-{elementCode}] DeleteElement(Layer not found!)");

        if (!_elements.TryGetValue(elementCode, out var element))
            return UpdateResult.Failed($"[{layerCode}-{elementCode}] DeleteElement(elementnot found!)");

        if (element.Layer.Code != layer.Code)
            return UpdateResult.Failed($"RemoveElement(Layer mismatch!)");

        if (element.DataVersion > requestVersion)
            return UpdateResult.Failed($"[{layerCode}-{elementCode}] DeleteElement(Version passed!)");

        _elements.Remove(element.Code);
        _elementsByLayerCode[element.Layer.Code].Remove(element.Code);
        ElecricalMatrix.Remove(element);

        updateVersion(requestVersion, logVersion);
        Delete(element);
        return UpdateResult.Success();
    }

    public LayerElement this[string code] => GetElement(code);

    public LayerElement GetElement(string code) => ReadByLock(() => GetElementWithoutLock(code));
    protected LayerElement GetElementWithoutLock(string code)
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
            var layerElements = GetLayerElementsWithoutLock(owner.Code);

            foreach (var item in layerElements)
            {
                if (version > item.DataVersion && version > item.StatusVersion) continue;
                yield return item;
            }
        }

    }
}