using System;
using System.Collections.Generic;
using System.Text;
using Projections.CoordinateSystems;
using Projections.CoordinateSystems.Transformations;

public static class EPSG
{
    //https://epsg.io European Petroleum Survey Group //https://www.puzzlr.org/epsg-codes-explained/ https://support.esri.com/en/technical-article/000002814

    //EPSG:4326 <> WGS 84
    //GEOGCS["WGS 84", DATUM["WGS_1984", SPHEROID["WGS 84",6378137,298.257223563, AUTHORITY["EPSG","7030"]], AUTHORITY["EPSG","6326"]], PRIMEM["Greenwich",0, AUTHORITY["EPSG","8901"]], UNIT["degree",0.01745329251994328, AUTHORITY["EPSG","9122"]], AUTHORITY["EPSG","4326"]]

    //EPSG:2058 <> ED50(ED77) / UTM zone 38N
    //PROJCS["ED50(ED77) / UTM zone 38N", GEOGCS["ED50(ED77)", DATUM["European_Datum_1950_1977", SPHEROID["International 1924", 6378388, 297, AUTHORITY["EPSG", "7022"]], TOWGS84[-117, -132, -164, 0, 0, 0, 0], AUTHORITY["EPSG", "6154"]], PRIMEM["Greenwich", 0, AUTHORITY["EPSG", "8901"]], UNIT["degree", 0.0174532925199433, AUTHORITY["EPSG", "9122"]], AUTHORITY["EPSG", "4154"]], PROJECTION["Transverse_Mercator"], PARAMETER["latitude_of_origin", 0], PARAMETER["central_meridian", 45], PARAMETER["scale_factor", 0.9996], PARAMETER["false_easting", 500000], PARAMETER["false_northing", 0], UNIT["metre", 1, AUTHORITY["EPSG", "9001"]], AXIS["Easting", EAST], AXIS["Northing", NORTH], AUTHORITY["EPSG", "2058"]]

    //EPSG:2059 <> ED50(ED77) / UTM zone 39N
    //PROJCS["ED50(ED77) / UTM zone 39N", GEOGCS["ED50(ED77)", DATUM["European_Datum_1950_1977", SPHEROID["International 1924", 6378388, 297, AUTHORITY["EPSG", "7022"]], TOWGS84[-117, -132, -164, 0, 0, 0, 0], AUTHORITY["EPSG", "6154"]], PRIMEM["Greenwich", 0, AUTHORITY["EPSG", "8901"]], UNIT["degree", 0.0174532925199433, AUTHORITY["EPSG", "9122"]], AUTHORITY["EPSG", "4154"]], PROJECTION["Transverse_Mercator"], PARAMETER["latitude_of_origin", 0], PARAMETER["central_meridian", 51], PARAMETER["scale_factor", 0.9996], PARAMETER["false_easting", 500000], PARAMETER["false_northing", 0], UNIT["metre", 1, AUTHORITY["EPSG", "9001"]], AXIS["Easting", EAST], AXIS["Northing", NORTH], AUTHORITY["EPSG", "2059"]]

    //EPSG:2060 <> ED50(ED77) / UTM zone 40N
    //PROJCS["ED50(ED77) / UTM zone 40N", GEOGCS["ED50(ED77)", DATUM["European_Datum_1950_1977", SPHEROID["International 1924", 6378388, 297, AUTHORITY["EPSG", "7022"]], TOWGS84[-117, -132, -164, 0, 0, 0, 0], AUTHORITY["EPSG", "6154"]], PRIMEM["Greenwich", 0, AUTHORITY["EPSG", "8901"]], UNIT["degree", 0.0174532925199433, AUTHORITY["EPSG", "9122"]], AUTHORITY["EPSG", "4154"]], PROJECTION["Transverse_Mercator"], PARAMETER["latitude_of_origin", 0], PARAMETER["central_meridian", 57], PARAMETER["scale_factor", 0.9996], PARAMETER["false_easting", 500000], PARAMETER["false_northing", 0], UNIT["metre", 1, AUTHORITY["EPSG", "9001"]], AXIS["Easting", EAST], AXIS["Northing", NORTH], AUTHORITY["EPSG", "2060"]]

    //EPSG:2061 <> ED50(ED77) / UTM zone 41N
    //PROJCS["ED50(ED77) / UTM zone 41N", GEOGCS["ED50(ED77)", DATUM["European_Datum_1950_1977", SPHEROID["International 1924", 6378388, 297, AUTHORITY["EPSG", "7022"]], TOWGS84[-117, -132, -164, 0, 0, 0, 0], AUTHORITY["EPSG", "6154"]], PRIMEM["Greenwich", 0, AUTHORITY["EPSG", "8901"]], UNIT["degree", 0.0174532925199433, AUTHORITY["EPSG", "9122"]], AUTHORITY["EPSG", "4154"]], PROJECTION["Transverse_Mercator"], PARAMETER["latitude_of_origin", 0], PARAMETER["central_meridian", 63], PARAMETER["scale_factor", 0.9996], PARAMETER["false_easting", 500000], PARAMETER["false_northing", 0], UNIT["metre", 1, AUTHORITY["EPSG", "9001"]], AXIS["Easting", EAST], AXIS["Northing", NORTH], AUTHORITY["EPSG", "2061"]]


