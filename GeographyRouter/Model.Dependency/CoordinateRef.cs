using System;

namespace GeographyRouter
{
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

}
