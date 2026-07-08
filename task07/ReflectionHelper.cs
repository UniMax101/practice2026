using System;
using System.Reflection;

public static class ReflectionHelper
{
    public static void PrintTypeInfo(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        var displayName = type.GetCustomAttribute<DisplayNameAttribute>();
        if (displayName != null)
        {
            Console.WriteLine($"Отображаемое имя: {displayName.DisplayName}");
        }

        var version = type.GetCustomAttribute<VersionAttribute>();
        if (version != null)
        {
            Console.WriteLine($"Версия: {version.Major}.{version.Minor}");
        }

        Console.WriteLine("Методы:");
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            var methodDisplayName = method.GetCustomAttribute<DisplayNameAttribute>();
            if (methodDisplayName != null)
            {
                Console.WriteLine($"  {method.Name}: {methodDisplayName.DisplayName}");
            }
        }

        Console.WriteLine("Свойства:");
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var property in properties)
        {
            var propertyDisplayName = property.GetCustomAttribute<DisplayNameAttribute>();
            if (propertyDisplayName != null)
            {
                Console.WriteLine($"  {property.Name}: {propertyDisplayName.DisplayName}");
            }
        }
    }
}