    static EPSG()
    {
        Add(new Projection("WGS 84", 4326));
        Add(new Projection("WGS 84/UTM Zone 39N", 32639, 0, 51, 0.9996, 500000, 0));
        Add(new Projection("ED50(ED77)/UTM Zone 38N", 2058, 0, 45, 0.9996, 500000, 0));
        Add(new Projection("ED50(ED77)/UTM Zone 39N", 2059, 0, 51, 0.9996, 500000, 0));
        Add(new Projection("ED50(ED77)/UTM Zone 40N", 2060, 0, 57, 0.9996, 500000, 0));
        Add(new Projection("ED50(ED77)/UTM Zone 41N", 2061, 0, 63, 0.9996, 500000, 0));

        ///Default = projections[2059];//ED50(ED77)/UTM Zone 39N
    }
    private static void Add(Projection projection)
    {
        if (projections.ContainsKey(projection.EPSGCode)) return;
        projections.Add(projection.EPSGCode, projection);
    }

    //public static Projection Default { get; private set; }
    static Dictionary<uint, Projection> projections = new Dictionary<uint, Projection>();
    public static IEnumerable<Projection> Projections { get { return projections.Values; } }

    public static void FromUTM(uint EPSGCode, bool inverse, double x, double y, out double Latitude, out double Longitude, uint defaultEPSGCode)
    {
        var projection = default(Projection);
        if (projections.ContainsKey(EPSGCode)) projection = projections[EPSGCode];
        else if (projections.ContainsKey(defaultEPSGCode)) projection = projections[defaultEPSGCode];

        if (projection == null) throw new ArgumentException($"EPSG:{EPSGCode} & EPSG:{defaultEPSGCode} not exist!");
        projection.FromUTM(inverse, x, y, out Latitude, out Longitude);
    }

    public class Projection
    {
        internal Projection(string name, uint ePSGCode)
        {
            Name = name;
            EPSGCode = ePSGCode;
            trans = null;
        }
        internal Projection(string name, uint ePSGCode, double LatitudeOfOrigin, double CentralMeridian, double ScaleFactor, double FalseEasting, double FalseNorthing)
        {
            Name = name;
            EPSGCode = ePSGCode;

            cFac = new CoordinateSystemFactory();

            ellipsoid = cFac.CreateFlattenedSphere("WGS 84", 6378137.000, 298.25722356300003, LinearUnit.Metre);


            datum = cFac.CreateHorizontalDatum("D WGS 1984", DatumType.HD_Geocentric, ellipsoid, null);
            gcs = cFac.CreateGeographicCoordinateSystem("GCS WGS 1984", AngularUnit.Degrees, datum,
                PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
                new AxisInfo("Lat", AxisOrientationEnum.North));
            List<ProjectionParameter> parameters = new List<ProjectionParameter>();
            parameters.Add(new ProjectionParameter("latitude_of_origin", LatitudeOfOrigin));
            parameters.Add(new ProjectionParameter("central_meridian", CentralMeridian));
            parameters.Add(new ProjectionParameter("scale_factor", ScaleFactor));
            parameters.Add(new ProjectionParameter("false_easting", FalseEasting));
            parameters.Add(new ProjectionParameter("false_northing", FalseNorthing));

            projection = cFac.CreateProjection(string.Format("WGS 1984 UTM Zone {0}", Name), "Transverse Mercator", parameters);
            coordsys = cFac.CreateProjectedCoordinateSystem("WGS 84", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
            trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);
        }

        public string Name { get; private set; }
        public uint EPSGCode { get; private set; }
        public override string ToString() => Name;

        CoordinateSystemFactory cFac;
        IEllipsoid ellipsoid;
        IHorizontalDatum datum;
        IGeographicCoordinateSystem gcs;
        IProjection projection;
        IProjectedCoordinateSystem coordsys;
        ICoordinateTransformation trans;



        //public void ToUTM(double Longitude, double Latitude, ref double Easting, ref double Northing)
        //{

        //    double[] pGeo = new double[] { Longitude, Latitude };
        //    double[] pUtm = trans.MathTransform.Transform(pGeo);
        //    Easting = pUtm[0];
        //    Northing = pUtm[1];
        //}

        public void FromUTM(bool inverse, double x, double y, out double Latitude, out double Longitude)
        {
            if (trans == null && EPSGCode == 4326)
            {
                Latitude = y;
                Longitude = x;
            }
            else
            {
                double[] pUtm = new double[] { x, y };
                double[] pGeo;
                if (inverse) pGeo = trans.MathTransform.Inverse().Transform(pUtm);
                else pGeo = trans.MathTransform.Transform(pUtm);
                Latitude = pGeo[1];
                Longitude = pGeo[0];
            }
        }
    }
}