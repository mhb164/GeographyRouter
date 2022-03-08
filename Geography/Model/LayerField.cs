using System.Runtime.Serialization;


namespace GeographyModel
{
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

}
