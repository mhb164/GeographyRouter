using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace GeographyModel
{
    public partial class Domain
    {
        public Domain(string key)
        {
            Key = key;
        }
        [IgnoreDataMember, ScriptIgnore]
        public string Key { get; private set; }
        Dictionary<long, DomainValue> values = new Dictionary<long, DomainValue>();
        [IgnoreDataMember, ScriptIgnore]
        public IEnumerable<DomainValue> Values => values.Values;

        public override string ToString() => $"{Key} ({values.Count} Values)";
        public void Add(DomainValue input)
        {
            if (values.ContainsKey(input.Code)) return;
            else values.Add(input.Code, input);
        }
        public DomainValue GetValue(long code)
        {
            if (values.ContainsKey(code)) return values[code];
            else return null;
        }

        public string this[string codeAsText]
        {
            get
            {
                if (long.TryParse(codeAsText, out var code) == false) return string.Empty;
                return this[code];
            }
        }

        public string this[long code]
        {
            get
            {
                if (values.ContainsKey(code)) return values[code].Value;
                else return string.Empty;
            }
        }


    }

   
}
