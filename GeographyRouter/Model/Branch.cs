using System.Collections.Generic;

namespace GeographyRouter
{
    public class Branch : RoutingItem
    {
        internal Branch(Routing owner, Route preroute, CoordinateRef coordinate)
        {
            Owner = owner;
            CrossedRoutes = new List<Route>();
            Coordinate = coordinate;

            AddCrossRoute(preroute);
        }

        public override string ToString() => $"Branch @ {Coordinate}";
        public CoordinateRef Coordinate { get; private set; }
        public List<Route> CrossedRoutes { get; private set; }

        internal void AddCrossRoute(Route route)
        {
            if (route == null) return;
            CrossedRoutes.Add(route);
        }
    }
}
