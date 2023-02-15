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
            foreach (var item in RoutingSources.OrderBy(x => x.Code))
            {
                BuildRouting(item);
            }

            foreach (var item in routings)
            {
                stopwatchDetail.Restart();
                try
                {
                    var routing = CreateRouting(item);
                    stopwatchDetail.Stop();
                    routingDetail.AppendLine($"\t{counter++,3}> Route Ok on {item.Source.Code} @ {stopwatchDetail.Elapsed.TotalSeconds:N3}s");
                    Log($"{counter} of {RoutingSources.Count} Route Ok on {item.Source.Code}({routing.Items.Count}) @ {stopwatchDetail.Elapsed.TotalSeconds:N3}s {item.Source.Coordinates.FirstOrDefault()}");
                }
                catch (Exception ex)
                {
                    stopwatchDetail.Stop();
                    routingDetail.AppendLine($"\t{counter++,3}> Route Failed on {item.Source.Code} @ {stopwatchDetail.Elapsed.TotalSeconds:N3}");
                    Log($"Exception on {item.Source.Code} route: {ex.Message}");
                }
            }


            var collisions = new Dictionary<string, List<Routing>>();

            foreach (var routing in routings)
            {
                var result = HitTest(routing.Source.CoordinateFirst, false);
                foreach (var item in result)
                {
                    var routingItem = GetRouting(item.Code);
                    if (routingItem.Owner != routing)
                    {
                        if (!collisions.TryGetValue(routingItem.Owner.Source.Code, out var collision))
                        {
                            collision = new List<Routing>();
                            collisions.Add(routingItem.Owner.Source.Code, collision);
                        }
                        collision.Add(routing);
                        break;
                    }
                }
            }

            _collisions = collisions.Select(x =>
            {
                var newCollision = new List<string>() { x.Key };
                newCollision.AddRange(x.Value.Select(y => y.Source.Code));
                return newCollision;
            }).ToList();

            stopwatch.Stop();
            Log($"Routing finished({stopwatch.Elapsed.TotalSeconds:N3}s)");
        }

        private void BuildRouting(ILayerElement source)
        {
            var routing = new Routing(source);
            routings.Add(routing);

            var sourceNode = new Node(routing, null, source);
            routing.Add(sourceNode, 0);
            this.Add(source, sourceNode);
        }

        private Routing CreateRouting(Routing routing)
        {
            //var routing = new Routing(source);
            //routings.Add(routing);
            try
            {
                CreateRoutingFromSource(this, routing);
            }
            catch (Exception ex)
            {
                Log($"Create routing has error {routing.Source.Code} {ex}");
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

        private static bool CheckIsDeadend(GeoRouter assistant, IEnumerable<ILayerElement> HitTestResultPoints, Node newnode, ILayerElement element)
        {
            if (assistant.Config.CheckAllElementsConnectedInNode)
            {
                foreach (var item in HitTestResultPoints)
                {
                    if (!item.Connected)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                foreach (var item in HitTestResultPoints)
                {
                    if (item.Connected)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private static void FillNodeItems(GeoRouter assistant, IEnumerable<ILayerElement> HitTestResultPoints, Node node, ILayerElement element)
        {
            foreach (var item in HitTestResultPoints)
            {
                if (item == element) continue;
                if (item.Routed)
                    throw new ApplicationException($"Node already analyzed! ({node.Coordinate}, {item.Code})");
                //if (item.Connected == false)
                //{
                //    return;
                //}
                assistant.Add(item, node);
                node.Add(item);
            }
        }

        private static void CreateRoutingFromSource(GeoRouter assistant, Routing routing)
        {
            //var sourceNode = new Node(routing, null, source);
            //routing.Add(sourceNode, 0);
            //assistant.Add(source, sourceNode);


            var HitTestResult = assistant.HitTest(routing.SourceNode.Coordinate, false);
            var IsDeadend = CheckIsDeadend(assistant, HitTestResult.Where(x => x.GeographyTypeIsPoint), routing.SourceNode, routing.Source);
            FillNodeItems(assistant, HitTestResult.Where(x => x.GeographyTypeIsPoint), routing.SourceNode, routing.Source);

            if (!IsDeadend)
            {
                foreach (var item in HitTestResult.Where(x => x.GeographyTypeIsLine))
                {
                    if (item.Routed) continue;
                    if (!item.Connected) continue;
                    if (routing.SourceNode.CrossedRoutes.Where(x => x.Elements.Contains(item)).Count() > 0) continue;//existed
                                                                                                                     //CreateRoute(new CreateRouteParameters(assistant, routing, item, node));
                    CreateRouteByStack(new CreateRouteParameters(assistant, routing, item, routing.SourceNode));
                }
            }
        }

        //private static void CreateRoute(CreateRouteParameters parameters)
        //{
        //    while (true)
        //    {
        //        if (parameters.Route.Output == null)
        //        {
        //            foreach (var output in parameters.Route.GetOutputs())
        //                if (parameters.Assistant.HitTest(output, false).Count > 0)
        //                {
        //                    parameters.Route.SetOutput(output);
        //                    break;
        //                }
        //            if (parameters.Route.Output == null)
        //                throw new ApplicationException($"Route output is null ({string.Join(",", parameters.Route.Elements.Select(x => x.Code))}, input: {parameters.Route.Input})");
        //        }
        //        var HitTestResult = parameters.Assistant.HitTest(parameters.Route.Output, true);
        //        var HitTestResultPoints = HitTestResult.Where(x => x.GeographyTypeIsPoint).ToList();
        //        var HitTestResultLines = HitTestResult.Where(x => x.GeographyTypeIsLine && x != parameters.Element).ToList();

        //        if (HitTestResultPoints.Count() > 0)//Node
        //        {
        //            var createNodeResult = CreateNode(parameters.Assistant, parameters.Routing, parameters.Route, HitTestResultPoints.First());
        //            parameters.Route.AddCrossPoint(createNodeResult.Node);//??
        //            foreach (var item in createNodeResult.GetMustRouteLines())
        //            {
        //                CreateRoute(new CreateRouteParameters(parameters.Assistant, parameters.Routing, item, createNodeResult.Node));
        //            }
        //            break;
        //        }
        //        else if (HitTestResultLines.Count() == 0)//End
        //        {
        //            //bool choosed = false;
        //            //foreach (var output in route.GetOutputs())
        //            //    if (Assistant.HitTest(output).Where(x => x.Routed == false).Count() > 0)
        //            //    {
        //            //        route.SetOutput(output);
        //            //        choosed = true;
        //            //        break;
        //            //    }
        //            ////Log($"Route end ({route.Output}, {string.Join(",", HitTestResultLines.Select(x => x.Code))})");
        //            //if(choosed== false)
        //            return;
        //        }
        //        else if (HitTestResultLines.Count() > 1)//Lines
        //        {
        //            var newbranch = CreateBranch(parameters.Routing, parameters.Route, parameters.Route.Output);
        //            parameters.Route.Add(newbranch);

        //            foreach (var item in HitTestResultLines.ToList())
        //            {
        //                if (item.Routed) continue;
        //                CreateRoute(new CreateRouteParameters(parameters.Assistant, parameters.Routing, item, newbranch));
        //            }
        //            break;
        //        }
        //        else
        //        {
        //            var newline = HitTestResultLines.First();
        //            parameters.Assistant.Add(newline, parameters.Route);
        //            parameters.Route.Add(newline);
        //        }
        //    }
        //}

        private static void CreateRouteByStack(CreateRouteParameters root)
        {
            Queue<CreateRouteParameters> queue = new Queue<CreateRouteParameters>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var parameters = queue.Dequeue();
                while (true)
                {
                    if (parameters.Route.Output == null)
                    {
                        foreach (var output in parameters.Route.GetOutputs())
                            if (parameters.Assistant.HitTest(output, false).Count > 0)
                            {
                                parameters.Route.SetOutput(output);
                                break;
                            }
                        if (parameters.Route.Output == null)
                            throw new ApplicationException($"Route output is null ({string.Join(",", parameters.Route.Elements.Select(x => x.Code))}, input: {parameters.Route.Input})");
                    }
                    var HitTestResult = parameters.Assistant.HitTest(parameters.Route.Output, true);
                    var HitTestResultPoints = HitTestResult.Where(x => x.GeographyTypeIsPoint).ToList();
                    var HitTestResultLines = HitTestResult.Where(x => x.GeographyTypeIsLine && x != parameters.Element).ToList();

                    if (HitTestResultPoints.Count() > 0)//Node
                    {
                        var createNodeResult = CreateNode(parameters.Assistant, parameters.Routing, parameters.Route, HitTestResultPoints.First());
                        parameters.Route.AddCrossPoint(createNodeResult.Node);//??
                        foreach (var item in createNodeResult.GetMustRouteLines())
                        {
                            //CreateRoute(new CreateRouteParameters(parameters.Assistant, parameters.Routing, item, createNodeResult.Node));
                            queue.Enqueue(new CreateRouteParameters(parameters.Assistant, parameters.Routing, item, createNodeResult.Node));
                        }
                        break;
                    }
                    else if (!HitTestResultLines.Any())//End
                    {
                        break;//return;
                    }
                    else if (HitTestResultLines.Count > 1)//Lines
                    {
                        var newbranch = CreateBranch(parameters.Routing, parameters.Route, parameters.Route.Output);
                        parameters.Route.Add(newbranch);

                        foreach (var item in HitTestResultLines.ToList())
                        {
                            if (item.Routed) continue;
                            if (!item.Connected) continue;
                            //CreateRoute(new CreateRouteParameters(parameters.Assistant, parameters.Routing, item, newbranch));
                            queue.Enqueue(new CreateRouteParameters(parameters.Assistant, parameters.Routing, item, newbranch));

                        }
                        break;
                    }
                    else
                    {
                        var newline = HitTestResultLines.First();
                        parameters.Assistant.Add(newline, parameters.Route);
                        parameters.Route.Add(newline);
                    }
                }
            }
        }


        private static CreateNodeResult CreateNode(GeoRouter assistant, Routing routing, Route preroute, ILayerElement element)
        {
            var newNode = new Node(routing, preroute, element);
            routing.Add(newNode, preroute.Precedence);
            assistant.Add(element, newNode);

            var HitTestResult = assistant.HitTest(newNode.Coordinate, false);
            var IsDeadend = CheckIsDeadend(assistant, HitTestResult.Where(x => x.GeographyTypeIsPoint), newNode, element);
            FillNodeItems(assistant, HitTestResult.Where(x => x.GeographyTypeIsPoint), newNode, element);

            if (IsDeadend)
            {
                return new CreateNodeResult(newNode);
            }
            else
            {
                return new CreateNodeResult(newNode, HitTestResult.Where(x => x.GeographyTypeIsLine).ToList());
            }
        }


        private static Branch CreateBranch(Routing routing, Route preroute, CoordinateRef coordinate)
        {
            var newbranch = new Branch(routing, preroute, coordinate);
            routing.Add(newbranch, preroute.Precedence);
            return newbranch;
        }


    }
}
