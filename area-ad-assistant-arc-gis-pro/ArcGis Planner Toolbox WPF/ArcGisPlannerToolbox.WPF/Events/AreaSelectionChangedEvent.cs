using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models.DTO;
using System;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Events;

public class AreaSelectionChangedEvent : CompositePresentationEvent<List<OccupancyUnitDTO>>
{
    public static void Publish(List<OccupancyUnitDTO> occupancyUnits)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreaSelectionChangedEvent>().Broadcast(occupancyUnits);
    }
    public static SubscriptionToken Subscribe(Action<List<OccupancyUnitDTO>> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<AreaSelectionChangedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<List<OccupancyUnitDTO>> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreaSelectionChangedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreaSelectionChangedEvent>().Unregister(token);
    }
}
