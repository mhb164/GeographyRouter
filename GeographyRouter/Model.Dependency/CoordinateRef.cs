using System;

namespace GeographyRouter
{
    public class CoordinateRef
    {
        private readonly Func<double> _getLatitudeAccessor;
        private readonly Func<double> _getLongitudeAccessor;
        public CoordinateRef(Func<double> getLatitudeAccessor, Func<double> getLongitudeAccessor)
        {
            _getLatitudeAccessor = getLatitudeAccessor;
            _getLongitudeAccessor = getLongitudeAccessor;
        }


        public double Latitude => _getLatitudeAccessor.Invoke();
        public double Longitude => _getLongitudeAccessor.Invoke();

        public string Text => $"{Latitude}, {Longitude}";
        public override string ToString() => $"({Latitude}, {Longitude})";
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
        public override int GetHashCode() => Latitude.GetHashCode() ^ Longitude.GetHashCode();
    }
}
