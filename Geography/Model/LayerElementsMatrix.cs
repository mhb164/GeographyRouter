using System;
using System.Collections.Generic;

namespace GeographyModel
{
    public abstract partial class LayerElementsMatrix
    {
        private readonly Func<Guid, LayerElement> GetElementById_Func;
        public LayerElementsMatrix(Func<Guid, LayerElement> getElementById_Func)
        {
            GetElementById_Func = getElementById_Func;
        }

        protected LayerElement GetElement(Guid id) => GetElementById_Func?.Invoke(id);
        public abstract void Add(LayerElement element);
        public abstract void Remove(LayerElement element);
        public abstract void HitTest(ref double latitude, ref double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute);
    }

    public partial class LayerElementsMatrixByPoint : LayerElementsMatrix
    {
        public LayerElementsMatrixByPoint(Func<Guid, LayerElement> getElementById_Func) : base(getElementById_Func) { }

        Dictionary<Guid, List<ulong>> lookupsByElements = new Dictionary<Guid, List<ulong>>();
        Dictionary<ulong, Dictionary<ulong, List<Guid>>> lookups = new Dictionary<ulong, Dictionary<ulong, List<Guid>>>();
        static ulong CreateKey1(double latitude, double longitude) => (ulong)Math.Floor(latitude * 1000) << 32 | (ulong)Math.Floor(longitude * 1000);
        static ulong CreateKey2(double latitude, double longitude) => (ulong)Math.Floor(latitude * 1000000) << 32 | (ulong)Math.Floor(longitude * 1000000);

        public override void Add(LayerElement element)
        {
            if (lookupsByElements.ContainsKey(element.Id) == false) lookupsByElements.Add(element.Id, new List<ulong>());
            if (element.Points.Length >= 2)
                Add(ref element.Points[0], ref element.Points[1], element);
            if (element.Points.Length >= 4)
                Add(ref element.Points[element.Points.Length - 2], ref element.Points[element.Points.Length - 1], element);
        }

        private void Add(ref double latitude, ref double longitude, LayerElement element)
        {
            var key1 = CreateKey1(latitude, longitude);
            var key2 = CreateKey2(latitude, longitude);
            if (!lookups.ContainsKey(key1)) lookups.Add(key1, new Dictionary<ulong, List<Guid>>());
            if (!lookups[key1].ContainsKey(key2)) lookups[key1].Add(key2, new List<Guid>());
            if (lookups[key1][key2].Contains(element.Id)) return;

            lookups[key1][key2].Add(element.Id);
            if (lookupsByElements[element.Id].Contains(key1) == false) lookupsByElements[element.Id].Add(key1);
        }

        public override void Remove(LayerElement element)
        {
            if (lookupsByElements.ContainsKey(element.Id) == false) return;
            foreach (var key in lookupsByElements[element.Id])
            {
                if (lookups.ContainsKey(key) == false) continue;
                foreach (var lookup in lookups[key].Values)
                    if (lookup.Contains(element.Id))
                        lookup.Remove(element.Id);
            }
            lookupsByElements.Remove(element.Id);
        }

        public override void HitTest(ref double latitude, ref double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute)
        {
            var key1 = CreateKey1(latitude, longitude);
            if (lookups.ContainsKey(key1) == false) return;
            var key2 = CreateKey2(latitude, longitude);
            if (lookups[key1].ContainsKey(key2) == false) return;
            var lookup = lookups[key1][key2];
            foreach (var elementId in lookup)
            {
                var element = GetElement(elementId);
                if (element == null) continue;
                if (element.Routed && justNotRoute) continue;

                if (element.CoordinateFirst != null)
                {
                    if (element.CoordinateFirst.Latitude == latitude && element.CoordinateFirst.Longitude == longitude)
                        if (result.Contains(element) == false)
                        {
                            result.Add(element);
                            continue;
                        }
                }

                if (element.CoordinateLast != null)
                {
                    if (element.CoordinateLast.Latitude == latitude && element.CoordinateLast.Longitude == longitude)
                        if (result.Contains(element) == false)
                        {
                            result.Add(element);
                            continue;
                        }
                }
            }

        }
    }
    public partial class LayerElementsMatrixByPolygon : LayerElementsMatrix
    {
        public LayerElementsMatrixByPolygon(Func<Guid, LayerElement> getElementById_Func) : base(getElementById_Func) { }
        Dictionary<Guid, LayerElement> elements = new Dictionary<Guid, LayerElement>();

        public override void Add(LayerElement element)
        {
            if (elements.ContainsKey(element.Id)) return;
            else elements.Add(element.Id, element);
        }
        public override void Remove(LayerElement element)
        {
            if (elements.ContainsKey(element.Id) == false) return;
            else elements.Remove(element.Id);
        }
        public override void HitTest(ref double latitude, ref double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute)
        {

        }
    }

}
