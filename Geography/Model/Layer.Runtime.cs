using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GeographyModel
{
    public partial class Layer
    {
        public override string ToString() => $"[{Code}] {Displayname}";

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
