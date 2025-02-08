using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class ListViewMultiSelection : Behavior<ListView>
{
    private List<MethodDescriptor> methodDescriptors;
    private object Target => TargetObject ?? base.AssociatedObject;
    private List<object> _listOfItems = new();

    public object TargetObject
    {
        get { return (object)GetValue(TargetObjectProperty); }
        set { SetValue(TargetObjectProperty, value); }
    }

    public static readonly DependencyProperty TargetObjectProperty =
        DependencyProperty.Register(nameof(TargetObject), typeof(object), typeof(ListViewMultiSelection), new PropertyMetadata(OnTargetObjectChanged));

    public string MethodName
    {
        get { return (string)GetValue(MethodNameProperty); }
        set { SetValue(MethodNameProperty, value); }
    }
    public static readonly DependencyProperty MethodNameProperty =
        DependencyProperty.Register(nameof(MethodName), typeof(string), typeof(ListViewMultiSelection), new PropertyMetadata(OnMethodNameChanged));

    public ListViewMultiSelection()
    {
        methodDescriptors = new List<MethodDescriptor>();
    }
    protected override void OnAttached()
    {
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _listOfItems.Clear();
        foreach (var item in AssociatedObject.SelectedItems)
            _listOfItems.Add(item);

        Invoke(_listOfItems);
    }

    private void Invoke(object parameter)
    {
        if (base.AssociatedObject == null)
            return;

        MethodDescriptor methodDescriptor = methodDescriptors.FirstOrDefault();
        if (methodDescriptor != null)
        {
            ParameterInfo[] parameters = methodDescriptor.Parameters;
            if (parameters.Length == 1)
            {
                methodDescriptor.MethodInfo.Invoke(Target, new object[1] {_listOfItems });
            }
        }
        else if (TargetObject != null)
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "CallMethodActionValidMethodNotFoundExceptionMessage", MethodName, TargetObject.GetType().Name));
    }
    private static void OnMethodNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((ListViewMultiSelection)sender).UpdateMethodInfo();
    }
    private static void OnTargetObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((ListViewMultiSelection)sender).UpdateMethodInfo();
    }
    private void UpdateMethodInfo()
    {
        methodDescriptors.Clear();
        if (Target == null || string.IsNullOrEmpty(MethodName))
        {
            return;
        }

        MethodInfo[] methods = Target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
        foreach (MethodInfo methodInfo in methods)
        {
            if (IsMethodValid(methodInfo))
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (AreMethodParamsValid(parameters))
                {
                    methodDescriptors.Add(new MethodDescriptor(methodInfo, parameters));
                }
            }
        }
    }
    private bool IsMethodValid(MethodInfo method)
    {
        if (!string.Equals(method.Name, MethodName, StringComparison.Ordinal))
        {
            return false;
        }

        if (method.ReturnType != typeof(void))
        {
            return false;
        }

        return true;
    }
    private static bool AreMethodParamsValid(ParameterInfo[] methodParams)
    {
        if (methodParams.Length == 1)
        {
            if (methodParams[0].ParameterType != typeof(List<object>))
                return false;
        }
        else if (methodParams.Length == 2)
        {
            if (methodParams[0].ParameterType != typeof(object))
            {
                return false;
            }

            if (!typeof(List<object>).IsAssignableFrom(methodParams[1].ParameterType))
            {
                return false;
            }
        }
        else if (methodParams.Length == 0)
        {
            return false;
        }

        return true;
    }
    private MethodDescriptor FindBestMethod(object parameter)
    {
        if (parameter != null)
        {
            parameter.GetType();
        }

        return methodDescriptors.FirstOrDefault((x) => x.Parameters.FirstOrDefault().ParameterType.IsAssignableFrom(parameter.GetType()));
    }
}
