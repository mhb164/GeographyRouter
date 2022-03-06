using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeographyModel;

namespace GeographyRouter
{
    public interface IGeoRepository
    {
        void ResetRouting();
        List<LayerElement> GetRoutingSources();
        void RoutingHitTest(double latitude, double longitude, ref List<LayerElement> result, bool justNotRoute);
    }

    public class Routing
    {
        public Routing(LayerElement source)
        {
            Source = source;
            Items = new List<RoutingItem>();
            Routes = new List<Route>();
            Nodes = new List<Node>();
            Branches = new List<Branch>();

            MaxPrecedence = 0;
            ItemsByPrecedence = new Dictionary<uint, RoutingItem>();
        }
        public LayerElement Source { get; private set; }
        public List<RoutingItem> Items { get; private set; }
        public List<Route> Routes { get; private set; }
        public List<Node> Nodes { get; private set; }
        public List<Branch> Branches { get; private set; }

        public Dictionary<uint, RoutingItem> ItemsByPrecedence { get; private set; }
        public RoutingItem GetItemsByPrecedence(uint prePrecedence)
        {
            if (ItemsByPrecedence.ContainsKey(prePrecedence)) return ItemsByPrecedence[prePrecedence];
            else return null;
        }

        internal void Add(Route item, uint prePrecedence)
        {
            item.PrePrecedence = prePrecedence;
            item.Precedence = ++MaxPrecedence;

            Items.Add(item);
            Routes.Add(item);
        }
        internal void Add(Node item, uint prePrecedence)
        {
            item.PrePrecedence = prePrecedence;
            item.Precedence = ++MaxPrecedence;
            Items.Add(item);
            Nodes.Add(item);
        }
        internal void Add(Branch item, uint prePrecedence)
        {
            item.PrePrecedence = prePrecedence;
            item.Precedence = ++MaxPrecedence;
            Items.Add(item);
            Branches.Add(item);
        }

        public uint MaxPrecedence { get; private set; }
        public override string ToString() => $"{Source.Code} routing";


    }

    public abstract class RoutingItem
    {
        public Routing Owner { get; protected set; }
        public uint PrePrecedence { get; set; }
        public uint Precedence { get; set; }
        public List<uint> NextPrecedences { get; set; } = new List<uint>();

        public List<RoutingItem> GetUpcomming()
        {
            var upcomming = new List<RoutingItem>();
            var subject = this;
            while (true)
            {
                if (Owner.ItemsByPrecedence.ContainsKey(subject.PrePrecedence) == false) break;
                subject = Owner.ItemsByPrecedence[subject.PrePrecedence];
                upcomming.Add(subject);
            }
            return upcomming;
        }

        public void FillDowngoing(ref List<uint> precedences)
        {
            if (precedences.Contains(Precedence) == false) precedences.Add(Precedence);
            foreach (var nextPrecedence in NextPrecedences) Owner.ItemsByPrecedence[nextPrecedence].FillDowngoing(ref precedences);
        }
        public void FillUpcomming(ref List<uint> precedences)
        {
            if (precedences.Contains(Precedence) == false) precedences.Add(Precedence);
            if (Owner.ItemsByPrecedence.ContainsKey(PrePrecedence)) Owner.ItemsByPrecedence[PrePrecedence].FillUpcomming(ref precedences);
        }

        //private void FillUpcomming(List<RoutingItem> upcomming)
        //{
        //    if (Owner.ItemsByPrecedence.ContainsKey(PrePrecedence)== false) return;
        //    var pre = Owner.ItemsByPrecedence[PrePrecedence];
        //    upcomming.Add(pre);
        //    pre.FillUpcomming(upcomming);
        //}
    }

    public class Route : RoutingItem
    {
        internal Route(Routing owner, Node prenode, LayerElement element)
        {
            Owner = owner;
            Elements = new List<LayerElement>();
            CrossedNodes = new List<Node>();
            Branches = new List<Branch>();

            Input = prenode.Coordinate;
            AddCrossPoint(prenode);

            Add(element);

        }
        internal Route(Routing owner, Branch branch, LayerElement element)
        {
            Owner = owner;
            Elements = new List<LayerElement>();
            CrossedNodes = new List<Node>();
            Branches = new List<Branch>();

            Input = branch.Coordinate;
            Add(branch);

            Add(element);

        }


        public List<LayerElement> Elements { get; private set; }
        public List<Node> CrossedNodes { get; private set; }
        public List<Branch> Branches { get; private set; }

        public CoordinateRef Input { get; private set; }
        public CoordinateRef Output { get; private set; }


        internal void Add(LayerElement line)
        {
            Elements.Add(line);
            var first = Output;
            if (first == null) first = Input;

            if (line.CoordinateFirst == first) Output = line.CoordinateLast;
            else if (line.CoordinateLast == first) Output = line.CoordinateFirst;
            else Output = null;

        }
        internal void AddCrossPoint(Node node)
        {
            if (node == null) return;
            CrossedNodes.Add(node);
        }
        internal void Add(Branch item) => Branches.Add(item);

        internal List<CoordinateRef> GetOutputs()
        {
            var result = new List<CoordinateRef>();
            var line = Elements.LastOrDefault();
            if (line != null)
            {
                var first = Output;
                if (first == null) first = Input;

                var lineStart = line.CoordinateFirst;
                var lineEnd = line.CoordinateLast;
                if (lineStart != first && CrossedNodes.Where(x => x.Coordinate == lineStart).Count() == 0 && Branches.Where(x => x.Coordinate == lineStart).Count() == 0)
                    result.Add(lineStart);
                if (lineEnd != first && CrossedNodes.Where(x => x.Coordinate == lineEnd).Count() == 0 && Branches.Where(x => x.Coordinate == lineEnd).Count() == 0)
                    result.Add(lineEnd);

                //foreach (var item in line.Coordinates)
                //    if (item != first && CrossedNodes.Where(x => x.Coordinate == item).Count() == 0 && Branches.Where(x => x.Coordinate == item).Count() == 0)
                //        result.Add(item);
            }
            return result;
        }

        internal void SetOutput(CoordinateRef output) => Output = output;
        public override string ToString() => $"Route {Input} ~ {Output} ({string.Join(",", Elements.Select(x => x.Code))})";
    }

    public class Node : RoutingItem
    {
        internal Node(Routing owner, Route preroute, LayerElement element)
        {
            Owner = owner;
            Elements = new List<LayerElement>();
            CrossedRoutes = new List<Route>();

            Add(element);
            AddCrossRoute(preroute);
        }

        public CoordinateRef Coordinate => Elements[0].Coordinates[0];

        public List<LayerElement> Elements { get; private set; }
        public List<Route> CrossedRoutes { get; private set; }

        internal void Add(LayerElement element)
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
