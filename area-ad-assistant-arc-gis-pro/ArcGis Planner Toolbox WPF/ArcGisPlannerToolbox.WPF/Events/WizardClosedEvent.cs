using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class WizardClosedEvent : CompositePresentationEvent<bool>
{
    public static void Publish(bool args)
    {
        FrameworkApplication.EventAggregator.GetEvent<WizardClosedEvent>().Broadcast(args);
    }
    public static SubscriptionToken Subscribe(Action<bool> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<WizardClosedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<bool> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<WizardClosedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<WizardClosedEvent>().Unregister(token);
    }
}
