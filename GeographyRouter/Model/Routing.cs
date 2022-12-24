using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GeographyRouter
{
    public class Routing
    {
        public Routing(ILayerElement source)
        {
            Source = source;
            Items = new List<RoutingItem>();
            Routes = new List<Route>();
            Nodes = new List<Node>();
            Branches = new List<Branch>();

            MaxPrecedence = 0;
            ItemsByPrecedence = new Dictionary<uint, RoutingItem>();
        }
        public ILayerElement Source { get; private set; }
        public Node SourceNode { get; private set; }
        public List<RoutingItem> Items { get; private set; }
        public List<Route> Routes { get; private set; }
        public List<Node> Nodes { get; private set; }
        public List<Branch> Branches { get; private set; }

        public Dictionary<uint, RoutingItem> ItemsByPrecedence { get; private set; }
        public RoutingItem GetItemsByPrecedence(uint prePrecedence)
        {
            if (ItemsByPrecedence.TryGetValue(prePrecedence, out var routingItem))
                return routingItem;
            else
                return null;
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
            if (prePrecedence == 0)
            {
                SourceNode = item;
            }
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
}
