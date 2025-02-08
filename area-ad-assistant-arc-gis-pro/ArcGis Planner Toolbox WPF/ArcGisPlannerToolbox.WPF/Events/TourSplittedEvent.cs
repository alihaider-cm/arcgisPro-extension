using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class TourSplittedEvent : CompositePresentationEvent<bool>
{
    public static void Publish(bool arg)
    {
        FrameworkApplication.EventAggregator.GetEvent<TourSplittedEvent>().Broadcast(arg);
    }
    public static SubscriptionToken Subscribe(Action<bool> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<TourSplittedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<bool> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<TourSplittedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<TourSplittedEvent>().Unregister(token);
    }
}
