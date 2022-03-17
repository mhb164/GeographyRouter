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
        Parallel.ForEach(elements.Values, element =>
        {
            element.ResetRouting();
        });
        //foreach (var element in elements.Values)
        //{
        //    element.ResetRouting();
        //}
    });

    public List<GeographyRouter.ILayerElement> GetRoutingSources() => ReadByLock(() =>
    {
        var layer = layers.Values.FirstOrDefault(x => x.IsRoutingSource);

        if (layer == null || !elementsByLayerId.ContainsKey(layer.Id))
        {
            return new List<GeographyRouter.ILayerElement>();
        }
        else
        {
            return elementsByLayerId[layer.Id].ToList<GeographyRouter.ILayerElement>();
        }
        //if (layers.ContainsKey("MVPT_HEADER") == false) return new List<GeographyRouter.ILayerElement>();
        //var layer = layers["MVPT_HEADER"];
        //return elementsByLayerId[layer.Id].ToList<GeographyRouter.ILayerElement>();
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
        foreach (var element in elements.Values)
        {
            if (element.Layer.IsElectrical == false) continue;
            if (element.Routed) continue;
            else result.Add(element.Code);
        }
        return result;
    });
}
