using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class PlanningNumberChangedEvent : CompositePresentationEvent<int>
{
    public static void Publish(int args)
    {
        FrameworkApplication.EventAggregator.GetEvent<PlanningNumberChangedEvent>().Broadcast(args);
    }
    public static SubscriptionToken Subscribe(Action<int> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<PlanningNumberChangedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<int> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<PlanningNumberChangedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<PlanningNumberChangedEvent>().Unregister(token);
    }
}
