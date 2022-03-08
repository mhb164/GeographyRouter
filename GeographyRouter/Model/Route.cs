using System.Collections.Generic;
using System.Linq;

namespace GeographyRouter
{
    public class Route : RoutingItem
    {
        internal Route(Routing owner, Node prenode, ILayerElement element)
        {
            Owner = owner;
            Elements = new List<ILayerElement>();
            CrossedNodes = new List<Node>();
            Branches = new List<Branch>();

            Input = prenode.Coordinate;
            AddCrossPoint(prenode);

            Add(element);

        }
        internal Route(Routing owner, Branch branch, ILayerElement element)
        {
            Owner = owner;
            Elements = new List<ILayerElement>();
            CrossedNodes = new List<Node>();
            Branches = new List<Branch>();

            Input = branch.Coordinate;
            Add(branch);

            Add(element);

        }


        public List<ILayerElement> Elements { get; private set; }
        public List<Node> CrossedNodes { get; private set; }
        public List<Branch> Branches { get; private set; }

        public CoordinateRef Input { get; private set; }
        public CoordinateRef Output { get; private set; }


        internal void Add(ILayerElement line)
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
}
