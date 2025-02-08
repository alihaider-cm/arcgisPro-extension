using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public sealed class MediaSelectionChangedEvent : CompositePresentationEvent<Media>
{
    public static void Publish(Media media)
    {
        FrameworkApplication.EventAggregator.GetEvent<MediaSelectionChangedEvent>().Broadcast(media);
    }
    public static SubscriptionToken Subscribe(Action<Media> action, bool keepSubscriberAlive = false)
    {
        return FrameworkApplication.EventAggregator.GetEvent<MediaSelectionChangedEvent>().Register(action, keepSubscriberAlive);
    }
    public static void Unsubscribe(Action<Media> action)
    {
        FrameworkApplication.EventAggregator.GetEvent<MediaSelectionChangedEvent>().Unregister(action);
    }
    public static void Unsubscribe(SubscriptionToken token)
    {
        FrameworkApplication.EventAggregator.GetEvent<MediaSelectionChangedEvent>().Unregister(token);
    }
}
