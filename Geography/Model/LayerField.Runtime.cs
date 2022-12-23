using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace GeographyModel
{
    public partial class LayerField
    {
        public override string ToString() => $"[{Code}] {Displayname}";

        public void Reset()
        {

        }

        public string GetValue(string[] elementFieldValues)
        {
            var result = string.Empty;
            if (Index > elementFieldValues.Length) return result;
            result = elementFieldValues[Index];
            if (result == null) result = "";
            return result;
        }
    }
}
