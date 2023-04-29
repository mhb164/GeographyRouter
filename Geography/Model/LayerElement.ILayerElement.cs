using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeographyModel
{
    public partial class LayerElement : GeographyRouter.ILayerElement
    {
        string GeographyRouter.ILayerElement.Code => Code;

        bool GeographyRouter.ILayerElement.GeographyTypeIsPoint => Layer.GeographyType == LayerGeographyType.Point;

        bool GeographyRouter.ILayerElement.GeographyTypeIsLine => Layer.GeographyType == LayerGeographyType.Polyline;

        bool GeographyRouter.ILayerElement.GeographyTypeIsPolygon => Layer.GeographyType == LayerGeographyType.Polygon;

        public List<GeographyRouter.CoordinateRef> Coordinates { get; private set; }

        public GeographyRouter.CoordinateRef CoordinateFirst { get; private set; }

        public GeographyRouter.CoordinateRef CoordinateLast { get; private set; }

        public bool Routed { get; set; }

        public void ResetRouting()
        {
            Routed = false;
            Coordinates = new List<GeographyRouter.CoordinateRef>();
            CoordinateFirst = null;
            CoordinateLast = null;
            //-----------
            for (int i = 0; i < Points.Length / 2; i++)
            {
                var i1 = i * 2;
                var i2 = (i * 2) + 1;
                Coordinates.Add(new GeographyRouter.CoordinateRef(() => Points[i1], () => Points[i2]));
            }
            //-----------
            if (Coordinates.Count > 0) CoordinateFirst = Coordinates[0];
            if (Coordinates.Count > 1) CoordinateLast = Coordinates[Coordinates.Count - 1];
        }


    }

}
