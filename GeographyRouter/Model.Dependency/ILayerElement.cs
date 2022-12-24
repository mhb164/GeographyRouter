using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeographyRouter
{
    public interface ILayerElement
    {
        

        string Code { get; }
        bool Connected { get; }
        List<CoordinateRef> Coordinates { get; }
        CoordinateRef CoordinateFirst { get; }
        CoordinateRef CoordinateLast { get; }

        bool GeographyTypeIsPoint { get; }
        bool GeographyTypeIsLine { get; }
        bool GeographyTypeIsPolygon { get; }

        bool Routed { get; set; }
    }
}
