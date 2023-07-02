using System.Collections.Generic;

namespace GeographyRouter
{
    public interface IGeoRepository
    {
        void EnableLockForRouting();
        void DisableLockForRouting();

        void ResetRouting();
        List<ILayerElement> GetRoutingSources();
        void RoutingHitTest(double latitude, double longitude, ref List<ILayerElement> result, bool justNotRoute);
    }
}
