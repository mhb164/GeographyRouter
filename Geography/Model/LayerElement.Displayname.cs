using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GeographyModel
{
    public partial class LayerElement
    {
        string displayname;
        public string Displayname
        {
            get
            {
                displayname ??= GenerateDisplayname(this);

                return displayname;
            }
        }

        public void MakeDisplaynameNull() => displayname = null;

        private static string GenerateDisplayname(LayerElement layerElement)
        {
            if (layerElement.Layer == null)
                return string.Empty;

            var layer = layerElement.Layer;
            try
            {
                if (string.IsNullOrWhiteSpace(layer.ElementDisplaynameFormat))
                    return $"{layer.Displayname} ({layerElement.Code})";

                if (layer.ElementDisplaynameFormat == "{LAYERNAME} ({CODE})")
                    return $"{layer.Displayname} ({layerElement.Code})";

                var result = layer.ElementDisplaynameFormat;
                //------------------
                if (result.Contains("{LAYERNAME}")) result = result.Replace("{LAYERNAME}", layer.Displayname);
                if (result.Contains("{CODE}")) result = result.Replace("{CODE}", layerElement.Code);
                if (result.Contains("{CONNECTED}")) result = result.Replace("{CONNECTED}", layerElement.Connected ? "CLOSE" : "OPEN");
                if (result.Contains("{CONNECTED-PERSIAN}")) result = result.Replace("{CONNECTED-PERSIAN}", layerElement.Connected ? "وصل" : "قطع");
                //------------------

                foreach (var field in layer.Fields)
                    if (field.Index < layerElement.FieldValues.Length && layerElement.Displayname.Contains($"{{{field.Code}}}"))
                    {
                        var value = string.Empty;
                        value = layerElement.FieldValues[field.Index];
                        result = result.Replace($"{{{field.Code}}}", value);
                    }

                return GeographyRepository.PerformTextCorrection(result);

            }
            catch
            {
                return $"{layer.Displayname} ({layerElement.Code})";
            }
        }



    }
}
