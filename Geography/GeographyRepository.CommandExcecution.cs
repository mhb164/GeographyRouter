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
    public List<UpdateResult> Excecute(UpdateElementPackageCommand command)
    {
        var result = WriteByLock(() =>
        {
            if (!_layers.TryGetValue(command.LayerCode, out var layer))
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
