using System;
using System.Reflection;
using System.Collections.Generic;

public class ClassAnalyzer
{
    private Type _type;
    public ClassAnalyzer(Type type)
    {
        _type = type;
    }
     public IEnumerable<string> GetPublicMethods()
    {
        var methods = _type.GetMethods();
        var names = methods.Select(m => m.Name);
        return names;
    }
    public IEnumerable<string> GetMethodParams(string methodname)
    {
        var method = _type.GetMethod(methodname);
        var result = new List<string>();
        var parameters = method.GetParameters();
        var paramNames = parameters.Select(p => p.Name);
        result.AddRange(paramNames);
        result.Add(method.ReturnType.Name);
        return result;
    }
    public IEnumerable<string> GetAllFields()
    {
        var fields = _type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var names = fields.Select(f => f.Name);
        return names;
    }

    public IEnumerable<string> GetProperties()
    {
        var properties = _type.GetProperties();
        var names = properties.Select(p => p.Name);
        return names;
    }

    public bool HasAttribute<T>() where T : Attribute
    {
        var attributes = _type.GetCustomAttributes(typeof(T), true);
        if (attributes.Length > 0)
        {
            return true;
        }
        return false;
    }
}
