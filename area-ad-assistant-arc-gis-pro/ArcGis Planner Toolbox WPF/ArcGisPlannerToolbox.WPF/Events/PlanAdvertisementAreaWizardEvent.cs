using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Data;
using System;
using System.Collections.Generic;

namespace ArcGisPlannerToolbox.WPF.Events;

public class PlanAdvertisementAreaWizardEvent
{
    public class CustomerBranchChanged : CompositePresentationEvent<CustomerBranch>
    {
        public static void Publish(CustomerBranch arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<CustomerBranchChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<CustomerBranch> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<CustomerBranchChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<CustomerBranch> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<CustomerBranchChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<CustomerBranchChanged>().Unregister(token);
        }
    }
    public class BaseGeometryChanged : CompositePresentationEvent<string>
    {
        public static void Publish(string arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<BaseGeometryChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<string> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<BaseGeometryChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<string> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<BaseGeometryChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<BaseGeometryChanged>().Unregister(token);
        }
    }
    public class PlanningLevelChanged : CompositePresentationEvent<string>
    {
        public static void Publish(string arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLevelChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<string> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<PlanningLevelChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<string> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLevelChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLevelChanged>().Unregister(token);
        }
    }
    public class PlanningLayerSelectionChanged : CompositePresentationEvent<List<PolygonGeometry>>
    {
        public static void Publish(List<PolygonGeometry> arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectionChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<List<PolygonGeometry>> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectionChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<List<PolygonGeometry>> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectionChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectionChanged>().Unregister(token);
        }
    }
    public class PlanningLayerSelectedItemChanged : CompositePresentationEvent<PolygonGeometry>
    {
        public static void Publish(PolygonGeometry arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectedItemChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<PolygonGeometry> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectedItemChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<PolygonGeometry> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectedItemChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<PlanningLayerSelectedItemChanged>().Unregister(token);
        }
    }
    public class SelectedCustomerChanged : CompositePresentationEvent<Customer>
    {
        public static void Publish(Customer arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<SelectedCustomerChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<Customer> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<SelectedCustomerChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<Customer> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<SelectedCustomerChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<SelectedCustomerChanged>().Unregister(token);
        }
    }
    public class SelectedOccupancyUnitsChanged : CompositePresentationEvent<List<PolygonGeometry>>
    {
        public static void Publish(List<PolygonGeometry> arg)
        {
            FrameworkApplication.EventAggregator.GetEvent<SelectedOccupancyUnitsChanged>().Broadcast(arg);
        }
        public static SubscriptionToken Subscribe(Action<List<PolygonGeometry>> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<SelectedOccupancyUnitsChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<List<PolygonGeometry>> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<SelectedOccupancyUnitsChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<SelectedOccupancyUnitsChanged>().Unregister(token);
        }
    }

}
