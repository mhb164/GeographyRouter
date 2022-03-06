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
}
