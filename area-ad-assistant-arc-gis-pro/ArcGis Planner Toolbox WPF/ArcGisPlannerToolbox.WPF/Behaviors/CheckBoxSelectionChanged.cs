using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class CheckBoxSelectionChanged : Behavior<CheckBox>
{
    private List<MethodDescriptor> methodDescriptors;
    private object Target => TargetObject ?? base.AssociatedObject;

    public object TargetObject
    {
        get { return (object)GetValue(TargetObjectProperty); }
        set { SetValue(TargetObjectProperty, value); }
    }

    public static readonly DependencyProperty TargetObjectProperty =
        DependencyProperty.Register(nameof(TargetObject), typeof(object), typeof(CheckBoxSelectionChanged), new PropertyMetadata(OnTargetObjectChanged));

    public string MethodName
    {
        get { return (string)GetValue(MethodNameProperty); }
        set { SetValue(MethodNameProperty, value); }
    }
    public static readonly DependencyProperty MethodNameProperty =
        DependencyProperty.Register(nameof(MethodName), typeof(string), typeof(CheckBoxSelectionChanged), new PropertyMetadata(OnMethodNameChanged));

    public CheckBoxSelectionChanged()
    {
        methodDescriptors = new List<MethodDescriptor>();
    }

    protected override void OnAttached()
    {
        AssociatedObject.Checked += AssociatedObject_Checked;
        AssociatedObject.Unchecked += AssociatedObject_Unchecked;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Checked -= AssociatedObject_Checked;
        AssociatedObject.Unchecked -= AssociatedObject_Unchecked;
    }

    private void AssociatedObject_Unchecked(object sender, RoutedEventArgs e)
    {
        Invoke(AssociatedObject.Tag);
    }
    private void AssociatedObject_Checked(object sender, RoutedEventArgs e)
    {
        Invoke(AssociatedObject.Tag);
    }
    private void Invoke(object parameter)
    {
        if (base.AssociatedObject == null)
            return;

        MethodDescriptor methodDescriptor = FindBestMethod(parameter);
        if (methodDescriptor != null)
        {
            ParameterInfo[] parameters = methodDescriptor.Parameters;
            if (parameters.Length == 0)
                methodDescriptor.MethodInfo.Invoke(Target, null);
            else if (parameters.Length == 1)
                methodDescriptor.MethodInfo.Invoke(Target, new object[1] { AssociatedObject.IsChecked });
            else if (parameters.Length == 2 && base.AssociatedObject != null && parameter != null && parameters[0].ParameterType.IsAssignableFrom(base.AssociatedObject.IsChecked.GetType()) && parameters[1].ParameterType.IsAssignableFrom(parameter.GetType()))
                methodDescriptor.MethodInfo.Invoke(Target, new object[2] { base.AssociatedObject.IsChecked.Value, parameter });
        }
        else if (TargetObject != null)
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "CallMethodActionValidMethodNotFoundExceptionMessage", MethodName, TargetObject.GetType().Name));
    }
    private static void OnMethodNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((CheckBoxSelectionChanged)sender).UpdateMethodInfo();
    }
    private static void OnTargetObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((CheckBoxSelectionChanged)sender).UpdateMethodInfo();
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

        if (method.ReturnType != typeof(Task))
        {
            return false;
        }

        return true;
    }
    private static bool AreMethodParamsValid(ParameterInfo[] methodParams)
    {
        if (methodParams.Length == 1)
        {
            if (methodParams[0].ParameterType != typeof(bool))
            {
                return false;
            }
        }
        else if (methodParams.Length == 2)
        {
            if (methodParams[0].ParameterType != typeof(bool))
            {
                return false;
            }

            if (!typeof(string).IsAssignableFrom(methodParams[1].ParameterType))
            {
                return false;
            }
        }
        else if (methodParams.Length != 0)
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

        return methodDescriptors.FirstOrDefault((x) => x.SecondParameterType.IsAssignableFrom(parameter.GetType()));
        //return methodDescriptors.FirstOrDefault((x) => x.Parameters.FirstOrDefault().ParameterType.IsAssignableFrom(parameter.GetType()));
    }
}
