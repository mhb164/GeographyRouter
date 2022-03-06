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
        [IgnoreDataMember, ScriptIgnore]
        public LayerField OperationStatusField { get; private set; }
    }
    public partial class LayerField
    {
        public override string ToString() => $"[{Code}] {Displayname}";

        public void Reset(Func<string, Domain> getDomainFunc)
        {
            GetDomainFunc = getDomainFunc;
        }

        Func<string, Domain> GetDomainFunc;
        [IgnoreDataMember, ScriptIgnore]
        public Domain Domain => GetDomainFunc?.Invoke(Code);
        public string GetValue(string[] elementFieldValues)
        {
            var result = string.Empty;
            if (Index > elementFieldValues.Length) return result;
            if (Domain == null) result = elementFieldValues[Index];
            else
            {
                result = Domain[elementFieldValues[Index]];
                if (string.IsNullOrWhiteSpace(result))
                    result = elementFieldValues[Index];
            }
            if (result == null) result = "";
            return result;
        }
        public string GetValueRaw(string[] elementFieldValues)
        {
            var result = string.Empty;
            if (Index > elementFieldValues.Length) return result;
            if (Domain != null) result = elementFieldValues[Index];
            if (result == null) result = "";
            return result;
        }
    }



    public partial class Domain
    {
        public Domain(string key)
        {
            Key = key;
        }
        [IgnoreDataMember, ScriptIgnore]
        public string Key { get; private set; }
        Dictionary<long, DomainValue> values = new Dictionary<long, DomainValue>();
        [IgnoreDataMember, ScriptIgnore]
        public IEnumerable<DomainValue> Values => values.Values;

        public override string ToString() => $"{Key} ({values.Count} Values)";
        public void Add(DomainValue input)
        {
            if (values.ContainsKey(input.Code)) return;
            else values.Add(input.Code, input);
        }
        public DomainValue GetValue(long code)
        {
            if (values.ContainsKey(code)) return values[code];
            else return null;
        }

        public string this[string codeAsText]
        {
            get
            {
                if (long.TryParse(codeAsText, out var code) == false) return string.Empty;
                return this[code];
            }
        }

        public string this[long code]
        {
            get
            {
                if (values.ContainsKey(code)) return values[code].Value;
                else return string.Empty;
            }
        }


    }
    public partial class DomainValue
    {
        //public DomainValue(Domain owner, Guid id, long code, string value)
        //{
        //    Owner = owner;
        //    Id = id;
        //    Code = code;
        //    Value = value;
        //}


        //public Domain Owner { get; private set; }
        [IgnoreDataMember, ScriptIgnore]
        public string DomainKey => GenerateKey(LayerCode, FieldCode);
        public static string GenerateKey(string layercode, string fieldcode) => $"[{layercode.ToUpper().Trim()}].[{fieldcode.ToUpper().Trim()}]";

        public override string ToString() => $"[{DomainKey}].{Code}= {Value} (v{new DateTime(Version):yyyy-MM-dd HH:mm:ss.fff})";
    }

    public class CoordinateRef
    {
        //public CoordinateRef(double latitude, double longitude)
        //{
        //    Latitude = latitude;
        //    Longitude = longitude;
        //}
        //public double Latitude { get; set; }
        //public double Longitude { get; set; }

        public CoordinateRef(Func<double> getLatitudeFunc, Func<double> getLongitudeFunc)
        {
            GetLatitudeFunc = getLatitudeFunc;
            GetLongitudeFunc = getLongitudeFunc;
        }
        Func<double> GetLatitudeFunc; public double Latitude => GetLatitudeFunc.Invoke();
        Func<double> GetLongitudeFunc; public double Longitude => GetLongitudeFunc.Invoke();
        public override string ToString() { return $"({Latitude}, {Longitude})"; }
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            var item = (CoordinateRef)obj;
            return (Latitude == item.Latitude) && (Longitude == item.Longitude);
        }
        public static bool operator ==(CoordinateRef x, CoordinateRef y) => Equals(x, y);
        public static bool operator !=(CoordinateRef x, CoordinateRef y) => !Equals(x, y);
        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }
    }
    public partial class LayerElement
    {
        public void Reset(Layer layer)
        {
            Layer = layer;
        }
        ~LayerElement()
        {
            Layer = null;
        }
        [IgnoreDataMember, ScriptIgnore]
        public Layer Layer { get; private set; }
        [IgnoreDataMember, ScriptIgnore]
        public string Displayname { get; private set; }
        [IgnoreDataMember, ScriptIgnore]
        public bool IsClosed
        {
            get
            {
                if (Layer == null) return false;
                if (Layer.IsElectrical == false) return false;
                //-------------
                if (Layer.OperationStatusField == null) return true;
                if (string.IsNullOrWhiteSpace(Layer.OperationStatusOpenValue)) return true;
                //-------------
                if (Layer.OperationStatusField.GetValue(FieldValues).Trim() == Layer.OperationStatusOpenValue.Trim()) return false;
                else return true;
            }
        }
        public void ResetDisplayname(Func<string, string, Domain> GetDomainFunc)
        {
            if (Layer == null) Displayname = "";
            else
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(Layer.ElementDisplaynameFormat)) Displayname = $"{Layer.Displayname} ({Code})";
                    else if (Layer.ElementDisplaynameFormat == "{LAYERNAME} ({CODE})") Displayname = $"{Layer.Displayname} ({Code})";
                    else
                    {
                        Displayname = Layer.ElementDisplaynameFormat;
                        //------------------
                        if (Displayname.Contains("{LAYERNAME}")) Displayname = Displayname.Replace("{LAYERNAME}", Layer.Displayname);
                        if (Displayname.Contains("{CODE}")) Displayname = Displayname.Replace("{CODE}", Code);
                        //------------------
                        var fieldValues = FieldValues;
                        foreach (var field in Layer.Fields)
                            if (field.Index < fieldValues.Length && Displayname.Contains($"{{{field.Code}}}"))
                            {
                                var value = string.Empty;
                                var domain = GetDomainFunc?.Invoke(Layer.Code, field.Code);
                                if (domain == null) value = FieldValues[field.Index];
                                else
                                {
                                    value = domain[FieldValues[field.Index]];
                                    if (string.IsNullOrWhiteSpace(value))
                                        value = FieldValues[field.Index];
                                }
                                Displayname = Displayname.Replace($"{{{field.Code}}}", value);
                            }

                        Displayname = PerformTextCorrection(Displayname);

                        if (Displayname.Contains("{"))
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    Displayname = $"{Layer.Displayname} ({Code})";
                }
            }
        }
        public static string PerformTextCorrection(string text)
        {
            if (text == null) return "";
            text = text.Replace("ي", "ی").Replace("ك", "ک");
            if (text.Contains("  "))
            {
                var options = System.Text.RegularExpressions.RegexOptions.None;
                var regex = new System.Text.RegularExpressions.Regex("[ ]{2,}", options);
                text = regex.Replace(text, " ");
            }
            return text;
        }
        [IgnoreDataMember, ScriptIgnore]
        public string[] FieldValues => FieldValuesText.Split('♦');


        public string GetFieldvalue(string Fieldcode)
        {
            if (Layer == null) return string.Empty;
            var field = Layer.Fields.FirstOrDefault(x => x.Code == Fieldcode.ToUpper().Trim());
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
        //public static string[] TranslateFieldValues(string text) => text.Split('♦');
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
                    if (coordinatesitems.Length > 2)
                    {
                        if (uint.TryParse(coordinatesitems[2], NumberStyles.Any, CultureInfo.InvariantCulture, out SRID) == false) SRID = defaultSRID;
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


        public void ResetRouting()
        {
            Routed = false;
            Coordinates = new List<CoordinateRef>();
            CoordinateFirst = null;
            CoordinateLast = null;
            //-----------
            for (int i = 0; i < Points.Length / 2; i++)
            {
                var i1 = i * 2;
                var i2 = (i * 2) + 1;
                Coordinates.Add(new CoordinateRef(() => Points[i1], () => Points[i2]));
                //Coordinates.Add(new CoordinateRef(Points[i1], Points[i2]));
            }

            if (Coordinates.Count > 0) CoordinateFirst = Coordinates[0];
            if (Coordinates.Count > 1) CoordinateLast = Coordinates[Coordinates.Count - 1];
        }
        [IgnoreDataMember, ScriptIgnore]
        public bool Routed { get; set; }
        [IgnoreDataMember, ScriptIgnore]
        public List<CoordinateRef> Coordinates { get; private set; }
        [IgnoreDataMember, ScriptIgnore]
        public CoordinateRef CoordinateFirst { get; private set; }
        [IgnoreDataMember, ScriptIgnore]
        public CoordinateRef CoordinateLast { get; private set; }

        [IgnoreDataMember, ScriptIgnore]
        public double DistanceInKm => CalculateDistance(this);
        internal static double CalculateDistance(LayerElement element)
        {
            var dist = 0.0;
            if (element.Points.Length <= 2) return dist;
            for (int i = 0; i < (element.Points.Length / 2) - 1; i++)
            {
                dist += calculateDistance(element.Points[i * 2], element.Points[(i * 2) + 1], element.Points[(i + 1) * 2], element.Points[((i + 1) * 2) + 1]);
            }
            return dist;
        }
        private static double calculateDistance(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            if ((Math.Round(lat1, 5) == Math.Round(lat2, 5)) && (Math.Round(lon1, 5) == Math.Round(lon2, 5))) return 0.0;
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
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
        private static double deg2rad(double deg) => (deg * Math.PI / 180.0);
        private static double rad2deg(double rad) => (rad / Math.PI * 180.0);
    }


    public abstract partial class LayerElementsMatrix
    {
        public LayerElementsMatrix(Func<Guid, LayerElement> getElementById_Func)
        {
            GetElementById_Func = getElementById_Func;
        }

        Func<Guid, LayerElement> GetElementById_Func; protected LayerElement GetElement(Guid id) => GetElementById_Func?.Invoke(id);
        public abstract void Add(LayerElement element);
        public abstract void Remove(LayerElement element);
        public abstract void HitTest(ref double latitude, ref double longitude, ref List<LayerElement> result, bool justNotRoute);
    }

    public partial class LayerElementsMatrixByPoint : LayerElementsMatrix
    {
        public LayerElementsMatrixByPoint(Func<Guid, LayerElement> getElementById_Func) : base(getElementById_Func) { }


        Dictionary<Guid, List<ulong>> lookupsByElements = new Dictionary<Guid, List<ulong>>();
        Dictionary<ulong, Dictionary<ulong, List<Guid>>> lookups = new Dictionary<ulong, Dictionary<ulong, List<Guid>>>();
        static ulong CreateKey1(double latitude, double longitude) => (ulong)Math.Floor(latitude * 1000) << 32 | (ulong)Math.Floor(longitude * 1000);
        static ulong CreateKey2(double latitude, double longitude) => (ulong)Math.Floor(latitude * 1000000) << 32 | (ulong)Math.Floor(longitude * 1000000);

        public override void Add(LayerElement element)
        {
            if (lookupsByElements.ContainsKey(element.Id) == false) lookupsByElements.Add(element.Id, new List<ulong>());
            if (element.Points.Length >= 2)
                Add(ref element.Points[0], ref element.Points[1], element);
            if (element.Points.Length >= 4)
                Add(ref element.Points[element.Points.Length - 2], ref element.Points[element.Points.Length - 1], element);
        }

        private void Add(ref double latitude, ref double longitude, LayerElement element)
        {
            var key1 = CreateKey1(latitude, longitude);
            var key2 = CreateKey2(latitude, longitude);
            if (!lookups.ContainsKey(key1)) lookups.Add(key1, new Dictionary<ulong, List<Guid>>());
            if (!lookups[key1].ContainsKey(key2)) lookups[key1].Add(key2, new List<Guid>());
            if (lookups[key1][key2].Contains(element.Id)) return;

            lookups[key1][key2].Add(element.Id);
            if (lookupsByElements[element.Id].Contains(key1) == false) lookupsByElements[element.Id].Add(key1);
        }

        public override void Remove(LayerElement element)
        {
            if (lookupsByElements.ContainsKey(element.Id) == false) return;
            foreach (var key in lookupsByElements[element.Id])
            {
                if (lookups.ContainsKey(key) == false) continue;
                foreach (var lookup in lookups[key].Values)
                    if (lookup.Contains(element.Id))
                        lookup.Remove(element.Id);
            }
            lookupsByElements.Remove(element.Id);
        }

        public override void HitTest(ref double latitude, ref double longitude, ref List<LayerElement> result, bool justNotRoute)
        {
            var key1 = CreateKey1(latitude, longitude);
            if (lookups.ContainsKey(key1) == false) return;
            var key2 = CreateKey2(latitude, longitude);
            if (lookups[key1].ContainsKey(key2) == false) return;
            var lookup = lookups[key1][key2];
            foreach (var elementId in lookup)
            {
                var element = GetElement(elementId);
                if (element == null) continue;
                if (element.Routed && justNotRoute) continue;

                if (element.CoordinateFirst != null)
                {
                    if (element.CoordinateFirst.Latitude == latitude && element.CoordinateFirst.Longitude == longitude)
                        if (result.Contains(element) == false)
                        {
                            result.Add(element);
                            continue;
                        }
                }

                if (element.CoordinateLast != null)
                {
                    if (element.CoordinateLast.Latitude == latitude && element.CoordinateLast.Longitude == longitude)
                        if (result.Contains(element) == false)
                        {
                            result.Add(element);
                            continue;
                        }
                }
            }

        }
    }
    public partial class LayerElementsMatrixByPolygon : LayerElementsMatrix
    {
        public LayerElementsMatrixByPolygon(Func<Guid, LayerElement> getElementById_Func) : base(getElementById_Func) { }
        Dictionary<Guid, LayerElement> elements = new Dictionary<Guid, LayerElement>();

        public override void Add(LayerElement element)
        {
            if (elements.ContainsKey(element.Id)) return;
            else elements.Add(element.Id, element);
        }
        public override void Remove(LayerElement element)
        {
            if (elements.ContainsKey(element.Id) == false) return;
            else elements.Remove(element.Id);
        }
        public override void HitTest(ref double latitude, ref double longitude, ref List<LayerElement> result, bool justNotRoute)
        {

        }
    }

}
