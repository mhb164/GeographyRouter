using System.Collections.Generic;
using System.Linq;

namespace GeographyRouter
{
    public class Node : RoutingItem
    {
        internal Node(Routing owner, Route preroute, ILayerElement element)
        {
            Owner = owner;
            Elements = new List<ILayerElement>();
            CrossedRoutes = new List<Route>();

            Add(element);
            AddCrossRoute(preroute);
        }

        public CoordinateRef Coordinate => Elements[0].Coordinates[0];

        public List<ILayerElement> Elements { get; private set; }
        public List<Route> CrossedRoutes { get; private set; }

        internal void Add(ILayerElement element)
        {
            Elements.Add(element);
        }
        internal void AddCrossRoute(Route route)
        {
            if (route == null) return;
            CrossedRoutes.Add(route);
        }

        public override string ToString() => $"Node @ {Coordinate} ({string.Join(",", Elements.Select(x => x.Code))})";
    }
}
