using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace GeographyModel
{
    public partial class LayerElement
    {
        public bool CheckPointsChange(in double[] points)
        {
            if (this.Points.Length != points.Length)
                return false;

            for (int i = 0; i < this.Points.Length; i++)
                if (this.Points[i] != points[i])
                    return false;

            return true;
        }

        public bool CheckFieldValuesChange(in string[] fieldValues)
        {
            if (this.FieldValues.Length != fieldValues.Length)
                return false;

            for (int i = 0; i < this.FieldValues.Length; i++)
                if (this.FieldValues[i] != fieldValues[i])
                    return false;

            return true;
        }

        public bool CheckStatusChange(in LayerElementStatus normalStatus, in LayerElementStatus actualStatus)
            => NormalStatus != normalStatus || ActualStatus != actualStatus;

        public string GetFieldvalue(string Fieldcode)
        {
            if (Layer == null) return string.Empty;
            var field = Layer.GetField(Fieldcode);
            if (field == null) return string.Empty;
            var fieldValues = FieldValues;
            if (field.Index >= fieldValues.Length) return string.Empty;
            return FieldValues[field.Index];
        }

        public static double[] TranslatePoints(byte[] bytes)
        {
            var points = new List<double>();
            if (bytes.Length == 0) return points.ToArray();
            if (bytes.Length % 8 != 0) return points.ToArray();
            for (int i = 0; i < bytes.Length / 8; i++)
                points.Add(BitConverter.ToDouble(bytes, i * 8));
            return points.ToArray();
        }

        public static byte[] TranslatePoints(double[] points)
        {
            var bytes = new List<byte>();
            foreach (var item in points)
                bytes.AddRange(BitConverter.GetBytes(item));
            return bytes.ToArray();
        }

        public static string TranslateFieldValues(List<string> values) => string.Join("♦", values);

        public static bool TranslateShape(string shape, uint defaultSRID, out List<double> points, out string message)
        {
            points = new List<double>();
            shape = shape.ToUpper().Trim();
            if (shape.Contains("MULTIPOINT"))
            {
                message = $"MULTIPOINT error ({shape})";
                return false;
                //throw new Exception($"MULTIPOINT error ({shape})");
            }
            else if (shape.Contains("MULTILINESTRING"))
            {
                shape = shape.Replace("MULTI", "");
            }
            else if (shape.Contains("MULTIPOLYGON"))
            {
                shape = shape.Replace("MULTI", "");
            }

            shape = shape.Replace("POINT ", "").Replace("LINESTRING ", "").Replace("POLYGON ", "").Replace("(", "").Replace(")", "");
            foreach (var coordinatestext in shape.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var coordinatesitems = coordinatestext.Trim().Split(' ');
                    var x = double.Parse(coordinatesitems[0], CultureInfo.InvariantCulture);
                    var y = double.Parse(coordinatesitems[1], CultureInfo.InvariantCulture);
                    var SRID = defaultSRID;
                    if (coordinatesitems.Length > 2 &&
                        !uint.TryParse(coordinatesitems[2], NumberStyles.Any, CultureInfo.InvariantCulture, out SRID))
                    {
                        SRID = defaultSRID;
                    }

                    EPSG.FromUTM(SRID, true, x, y, out var latitude, out var longitude, defaultSRID);

                    points.Add(latitude);
                    points.Add(longitude);
                }
                catch (Exception ex)
                {
                    message = $"Error On ({shape}>{ex.Message})";
                    return false;
                    //throw new Exception($"Error On {shape}", ex);
                }
            }

            message = "";
            return true;
        }

        [IgnoreDataMember, JsonIgnore]
        public double DistanceInKm => CalculateDistance(this);
        internal static double CalculateDistance(LayerElement element)
        {
            var dist = 0.0;
            if (element.Points.Length <= 2) return dist;
            for (int i = 0; i < (element.Points.Length / 2) - 1; i++)
            {
                dist += CalculateDistance(element.Points[i * 2], element.Points[(i * 2) + 1], element.Points[(i + 1) * 2], element.Points[((i + 1) * 2) + 1]);
            }
            return dist;
        }
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            if ((Math.Round(lat1, 5) == Math.Round(lat2, 5)) && (Math.Round(lon1, 5) == Math.Round(lon2, 5))) return 0.0;
            double theta = lon1 - lon2;
            double dist = Math.Sin(Degree2Radian(lat1)) * Math.Sin(Degree2Radian(lat2)) + Math.Cos(Degree2Radian(lat1)) * Math.Cos(Degree2Radian(lat2)) * Math.Cos(Degree2Radian(theta));
            dist = Math.Acos(dist);
            dist = Radian2Degree(dist);
            dist = dist * 60 * 1.1515;//miles

            if (unit == 'M')//miles
            {
                return dist;
            }
            else if (unit == 'K')//kilometers  
            {
                return dist * 1.609344;
            }
            else if (unit == 'N') //nautical miles
            {
                return dist * 0.8684;
            }
            return (dist);
        }
        private static double Degree2Radian(double deg) => (deg * Math.PI / 180.0);
        private static double Radian2Degree(double rad) => (rad / Math.PI * 180.0);
    }
}
