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

        public void Reset(Func<string, Domain> getDomainFunc)
        {
            GetDomainFunc = getDomainFunc;
        }

        Func<string, Domain> GetDomainFunc;
        [IgnoreDataMember, ScriptIgnore]
        public Domain Domain => GetDomainFunc?.Invoke(Code);
        public string GetValue(string[] elementFieldValues)
        {
            var result = string.Empty;
            if (Index > elementFieldValues.Length) return result;
            if (Domain == null) result = elementFieldValues[Index];
            else
            {
                result = Domain[elementFieldValues[Index]];
                if (string.IsNullOrWhiteSpace(result))
                    result = elementFieldValues[Index];
            }
            if (result == null) result = "";
            return result;
        }

        public string GetValueRaw(string[] elementFieldValues)
        {
            var result = string.Empty;
            if (Index > elementFieldValues.Length) return result;
            if (Domain != null) result = elementFieldValues[Index];
            if (result == null) result = "";
            return result;
        }
    }
}
