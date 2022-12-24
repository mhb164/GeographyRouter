using System.Collections.Generic;

namespace GeographyRouter
{
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
                if (!Owner.ItemsByPrecedence.TryGetValue(subject.PrePrecedence, out var newSubject)) break;
                subject = newSubject;
                upcomming.Add(subject);
            }
            return upcomming;
        }

        public List<uint> FillDowngoing() => FillDowngoing(this);
        public static List<uint> FillDowngoing(RoutingItem root)
        {
            HashSet<uint> precedences = new HashSet<uint>();
            Queue<RoutingItem> queue = new Queue<RoutingItem>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                //if (precedences.Contains(current.Precedence) == false) 
                precedences.Add(current.Precedence);

                foreach (var nextPrecedence in current.NextPrecedences)
                    queue.Enqueue(current.Owner.ItemsByPrecedence[nextPrecedence]);
            }

            return new List<uint>(precedences);
        }

        public List<uint> FillUpcomming() => FillUpcomming(this);
        public static List<uint> FillUpcomming(RoutingItem root)
        {
            HashSet<uint> precedences = new HashSet<uint>();
            Queue<RoutingItem> queue = new Queue<RoutingItem>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                precedences.Add(current.Precedence);

                if (current.Owner.ItemsByPrecedence.TryGetValue(current.PrePrecedence, out var other))
                    queue.Enqueue(other);
            }

            return new List<uint>(precedences);
        }

    }
}
