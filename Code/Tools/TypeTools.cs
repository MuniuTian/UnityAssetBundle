using System;
using System.Reflection;
using System.Collections.Generic;

public static class TypeTools
{
    private class AssemblyComparer : IComparer<Assembly>
    {
        public int Compare(Assembly lhs, Assembly rhs)
        {
            // version = "0.0.0.0" means this is an our own assembly.
            var leftVersion = lhs.FullName.Split(_splitter)[1];
            var rightVersion = rhs.FullName.Split(_splitter)[1];

            var result = leftVersion.CompareTo(rightVersion);
            if (result == 0)
            {
                result = lhs.FullName.CompareTo(rhs.FullName);
            }

            return result;
        }

        private readonly char[] _splitter = new char[] { '=' };
    }

    /*
     * Through the assembly to take, there must be a namespace.classname parameter.
     * */
    public static Type SerchType(string typeFullName)
    {
        if (null != typeFullName)
        {
            if (null == _currentAssemblies)
            {
                _currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                Array.Sort(_currentAssemblies, new AssemblyComparer());
            }

            var count = _currentAssemblies.Length;
            for (int i = 0; i < count; ++i)
            {
                var assembly = _currentAssemblies[i];
                var type = assembly.GetType(typeFullName);

                if (null != type)
                {
                    return type;
                }
            }
        }

        return null;
    }

    public static void CreateDelegate<T>(MethodInfo methodInfo, out T lpfnMethod) where T : class
    {
        lpfnMethod = Delegate.CreateDelegate(typeof(T), methodInfo) as T;
    }

    public static void CreateDelegate<T>(object obj, string func, out T lpfnMethod) where T : class
    {
        lpfnMethod = Delegate.CreateDelegate(typeof(T), obj, func) as T;
    }

    public static void CreateDelegate<T>(Type classType, string func, out T lpfnMethod) where T : class
    {
        lpfnMethod = Delegate.CreateDelegate(typeof(T), classType, func) as T;
    }

    public static T getAttributes<T>(Type t) where T : Attribute
    {
        var attrs = t.GetCustomAttributes(true);
        for (int i = 0; i < attrs.Length; ++i)
        {
            var attr = attrs[i];
            if (attr.GetType() == typeof(T))
            {
                return attr as T;
            }
        }
        return default;
    }

    public static T getAttributes<T>(this FieldInfo fieldInfo) where T : class
    {
        var attrs = fieldInfo.GetCustomAttributes(true);
        for (int i = 0; i < attrs.Length; ++i)
        {
            var attr = attrs[i];
            if (attr.GetType() == typeof(T))
            {
                return attr as T;
            }
        }
        return null;
    }

    public static T getAttributes<T>(this MemberInfo memberInfo) where T : class
    {
        var attrs = memberInfo.GetCustomAttributes(true);
        for (int i = 0; i < attrs.Length; ++i)
        {
            var attr = attrs[i];
            if (attr.GetType() == typeof(T))
            {
                return attr as T;
            }
        }
        return null;
    }

    public static Assembly GetEditorAssembly()
    {
        if (null != _editorAssembly)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var fullName = assembly.FullName;
                if (fullName.StartsWith("Assembly-CSharp-Editor"))
                {
                    _editorAssembly = assembly;
                    break;
                }
            }
        }

        return _editorAssembly;
    }

    private static Assembly _editorAssembly;
    private static Assembly[] _currentAssemblies;
}