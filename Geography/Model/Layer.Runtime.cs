using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace GeographyModel
{
    public partial class Layer
    {
        [IgnoreDataMember, ScriptIgnore]
        public LayerField OperationStatusField { get; private set; }

        public override string ToString() => $"[{Code}] {Displayname}";

        public void Reset(Func<string, string, Domain> getDomainFunc)
        {
            foreach (var item in Fields)
            {
                item.Reset((string fieldCode) => getDomainFunc?.Invoke(Code, fieldCode));
            }
            OperationStatusField = null;
            if (IsElectrical && !string.IsNullOrWhiteSpace(OperationStatusFieldCode))
                OperationStatusField = Fields.FirstOrDefault(x => x.Code == OperationStatusFieldCode);
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

                var field = Fields.FirstOrDefault(x => x.Code == fieldCode);
                if (field == null)
                {
                    errorMessage = $"{fieldCode } not found!";
                    return false;
                }
            }

            errorMessage = "";
            return true;

        }


        internal void ReIndexFields()
        {
            var index = 0;
            foreach (var item in Fields.OrderBy(x => x.Code))
            {
                item.Index = index;
                index++;
            }
        }

    }
}
