using System;
using System.Collections.Generic;

namespace GeographyModel
{
    public abstract partial class LayerElementsMatrix
    {
        private readonly Func<string, LayerElement> GetElement_Func;
        public LayerElementsMatrix(Func<string, LayerElement> getElement_Func)
        {
            GetElement_Func = getElement_Func;
        }

        protected LayerElement GetElement(string elementCode) => GetElement_Func?.Invoke(elementCode);
        public abstract void Add(LayerElement element);
        public abstract void Remove(LayerElement element);
        public abstract void HitTest(ref double latitude, ref double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute);
    }

    public partial class LayerElementsMatrixByPoint : LayerElementsMatrix
    {
        public LayerElementsMatrixByPoint(Func<string, LayerElement> getElement_Func) : base(getElement_Func) { }

        Dictionary<string, HashSet<ulong>> lookupsByElements = new Dictionary<string, HashSet<ulong>>();
        Dictionary<ulong, Dictionary<ulong, HashSet<string>>> lookups = new Dictionary<ulong, Dictionary<ulong, HashSet<string>>>();

        static ulong CreateKey1(double latitude, double longitude) => (ulong)Math.Floor(latitude * 1000) << 32 | (ulong)Math.Floor(longitude * 1000);
        static ulong CreateKey2(double latitude, double longitude) => (ulong)Math.Floor(latitude * 1000000) << 32 | (ulong)Math.Floor(longitude * 1000000);

        public override void Add(LayerElement element)
        {
            if (!lookupsByElements.TryGetValue(element.Code, out var elementLookup))
            {
                elementLookup = new HashSet<ulong>();
                lookupsByElements.Add(element.Code, elementLookup);
            }

            if (element.Points.Length >= 2)
                Add(ref element.Points[0], ref element.Points[1], element, elementLookup);
            if (element.Points.Length >= 4)
                Add(ref element.Points[element.Points.Length - 2], ref element.Points[element.Points.Length - 1], element, elementLookup);
        }

        private void Add(ref double latitude, ref double longitude, LayerElement element, HashSet<ulong> elementLookup)
        {
            var key1 = CreateKey1(latitude, longitude);
            var key2 = CreateKey2(latitude, longitude);
            if (!lookups.TryGetValue(key1, out var key1Lookup))
            {
                key1Lookup = new Dictionary<ulong, HashSet<string>>();
                lookups.Add(key1, key1Lookup);
            }

            if (!key1Lookup.TryGetValue(key2, out var key1Key2Lookup))
            {
                key1Key2Lookup = new HashSet<string>();
                key1Lookup.Add(key2, key1Key2Lookup);
            }

            if (!key1Key2Lookup.Add(element.Code)) return;
            elementLookup.Add(key1);
        }

        public override void Remove(LayerElement element)
        {
            if (!lookupsByElements.TryGetValue(element.Code, out var elementLookup)) return;

            foreach (var key in elementLookup)
            {
                if (!lookups.TryGetValue(key, out var key1Lookup)) continue;

                foreach (var lookup in key1Lookup.Values)
                    lookup.Remove(element.Code);
            }

            lookupsByElements.Remove(element.Code);
        }

        public override void HitTest(ref double latitude, ref double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute)
        {
            var key1 = CreateKey1(latitude, longitude);
            if (!lookups.TryGetValue(key1, out var key1Lookup)) return;

            var key2 = CreateKey2(latitude, longitude);
            if (!key1Lookup.TryGetValue(key2, out var key1Key2Lookup)) return;

            foreach (var elementId in key1Key2Lookup)
            {
                var element = GetElement(elementId);
                if (element == null) continue;
                if (element.Routed && justNotRoute) continue;

                if (element.CoordinateFirst != null &&
                    element.CoordinateFirst.Latitude == latitude &&
                    element.CoordinateFirst.Longitude == longitude &&
                    !result.Contains(element))
                {
                    result.Add(element);
                    continue;
                }

                if (element.CoordinateLast != null &&
                    element.CoordinateLast.Latitude == latitude &&
                    element.CoordinateLast.Longitude == longitude &&
                    !result.Contains(element))
                {
                    result.Add(element);
                }
            }

        }
    }

    public partial class LayerElementsMatrixByPolygon : LayerElementsMatrix
    {
        public LayerElementsMatrixByPolygon(Func<string, LayerElement> getElement_Func) : base(getElement_Func) { }
        Dictionary<string, LayerElement> elements = new Dictionary<string, LayerElement>();

        public override void Add(LayerElement element)
        {
            if (elements.ContainsKey(element.Code)) return;

            elements.Add(element.Code, element);
        }
        public override void Remove(LayerElement element)
        {
            if (!elements.ContainsKey(element.Code)) return;

            elements.Remove(element.Code);
        }
        public override void HitTest(ref double latitude, ref double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute)
        {

        }
    }

}
