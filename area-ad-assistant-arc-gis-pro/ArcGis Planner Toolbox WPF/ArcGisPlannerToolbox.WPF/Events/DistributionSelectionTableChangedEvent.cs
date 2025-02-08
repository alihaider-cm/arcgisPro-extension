using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class DistributionSelectionTableChangedEvent : CompositePresentationEvent<int>
{
    public static void Publish(int provideNeigborZipCodes)
    {
        FrameworkApplication.EventAggregator.GetEvent<DistributionSelectionTableChangedEvent>().Broadcast(provideNeigborZipCodes);
    }
    public static SubscriptionToken Subscribe(Action<int> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<DistributionSelectionTableChangedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<int> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<DistributionSelectionTableChangedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<DistributionSelectionTableChangedEvent>().Unregister(token);
    }
}
