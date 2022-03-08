using System.Collections.Generic;

namespace GeographyRouter
{
    public interface IGeoRepository
    {
        void ResetRouting();
        List<ILayerElement> GetRoutingSources();
        void RoutingHitTest(double latitude, double longitude, ref List<ILayerElement> result, bool justNotRoute);
    }
}
