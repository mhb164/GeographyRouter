using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeographyRouter
{
    public interface ILayerElement
    {
        //Guid Id { get; set; }
        //bool Activation { get; set; }
        string Code { get; }
        //long Version { get; set; }
        //double[] Points { get; set; }
        //string FieldValuesText { get; set; }

        bool Routed { get; set; }
        bool IsClosed { get; }
        List<CoordinateRef> Coordinates { get; }
        CoordinateRef CoordinateFirst { get; }
        CoordinateRef CoordinateLast { get; }
        LayerGeographyType GeographyType { get; }
    }
}
