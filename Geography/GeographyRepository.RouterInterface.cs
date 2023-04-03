using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class GeographyRepository : GeographyRouter.IGeoRepository
{
    public void ResetRouting() => WriteByLock(() =>
    {
        Parallel.ForEach(_elements.Values, element =>
        {
            element.ResetRouting();
        });
    });

    public List<GeographyRouter.ILayerElement> GetRoutingSources() => ReadByLock(() =>
    {
        var layer = _layers.Values.FirstOrDefault(x => x.IsRoutingSource);

        if (layer == null)
            return new List<GeographyRouter.ILayerElement>();

        return getLayerElements(layer.Code).ToList<GeographyRouter.ILayerElement>();

    });

    public void RoutingHitTest(double latitude, double longitude, ref List<GeographyRouter.ILayerElement> result, bool justNotRoute) /*=> Lock.PerformRead(() =>*/
    {
        //var result = new List<LayerElement>();
        ElecricalMatrix.HitTest(ref latitude, ref longitude, ref result, justNotRoute);

        //return result;
    }//);

    public List<string> GetNotRoutedCodes() => ReadByLock(() =>
    {
        var result = new List<string>();
        foreach (var element in _elements.Values)
        {
            if (!element.Layer.IsElectrical) continue;
            if (element.Routed) continue;

            result.Add(element.Code);
        }
        return result;
    });
}
