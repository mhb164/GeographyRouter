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

            Code = code.ToUpperInvariant().Trim();
            GeographyType = geographyType;
            Displayname = displayname.Trim();

            var fieldCodes = fields.Select(x => x.Code);
            if (fieldCodes.Count() != fieldCodes.Distinct().Count())
                throw new ArgumentException("کُدِ یکی از فیلدها تکراری است!");
            Fields = fields.ToList();
        }

        internal Layer Createlayer()
        {
            var layer = new Layer()
            {
                Code = Code.Trim().ToUpperInvariant(),
                GeographyType = GeographyType,
                Displayname = Displayname.Trim(),
                IsRoutingSource = false,
                IsElectrical = false,
                IsDisconnector = false,
                OperationStatusFieldCode = "",
                OperationStatusAbnormalValues = new List<string>(),
                IsNormalOpen = false,
                ElementDisplaynameFormat = "{LAYERNAME} ({CODE})",
                Fields = new List<LayerField>(),
            };

            var index = 0;
            foreach (var field in Fields)
            {
                layer.Fields.Add(new LayerField()
                {
                    Index = index,
                    Code = field.Code.ToUpperInvariant().Trim(),
                    Displayname = field.Displayname,
                });
                index++;
            }
            return layer;
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

            Code = code.ToUpperInvariant().Trim();
            Displayname = displayname.Trim();
        }
    }

    public class MakeLayerAsRoutingSourceCommand
    {
        public readonly string LayerCode;

        public MakeLayerAsRoutingSourceCommand(string layerCode)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");

            LayerCode = layerCode.ToUpperInvariant().Trim();
        }
    }

    public class UpdateLayerCommand
    {
        public readonly string LayerCode;
        public readonly string Displayname;
        public readonly string DisplaynameFormat;//ElementDisplaynameFormat

        public UpdateLayerCommand(string layerCode, string displayname, string displaynameFormat)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displayname)) throw new ArgumentException("نام خالی وارد شده است!");
            if (string.IsNullOrWhiteSpace(displaynameFormat)) throw new ArgumentException("قالب نمایش خالی وارد شده است!");

            LayerCode = layerCode.ToUpperInvariant().Trim();
            Displayname = displayname.Trim();
            DisplaynameFormat = displaynameFormat.Trim();
        }
    }

    public class UpdateLayerRoutingCommand
    {
        public readonly string LayerCode;
        public readonly bool UseInRouting;//IsElectrical
        public readonly string ConnectivityStateFieldCode;//OperationStatusFieldCode
        public readonly List<string> ConnectivityStateAbnormalValues;//OperationStatusAbnormalValues
        public readonly bool Disconnectable;//IsDisconnector
        public readonly bool NormalOpen;//IsNormalOpen

        public UpdateLayerRoutingCommand(string layerCode,
                                         bool useInRouting,
                                         string connectivityStateFieldCode,
                                         List<string> connectivityStateAbnormalValues,
                                         bool disconnectable,
                                         bool normalOpen)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");

            LayerCode = layerCode.ToUpperInvariant().Trim();
            UseInRouting = useInRouting;
            ConnectivityStateFieldCode = connectivityStateFieldCode.ToUpperInvariant().Trim();
            ConnectivityStateAbnormalValues = connectivityStateAbnormalValues.Select(x => x.Trim()).ToList();
            Disconnectable = disconnectable;
            NormalOpen = normalOpen;
        }
    }

    public class DeleteLayerCommand
    {
        public readonly string LayerCode;

        public DeleteLayerCommand(string layerCode)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");

            LayerCode = layerCode.ToUpperInvariant().Trim();
        }
    }

    public class DeleteAllLayersCommand
    {
        public DeleteAllLayersCommand()
        {
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

            LayerCode = layerCode.ToUpperInvariant().Trim();
            Code = code.ToUpperInvariant().Trim();
            Displayname = displayname.Trim();
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

            LayerCode = layerCode.ToUpperInvariant().Trim();
            Code = code.ToUpperInvariant().Trim();
            Displayname = displayname.Trim();
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

            LayerCode = layerCode.ToUpperInvariant().Trim();
            Code = code.ToUpperInvariant().Trim();
        }
    }

    public class UpdateElementPackageCommand
    {
        public readonly string LayerCode;
        public readonly string[] Descriptors;
        public readonly IEnumerable<UpdateElementPackageItem> Items;

        public UpdateElementPackageCommand(string layerCode, string[] descriptors, IEnumerable<UpdateElementPackageItem> items)
        {
            if (string.IsNullOrWhiteSpace(layerCode)) throw new ArgumentException("کُد لایه خالی وارد شده است!");

            LayerCode = layerCode.ToUpperInvariant().Trim();
            Descriptors = descriptors.Select(x => x.ToUpperInvariant().Trim()).ToArray();
            Items = items;
        }

        public override string ToString() => $"{LayerCode}";
    }

    public class UpdateElementPackageItem
    {
        public readonly string ElementCode;
        public readonly DateTime Timetag;
        public readonly double[] Points;
        public readonly string[] DescriptorValues;

        public UpdateElementPackageItem(string elementCode, DateTime timetag, double[] points, string[] descriptorValues)
        {
            if (string.IsNullOrWhiteSpace(elementCode)) throw new ArgumentException("کُد خالی وارد شده است!");

            ElementCode = elementCode;
            Timetag = timetag;
            Points = points;
            DescriptorValues = descriptorValues;
        }

        public override string ToString() => $"{ElementCode}-{string.Join(", ", Points)}-{string.Join(", ", DescriptorValues)}";
    }

}