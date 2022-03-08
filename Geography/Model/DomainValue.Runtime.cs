using System;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace GeographyModel
{
    public partial class DomainValue
    {
        [IgnoreDataMember, ScriptIgnore]
        public string DomainKey => GenerateKey(LayerCode, FieldCode);

        public static string GenerateKey(string layercode, string fieldcode) => $"[{layercode.ToUpper().Trim()}].[{fieldcode.ToUpper().Trim()}]";

        public override string ToString() => $"[{DomainKey}].{Code}= {Value} (v{new DateTime(Version):yyyy-MM-dd HH:mm:ss.fff})";
    }

}
