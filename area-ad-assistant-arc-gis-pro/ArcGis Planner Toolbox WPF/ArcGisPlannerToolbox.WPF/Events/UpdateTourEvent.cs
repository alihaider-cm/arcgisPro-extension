using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class UpdateTourEvent : CompositePresentationEvent<bool>
{
    public static void Publish(bool reloadable)
    {
        FrameworkApplication.EventAggregator.GetEvent<UpdateTourEvent>().Broadcast(reloadable);
    }
    public static SubscriptionToken Subscribe(Action<bool> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<UpdateTourEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<bool> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<UpdateTourEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<UpdateTourEvent>().Unregister(token);
    }
}
