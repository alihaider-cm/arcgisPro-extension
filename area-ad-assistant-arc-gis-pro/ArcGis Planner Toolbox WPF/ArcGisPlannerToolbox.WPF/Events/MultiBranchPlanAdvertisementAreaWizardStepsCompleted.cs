using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Models;
using System;

namespace ArcGisPlannerToolbox.WPF.Events;

public class MultiBranchPlanAdvertisementAreaWizardStepsCompleted
{
    public class CustomerChanged : CompositePresentationEvent<Customer>
    {
        public static void Publish(Customer selectedCustomer)
        {
            FrameworkApplication.EventAggregator.GetEvent<CustomerChanged>().Broadcast(selectedCustomer);
        }
        public static SubscriptionToken Subscribe(Action<Customer> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<CustomerChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<Customer> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<CustomerChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<CustomerChanged>().Unregister(token);
        }
    }
    public class FormerPlanningChanged : CompositePresentationEvent<string>
    {
        public static void Publish(string formerPlanning)
        {
            FrameworkApplication.EventAggregator.GetEvent<FormerPlanningChanged>().Broadcast(formerPlanning);
        }
        public static SubscriptionToken Subscribe(Action<string> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<FormerPlanningChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<string> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<FormerPlanningChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<FormerPlanningChanged>().Unregister(token);
        }
    }
    public class AnalysisIdChanged : CompositePresentationEvent<int>
    {
        public static void Publish(int analysisId)
        {
            FrameworkApplication.EventAggregator.GetEvent<AnalysisIdChanged>().Broadcast(analysisId);
        }
        public static SubscriptionToken Subscribe(Action<int> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<AnalysisIdChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<int> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<AnalysisIdChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<AnalysisIdChanged>().Unregister(token);
        }
    }
    public class SecuredPlanningSelectionChanged : CompositePresentationEvent<PlanningData>
    {
        public static void Publish(PlanningData planningData)
        {
            FrameworkApplication.EventAggregator.GetEvent<SecuredPlanningSelectionChanged>().Broadcast(planningData);
        }
        public static SubscriptionToken Subscribe(Action<PlanningData> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<SecuredPlanningSelectionChanged>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<PlanningData> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<SecuredPlanningSelectionChanged>().Unregister(action);
        }
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<SecuredPlanningSelectionChanged>().Unregister(token);
        }
    }
}
