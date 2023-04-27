using System;
using System.Runtime.Serialization;


namespace GeographyModel
{
    public partial class LayerField
    {
        public readonly Layer Layer;
        public readonly int Index;
        public readonly string Code;

        public LayerField(Layer layer, int index, string code, string displayname)
        {
            Layer = layer;
            Index = index;
            Code = code;
            Displayname = displayname; 
        }

        public string Displayname { get; private set; }

        public void UpdateDisplayname(string displayname)
        {
            Displayname = displayname;
        }

        public override string ToString() => $"[{Code}] {Displayname}";

        public string GetValue(in string[] elementFieldValues)
        {
            if (Index > elementFieldValues.Length)
                return string.Empty;

            var result = elementFieldValues[Index] ?? string.Empty;

            return result;
        }

        
    }

}
