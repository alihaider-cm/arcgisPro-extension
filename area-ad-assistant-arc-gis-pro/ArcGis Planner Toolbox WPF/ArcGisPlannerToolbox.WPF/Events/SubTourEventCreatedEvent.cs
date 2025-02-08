using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class SubTourEventCreatedEvent : CompositePresentationEvent<Tour>
{
    public static void Publish(Tour args)
    {
        FrameworkApplication.EventAggregator.GetEvent<SubTourEventCreatedEvent>().Broadcast(args);
    }
    public static SubscriptionToken Subscribe(Action<Tour> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<SubTourEventCreatedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<Tour> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<SubTourEventCreatedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<SubTourEventCreatedEvent>().Unregister(token);
    }
}
