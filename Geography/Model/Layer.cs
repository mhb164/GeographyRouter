using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace GeographyModel
{
    [DataContract]
    public partial class Layer
    {
        [DataMember(Order = 00)]
        public Guid Id { get; set; }
        [DataMember(Order = 01)]
        public bool Activation { get; set; }
        [DataMember(Order = 02)]
        public string Code { get; set; }
        [DataMember(Order = 03)]
        public string Displayname { get; set; }
        [DataMember(Order = 04)]
        public LayerGeographyType GeographyType { get; set; }
        [DataMember(Order = 05)]
        public bool IsElectrical { get; set; }
        [DataMember(Order = 06)]
        public bool IsDisconnector { get; set; }
        [DataMember(Order = 07)]
        public string OperationStatusFieldCode { get; set; }
        [DataMember(Order = 08)]
        public string OperationStatusOpenValue { get; set; }
        [DataMember(Order = 09)]
        public string ElementDisplaynameFormat { get; set; }
        [DataMember(Order = 10)]
        public List<LayerField> Fields { get; set; }
    }

}
