using System;
using System.Runtime.Serialization;


namespace GeographyModel
{
    [DataContract]
    public partial class DomainValue
    {
        [DataMember(Order = 00)]
        public Guid Id { get; set; }
        [DataMember(Order = 01)]
        public bool Activation { get; set; }
        [DataMember(Order = 02)]
        public string LayerCode { get; set; }
        [DataMember(Order = 02)]
        public string FieldCode { get; set; }
        [DataMember(Order = 03)]
        public long Code { get; set; }
        [DataMember(Order = 04)]
        public string Value { get; set; }
        [DataMember(Order = 99)]
        public long Version { get; set; }


    }

}
