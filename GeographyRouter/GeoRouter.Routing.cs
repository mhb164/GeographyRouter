using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GeographyRouter
{
    public partial class GeoRouter
    {
        private void start()
        {
            Log($"Reset Routing start...");
            var stopwatch = Stopwatch.StartNew();
            Repo.ResetRouting();
            stopwatch.Stop();
            Log($"Reset Routing finished({stopwatch.Elapsed.TotalSeconds:N3}s)");
            //-------------------
            var routingDetail = new StringBuilder();
            var stopwatchDetail = new Stopwatch();
            var counter = 1;
            stopwatch.Restart();

            Log($"Routing started");
            var RoutingSources = Repo.GetRoutingSources();
            foreach (var item in RoutingSources.OrderBy(x => x.Code))//Repo.GetLayer("MVPT_HEADER").OrderBy(x => x.Code))
            {
                stopwatchDetail.Restart();
                try
                {
                    var routing = CreateRouting(item);
                    stopwatchDetail.Stop();
                    routingDetail.AppendLine($"\t{counter++,3}> Route Ok on {item.Code} @ {stopwatchDetail.Elapsed.TotalSeconds:N3}s");
                    Log($"{counter} of {RoutingSources.Count} Route Ok on {item.Code}({routing.Items.Count}) @ {stopwatchDetail.Elapsed.TotalSeconds:N3}s {item.Coordinates.FirstOrDefault()}");
                }
                catch (Exception ex)
                {
                    stopwatchDetail.Stop();
                    routingDetail.AppendLine($"\t{counter++,3}> Route Failed on {item.Code} @ {stopwatchDetail.Elapsed.TotalSeconds:N3}");
                    Log($"Exception on {item.Code} route: {ex.Message}");
                }
            }

            stopwatch.Stop();
            Log($"Routing finished({stopwatch.Elapsed.TotalSeconds:N3}s)");
        }

        private Routing CreateRouting(ILayerElement source)
        {
            var routing = new Routing(source);
            routings.Add(routing);
            try
            {
                var node = new Node(routing, null, source);
                routing.Add(node, 0);
                this.Add(source, node);


                var HitTestResult = this.HitTest(node.Coordinate, false);
                FillNodeItems(this, HitTestResult.Where(x => x.GeographyTypeIsPoint), node, source);

                foreach (var item in HitTestResult.Where(x => x.GeographyTypeIsLine))
                {
                    if (item.Routed) continue;
                    if (node.CrossedRoutes.Where(x => x.Elements.Contains(item)).Count() > 0) continue;//existed
                    CreateRoute(this, ref routing, null, node, item);
                }
            }
            catch (Exception ex)
            {
                Log($"Create routing has error {source.Code} {ex}");
            }

            foreach (var routingitem in routing.Items)
            {
                routing.ItemsByPrecedence.Add(routingitem.Precedence, routingitem);

                //------------------------
                if (routingitem is Route)
                {
                    foreach (var item in (routingitem as Route).Branches)
                        if (item.Precedence > routingitem.Precedence)
                            routingitem.NextPrecedences.Add(item.Precedence);
                    foreach (var item in (routingitem as Route).CrossedNodes)
                        if (item.Precedence > routingitem.Precedence)
                            routingitem.NextPrecedences.Add(item.Precedence);
                }
                else if (routingitem is Node)
                {
                    foreach (var item in (routingitem as Node).CrossedRoutes)
                        if (item.Precedence > routingitem.Precedence)
                            routingitem.NextPrecedences.Add(item.Precedence);
                }
                else if (routingitem is Branch)
                {
                    foreach (var item in (routingitem as Branch).CrossedRoutes)
                        if (item.Precedence > routingitem.Precedence)
                            routingitem.NextPrecedences.Add(item.Precedence);
                }
            }
            return routing;
        }
        public List<ILayerElement> HitTest(CoordinateRef coordinate, bool justNotRoute)
        {
            var result = new List<ILayerElement>();
            Repo.RoutingHitTest(coordinate.Latitude, coordinate.Longitude, ref result, justNotRoute);
            return result;
        }      

        private static void FillNodeItems(GeoRouter assistant, IEnumerable<ILayerElement> HitTestResultNodes, Node node, ILayerElement element)
        {
            foreach (var item in HitTestResultNodes)
            {
                if (item == element) continue;
                if (item.Routed)
                    throw new ApplicationException($"Node already analyzed! ({node.Coordinate}, {item.Code})");
                if (item.Connected == false)
                {
                    return;
                }
                assistant.Add(item, node);

                node.Add(item);
            }
        }
      
        private static void CreateRoute(GeoRouter assistant, ref Routing routing, Branch branch, Node node, ILayerElement element)
        {
            var route = default(Route);
            if (node != null)
            {
                route = new Route(routing, node, element);
                node.AddCrossRoute(route);
                routing.Add(route, node.Precedence);
            }
            else
            {
                route = new Route(routing, branch, element);
                branch.AddCrossRoute(route);
                routing.Add(route, branch.Precedence);
            }
            assistant.Add(element, route);

            while (true)
            {
                if (route.Output == null)
                {
                    foreach (var output in route.GetOutputs())
                        if (assistant.HitTest(output, false).Count > 0)
                        {
                            route.SetOutput(output);
                            break;
                        }
                    if (route.Output == null)
                        throw new ApplicationException($"Route output is null ({string.Join(",", route.Elements.Select(x => x.Code))}, input: {route.Input})");
                }
                var HitTestResult = assistant.HitTest(route.Output, true);
                var HitTestResultPoints = HitTestResult.Where(x => x.GeographyTypeIsPoint).ToList();
                var HitTestResultLines = HitTestResult.Where(x => x.GeographyTypeIsLine && x != element).ToList();

                if (HitTestResultPoints.Count() > 0)//Node
                {
                    var newnode = CreateNode(assistant, ref routing, route, HitTestResultPoints.First());
                    route.AddCrossPoint(newnode);
                    break;
                }
                else if (HitTestResultLines.Count() == 0)//End
                {
                    //bool choosed = false;
                    //foreach (var output in route.GetOutputs())
                    //    if (Assistant.HitTest(output).Where(x => x.Routed == false).Count() > 0)
                    //    {
                    //        route.SetOutput(output);
                    //        choosed = true;
                    //        break;
                    //    }
                    ////Log($"Route end ({route.Output}, {string.Join(",", HitTestResultLines.Select(x => x.Code))})");
                    //if(choosed== false)
                    return;
                }
                else if (HitTestResultLines.Count() > 1)//Lines
                {
                    var newbranch = CreateBranch(assistant, ref routing, route, route.Output, HitTestResultLines.ToList());
                    route.Add(newbranch);
                    break;
                }
                else
                {
                    var newline = HitTestResultLines.First();
                    assistant.Add(newline, route);
                    route.Add(newline);
                }
            }
        }

        private static Node CreateNode(GeoRouter assistant, ref Routing routing, Route preroute, ILayerElement element)
        {
            var node = new Node(routing, preroute, element);
            if (preroute == null) routing.Add(node, 0);
            else routing.Add(node, preroute.Precedence);
            assistant.Add(element, node);

            var HitTestResult = assistant.HitTest(node.Coordinate, false);
            FillNodeItems(assistant, HitTestResult.Where(x => x.GeographyTypeIsPoint), node, element);

            foreach (var item in HitTestResult.Where(x => x.GeographyTypeIsLine))
            {
                if (item.Routed) continue;
                if (node.CrossedRoutes.Where(x => x.Elements.Contains(item)).Count() > 0) continue;//existed
                CreateRoute(assistant, ref routing, null, node, item);
            }

            return node;
        }

        private static Branch CreateBranch(GeoRouter assistant, ref Routing routing, Route preroute, CoordinateRef coordinate, List<ILayerElement> HitTestResult)
        {
            var branch = new Branch(routing, preroute, coordinate);
            routing.Add(branch, preroute.Precedence);

            foreach (var item in HitTestResult)
            {
                if (item.Routed) continue;
                CreateRoute(assistant, ref routing, branch, null, item);
            }

            return branch;
        }


    }
}
