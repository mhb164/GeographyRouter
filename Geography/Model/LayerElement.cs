using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace GeographyModel
{

    [DataContract]
    public partial class LayerElement
    {
        [DataMember(Order = 01)]
        public Guid Id { get; set; }

        [DataMember(Order = 02)]
        public string Code { get; set; }

        [DataMember(Order = 03)]
        public long Version { get; set; }

        [DataMember(Order = 04)]
        public double[] Points { get; set; }

        [DataMember(Order = 05)]
        public string FieldValuesText { get; set; }
    }

}
