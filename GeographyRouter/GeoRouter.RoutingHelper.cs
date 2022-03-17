using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GeographyRouter
{
    public partial class GeoRouter
    {
        public class CreateNodeResult
        {
            public readonly Node Node;
            readonly List<ILayerElement> OutputLines;

            public CreateNodeResult(Node node, List<ILayerElement> outputLines)
            {
                Node = node;
                OutputLines = outputLines;
            }

            public IEnumerable<ILayerElement> GetMustRouteLines()
            {
                foreach (var item in OutputLines)
                {
                    if (item.Routed) continue;
                    if (Node.CrossedRoutes.Where(x => x.Elements.Contains(item)).Count() > 0) continue;//existed
                    yield return item;
                }
            }
        }

        public class CreateRouteParameters
        {
            public readonly GeoRouter Assistant;
            public readonly Routing Routing;
            public readonly ILayerElement Element;
            public readonly Node Node;
            public readonly Branch Branch;


            public CreateRouteParameters(GeoRouter assistant, Routing routing, ILayerElement element, Node node)
            {
                Assistant = assistant;
                Routing = routing;
                Element = element;
                Node = node;
                Branch = null;

                Create();
            }

            public CreateRouteParameters(GeoRouter assistant, Routing routing, ILayerElement element, Branch branch)
            {
                Assistant = assistant;
                Routing = routing;
                Element = element;
                Node = null;
                Branch = branch;

                Create();
            }

            public Route Route { get; private set; }

            public void Create()
            {
                if (Route != null) return;
                if (Node != null)
                {
                    CreateRouteByNode();
                }
                else if (Branch != null)
                {
                    CreateRouteByBranch();
                }
                Assistant.Add(Element, Route);
            }

            private void CreateRouteByNode()
            {
                Route = new Route(Routing, Node, Element);
                Node.AddCrossRoute(Route);
                Routing.Add(Route, Node.Precedence);
            }

            private void CreateRouteByBranch()
            {
                Route = new Route(Routing, Branch, Element);
                Branch.AddCrossRoute(Route);
                Routing.Add(Route, Branch.Precedence);
            }


        }
    }
}