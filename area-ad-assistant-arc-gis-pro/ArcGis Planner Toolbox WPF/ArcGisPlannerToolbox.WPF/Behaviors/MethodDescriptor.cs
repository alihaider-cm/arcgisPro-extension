using System;
using System.Reflection;

namespace ArcGisPlannerToolbox.WPF.Behaviors;

public class MethodDescriptor
{
    public MethodInfo MethodInfo { get; private set; }
    public bool HasParameters => Parameters.Length != 0;
    public int ParameterCount => Parameters.Length;
    public ParameterInfo[] Parameters { get; private set; }
    public Type SecondParameterType
    {
        get
        {
            if (Parameters.Length >= 2)
            {
                return Parameters[1].ParameterType;
            }

            return null;
        }
    }

    public MethodDescriptor(MethodInfo methodInfo, ParameterInfo[] methodParams)
    {
        MethodInfo = methodInfo;
        Parameters = methodParams;
    }
}
