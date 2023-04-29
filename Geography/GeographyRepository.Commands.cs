using GeographyModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class GeographyRepository
{
    public static string PerformCodeCorrection(string code)
        => code.ToUpperInvariant().Trim();

    public static string PerformTextCorrection(string text)
    {
        if (text == null) return "";
        text = text.Replace("ي", "ی").Replace("ك", "ک");
        if (text.Contains("  "))
        {
            var options = System.Text.RegularExpressions.RegexOptions.None;
            var regex = new System.Text.RegularExpressions.Regex("[ ]{2,}", options);
            text = regex.Replace(text, " ");
        }
        return text.Trim();
    }

    public class CreateLayerCommand
    {
        public readonly string Code;
        public readonly LayerGeographyType GeographyType;
        public readonly string Displayname;
        public readonly IEnumerable<CreateLayerCommandFiled> Fields;

        public CreateLayerCommand(string code, LayerGeographyType geographyType, string displayname, IEnumerable<CreateLayerCommandFiled> fields)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("کُد خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displayname)) throw new ArgumentException("نام خالی وارد شده است!");

            Code = PerformCodeCorrection(code);
            GeographyType = geographyType;
            Displayname = PerformTextCorrection(displayname);

            var fieldCodes = fields.Select(x => x.Code);
            if (fieldCodes.Count() != fieldCodes.Distinct().Count())
                throw new ArgumentException("کُدِ یکی از فیلدها تکراری است!");
            Fields = fields.ToList();
        }

        internal Layer Createlayer()
        {
            var layer = new Layer(Code,
                Displayname,
                GeographyType,
                elementDisplaynameFormat: "{LAYERNAME} ({CODE})",
                isRoutingSource: false,
                isElectrical: false,
                isDisconnector: false);

            foreach (var field in Fields)
            {
                layer.AddFiled(field.Code, field.Displayname);
            }
            return layer;
        }
    }

    public class CreateLayerFieldCommand
    {
        public readonly string LayerCode;
        public readonly string Code;
        public readonly string Displayname;

        public CreateLayerFieldCommand(string layerCode, string code, string displayname)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("کُد خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displayname)) throw new ArgumentException("نام خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            Code = PerformCodeCorrection(code);
            Displayname = PerformTextCorrection(displayname);
        }
    }

    public class UpdateLayerFieldCommand
    {
        public readonly string LayerCode;
        public readonly string Code;
        public readonly string Displayname;

        public UpdateLayerFieldCommand(string layerCode, string code, string displayname)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("کُد خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displayname)) throw new ArgumentException("نام خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            Code = PerformCodeCorrection(code);
            Displayname = PerformTextCorrection(displayname);
        }
    }

    public class DeleteLayerFieldCommand
    {
        public readonly string LayerCode;
        public readonly string Code;

        public DeleteLayerFieldCommand(string layerCode, string code)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("کُد خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            Code = PerformCodeCorrection(code);
        }
    }

    public class DeleteLayerCommand
    {
        public readonly string LayerCode;

        public DeleteLayerCommand(string layerCode)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
        }
    }

    public class DeleteAllLayersCommand
    {
        public DeleteAllLayersCommand() { }
    }

    public class MakeLayerAsRoutingSourceCommand
    {
        public readonly string LayerCode;

        public MakeLayerAsRoutingSourceCommand(string layerCode)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
        }
    }

    public class CreateLayerCommandFiled
    {
        public readonly string Code;
        public readonly string Displayname;

        public CreateLayerCommandFiled(string code, string displayname)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("کُد خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displayname)) throw new ArgumentException("نام خالی وارد شده است!");

            Code = PerformCodeCorrection(code);
            Displayname = PerformTextCorrection(displayname);
        }
    }

    public class UpdateLayerCommand
    {
        public readonly string LayerCode;
        public readonly string Displayname;
        public readonly string DisplaynameFormat;//ElementDisplaynameFormat
        public readonly bool UseInRouting;//IsElectrical
        public readonly bool Disconnectable;//IsDisconnector

        public UpdateLayerCommand(string layerCode,
                                  string displayname,
                                  string displaynameFormat,
                                  bool useInRouting,
                                  bool disconnectable)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displayname)) throw new ArgumentException("نام خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displaynameFormat)) throw new ArgumentException("قالب نمایش خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            Displayname = PerformTextCorrection(displayname);
            DisplaynameFormat = displaynameFormat.Trim();
            UseInRouting = useInRouting;
            Disconnectable = disconnectable;
        }
    }

    public class CreateUpdateElementCommand
    {
        public readonly string LayerCode;
        public readonly string[] LayerDescriptors;
        public readonly string ElementCode;
        public readonly DateTime Timetag;
        public readonly double[] Points;
        public readonly string[] DescriptorValues;
        public readonly LayerElementStatus NormalStatus;
        public readonly LayerElementStatus ActualStatus;

        public CreateUpdateElementCommand(string layerCode,
            string[] layerDescriptors,
            string elementCode,
            DateTime timetag,
            double[] points,
            string[] descriptorValues,
            bool isNormalOpen,
            bool connected)
            : this(layerCode,
                   layerDescriptors,
                   elementCode,
                   timetag,
                   points,
                   descriptorValues,
                   isNormalOpen ? LayerElementStatus.Open : LayerElementStatus.Close,
                   connected ? LayerElementStatus.Close : LayerElementStatus.Open)
        { }

        public CreateUpdateElementCommand(string layerCode,
            string[] layerDescriptors,
            string elementCode,
            DateTime timetag,
            double[] points,
            string[] descriptorValues,
            LayerElementStatus normalStatus,
            LayerElementStatus actualStatus)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(elementCode)) throw new ArgumentException("کُد خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            LayerDescriptors = layerDescriptors.Select(x => PerformCodeCorrection(x)).ToArray();
            ElementCode = PerformCodeCorrection(elementCode);
            Timetag = timetag;
            Points = points;
            DescriptorValues = descriptorValues.Select(x => PerformTextCorrection(x))
                                               .ToArray();
            NormalStatus = normalStatus;
            ActualStatus = actualStatus;
        }
    }

    public class UpdateElementDataCommand
    {
        public readonly string LayerCode;
        public readonly string[] LayerDescriptors;
        public readonly string ElementCode;
        public readonly DateTime Timetag;
        public readonly double[] Points;
        public readonly string[] DescriptorValues;

        public UpdateElementDataCommand(string layerCode,
            string[] layerDescriptors,
            string elementCode,
            DateTime timetag,
            double[] points,
            string[] descriptorValues)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(elementCode)) throw new ArgumentException("کُد خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            LayerDescriptors = layerDescriptors.Select(x => PerformCodeCorrection(x)).ToArray();
            ElementCode = PerformCodeCorrection(elementCode);
            Timetag = timetag;
            Points = points;
            DescriptorValues = descriptorValues.Select(x => PerformTextCorrection(x))
                                               .ToArray();
        }
    }

    public class UpdateElementStatusCommand
    {
        public readonly string LayerCode;
        public readonly string ElementCode;
        public readonly DateTime Timetag;
        public readonly LayerElementStatus NormalStatus;
        public readonly LayerElementStatus ActualStatus;

        public UpdateElementStatusCommand(string layerCode,
                                          string elementCode,
                                          DateTime timetag,
                                          bool isNormalOpen,
                                          bool connected)
            : this(layerCode,
                   elementCode,
                   timetag,
                   isNormalOpen ? LayerElementStatus.Open : LayerElementStatus.Close,
                   connected ? LayerElementStatus.Close : LayerElementStatus.Open)
        { }

        public UpdateElementStatusCommand(string layerCode, string elementCode, DateTime timetag, LayerElementStatus normalStatus, LayerElementStatus actualStatus)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(elementCode)) throw new ArgumentException("کُد خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            ElementCode = PerformCodeCorrection(elementCode);
            Timetag = timetag;
            NormalStatus = normalStatus;
            ActualStatus = actualStatus;
        }
    }

    public class DeleteElementsCommand
    {
        public readonly string LayerCode;
        public readonly List<string> ElementCodes;
        public readonly DateTime Timetag;

        public DeleteElementsCommand(string layerCode, List<string> elementCodes, DateTime timetag)
        {
            if (string.IsNullOrWhiteSpace(layerCode))
                throw new ArgumentException("کُد لایه خالی وارد شده است!");

            if (!elementCodes.Select(x => string.IsNullOrWhiteSpace(x)).Any())
                throw new ArgumentException("کُد خالی وارد شده است!");

            LayerCode = PerformCodeCorrection(layerCode);
            ElementCodes = elementCodes.Select(x => PerformCodeCorrection(x)).ToList();
            Timetag = timetag;
        }

        public override string ToString() => $"{string.Join(", ", ElementCodes)}";
    }
}