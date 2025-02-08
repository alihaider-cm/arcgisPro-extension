using ArcGIS.Core.Events;
using ArcGisPlannerToolbox.Core.Models.DTO;
using System.Collections.Generic;
using System;
using ArcGIS.Desktop.Framework;

namespace ArcGisPlannerToolbox.WPF.Events;

public class AreasPublishEvent : CompositePresentationEvent<List<OccupancyUnitDTO>>
{
    public static void Publish(List<OccupancyUnitDTO> occupancyUnits)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreasPublishEvent>().Broadcast(occupancyUnits);
    }
    public static SubscriptionToken Subscribe(Action<List<OccupancyUnitDTO>> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<AreasPublishEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<List<OccupancyUnitDTO>> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreasPublishEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreasPublishEvent>().Unregister(token);
    }
}
