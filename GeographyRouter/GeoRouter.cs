using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GeographyRouter
{
    public partial class GeoRouter
    {
        readonly Config Config;
        readonly IGeoRepository Repo;
        readonly Action<string> LogAction;
        public GeoRouter(Config config, IGeoRepository repo, Action<string> logAction)
        {
            Config = config;
            Repo = repo;
            LogAction = logAction;

            start();
        }

        private List<Routing> routings = new List<Routing>();
        private Dictionary<string, RoutingItem> routingPairs = new Dictionary<string, RoutingItem>();
        private List<List<string>> _collisions = new List<List<string>>();

        public IEnumerable<Routing> Routings => routings;
        public IEnumerable<IEnumerable<string>> Collisions => _collisions;

        private void Log(string message) => LogAction?.Invoke(message);

        public RoutingItem GetRouting(string code)
        {
            if (routingPairs.TryGetValue(code, out var routingItem))
                return routingItem;
            else 
                return null;
        }

        private void Add(ILayerElement element, RoutingItem routingItem)
        {
            if (routingPairs.ContainsKey(element.Code)) return;
            element.Routed = true;
            routingPairs.Add(element.Code, routingItem);
        }
    }
}
