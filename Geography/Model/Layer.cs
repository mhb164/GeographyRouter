using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace GeographyModel
{
    public partial class Layer
    {
        public readonly string Code;
        public readonly LayerGeographyType GeographyType;
        private readonly Dictionary<string, LayerField> fields;
        private readonly Dictionary<int, LayerField> fieldsbyIndex;

        public Layer(string code,
                     string displayname,
                     LayerGeographyType geographyType)
        {
            Code = code;
            Displayname = displayname;
            GeographyType = geographyType;

            fields = new Dictionary<string, LayerField>();
            fieldsbyIndex = new Dictionary<int, LayerField>();

            ElementDisplaynameFormat = "{LAYERNAME} ({CODE})";
            IsRoutingSource = false;
            IsElectrical = false;
            IsDisconnector = false;
        }

        public Layer(string code,
                     string displayname,
                     LayerGeographyType geographyType,
                     string elementDisplaynameFormat,
                     bool isRoutingSource,
                     bool isElectrical,
                     bool isDisconnector)
        {
            Code = code;
            fields = new Dictionary<string, LayerField>();
            fieldsbyIndex = new Dictionary<int, LayerField>();

            Displayname = displayname;
            GeographyType = geographyType;
            ElementDisplaynameFormat = elementDisplaynameFormat;
            IsRoutingSource = isRoutingSource;
            IsElectrical = isElectrical;
            IsDisconnector = isDisconnector;
        }

        public string Displayname { get; private set; }


        public IEnumerable<LayerField> Fields => fields.Values;

        public string ElementDisplaynameFormat { get; private set; }

        public bool IsRoutingSource { get; private set; }

        public bool IsElectrical { get; private set; }

        public bool IsDisconnector { get; private set; }

        public override string ToString() => $"[{Code}] {Displayname}";

        public void UpdateIsRoutingSource(bool isRoutingSource)
        {
            IsRoutingSource = isRoutingSource;
        }

        public void Update(string displayname,
            string elementDisplaynameFormat,
            bool isElectrical,
            bool isDisconnector)
        {
            Displayname = displayname;
            ElementDisplaynameFormat = elementDisplaynameFormat;
            IsElectrical = isElectrical;
            IsDisconnector = isDisconnector;
        }

        public bool CheckDisplaynameFormat(string input, out string errorMessage)
        {
            var fieldCodes = Regex.Matches(input, @"\{(.+?)\}").Cast<Match>().Select(m => m.Groups[1].Value.ToUpperInvariant());
            foreach (var fieldCode in fieldCodes)
            {
                if (fieldCode == "LAYERNAME") continue;
                if (fieldCode == "CODE") continue;
                if (fieldCode == "CONNECTED") continue;
                if (fieldCode == "CONNECTED-PERSIAN") continue;

                var field = GetField(fieldCode);
                if (field == null)
                {
                    errorMessage = $"{fieldCode} not found!";
                    return false;
                }
            }

            errorMessage = "";
            return true;

        }

        public void AddFiled(string fieldCode, string fieldDisplayname)
        {
            var index = 0;
            if (fields.Count > 0)
                index = fields.Values.Max(x => x.Index) + 1;

            var field = new LayerField(this, index, fieldCode, fieldDisplayname);

            fields.Add(field.Code, field);
            fieldsbyIndex.Add(field.Index, field);
        }

        public void RemoveFiled(string fieldCode)
        {
            if (!fields.TryGetValue(fieldCode, out var layerField))
                throw new ArgumentException($"فیلد با کُدِ {fieldCode} پیدا نشد!");

            fields.Remove(layerField.Code);
            fieldsbyIndex.Remove(layerField.Index);

            ReIndexFields();
        }

        private void ReIndexFields()
        {
            var ordered = Fields.OrderBy(x => x.Index).ToList();
            fields.Clear();
            fieldsbyIndex.Clear();

            foreach (var item in ordered)
                AddFiled(item.Code, item.Displayname);
        }

        public LayerField GetField(string fieldCode)
        {
            if (fields.TryGetValue(fieldCode, out var layerField))
                return layerField;

            return null;
        }

        public LayerField GetField(int fieldIndex)
        {
            if (fieldsbyIndex.TryGetValue(fieldIndex, out var layerField))
                return layerField;

            return null;
        }
    }
}