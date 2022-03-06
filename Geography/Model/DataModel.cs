using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace GeographyModel
{
    public enum LayerGeographyType
    {
        Point = 1,
        Polyline = 2,
        Polygon = 3,
    }
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
    [DataContract]
    public partial class LayerField
    {
        [DataMember(Order = 01)]
        public bool Activation { get; set; }
        [DataMember(Order = 02)]
        public int Index { get; set; }
        [DataMember(Order = 03)]
        public string Code { get; set; }
        [DataMember(Order = 04)]
        public string Displayname { get; set; }
        [DataMember(Order = 05)]
        public string Type { get; set; }
    }

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


    public partial class LayerElement
    {
        public Guid Id { get; set; }
        public bool Activation { get; set; }
        public string Code { get; set; }
        public long Version { get; set; }
        public double[] Points { get; set; }
        public string FieldValuesText { get; set; }
    }

}
