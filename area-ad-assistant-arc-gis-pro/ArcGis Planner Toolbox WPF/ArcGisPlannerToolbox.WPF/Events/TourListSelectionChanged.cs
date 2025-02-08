using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Events;

public class TourListSelectionChanged : CompositePresentationEvent<Tuple<Tour, List<Tour>>>
{
    public static void Publish(Tuple<Tour, List<Tour>> args)
    {
        FrameworkApplication.EventAggregator.GetEvent<TourListSelectionChanged>().Broadcast(args);
    }
    public static SubscriptionToken Subscribe(Action<Tuple<Tour, List<Tour>>> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<TourListSelectionChanged>().Register(action, keepSubscriberAlive); 
    }
    public static void Unsubscribe(Action<Tuple<Tour, List<Tour>>> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<TourListSelectionChanged>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<TourListSelectionChanged>().Unregister(token);
    }
}
