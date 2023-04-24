using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace GeographyModel
{
    [DataContract]
    public partial class Layer
    {
        [DataMember(Order = 01)]
        public string Code { get; set; }

        [DataMember(Order = 02)]
        public string Displayname { get; set; }

        [DataMember(Order = 03)]
        public LayerGeographyType GeographyType { get; set; }

        [DataMember(Order = 04)]
        public List<LayerField> Fields { get; set; }

        [DataMember(Order = 05)]
        public string ElementDisplaynameFormat { get; set; }

        [DataMember(Order = 05)]
        public bool IsRoutingSource { get; set; }

        [DataMember(Order = 05)]
        public bool IsElectrical { get; set; }

        [DataMember(Order = 06)]
        public bool IsDisconnector { get; set; }
    }
}