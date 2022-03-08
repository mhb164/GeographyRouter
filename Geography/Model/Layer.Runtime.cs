using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
    }     
}
