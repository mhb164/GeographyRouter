using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace GeographyModel
{
    public partial class LayerElement : GeographyRouter.ILayerElement
    {
        [IgnoreDataMember, ScriptIgnore]
        public bool Connected
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

        [IgnoreDataMember, ScriptIgnore]
        bool GeographyRouter.ILayerElement.GeographyTypeIsPoint => Layer.GeographyType == LayerGeographyType.Point;

        [IgnoreDataMember, ScriptIgnore]
        bool GeographyRouter.ILayerElement.GeographyTypeIsLine => Layer.GeographyType == LayerGeographyType.Polyline;

        [IgnoreDataMember, ScriptIgnore]
        bool GeographyRouter.ILayerElement.GeographyTypeIsPolygon => Layer.GeographyType == LayerGeographyType.Polygon;

        [IgnoreDataMember, ScriptIgnore]
        public List<GeographyRouter.CoordinateRef> Coordinates { get; private set; }

        [IgnoreDataMember, ScriptIgnore]
        public GeographyRouter.CoordinateRef CoordinateFirst { get; private set; }

        [IgnoreDataMember, ScriptIgnore]
        public GeographyRouter.CoordinateRef CoordinateLast { get; private set; }

        [IgnoreDataMember, ScriptIgnore]
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
