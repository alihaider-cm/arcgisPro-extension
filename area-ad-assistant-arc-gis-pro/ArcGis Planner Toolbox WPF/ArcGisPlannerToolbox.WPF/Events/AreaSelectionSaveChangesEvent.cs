using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Events;

public class AreaSelectionSaveChangesEvent : CompositePresentationEvent<List<OccupancyUnit>>
{
    public static void Publish(List<OccupancyUnit> occupancyUnits)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreaSelectionSaveChangesEvent>().Broadcast(occupancyUnits);
    }
    public static SubscriptionToken Subscribe(Action<List<OccupancyUnit>> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<AreaSelectionSaveChangesEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<List<OccupancyUnit>> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreaSelectionSaveChangesEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<AreaSelectionSaveChangesEvent>().Unregister(token);
    }
}
