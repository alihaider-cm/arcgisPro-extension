using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class BranchCreatedEvent : CompositePresentationEvent<CustomerBranch>
{
    public static void Publish(CustomerBranch args)
    {
        FrameworkApplication.EventAggregator.GetEvent<BranchCreatedEvent>().Broadcast(args);
    }
    public static SubscriptionToken Subscribe(Action<CustomerBranch> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<BranchCreatedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<CustomerBranch> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<BranchCreatedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<BranchCreatedEvent>().Unregister(token);
    }
}
