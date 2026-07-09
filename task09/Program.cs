using System;
using System.Reflection;

if (args.Length == 0)
{
    Console.WriteLine("Укажите путь до библиотеки.");
    return;
}

var path = args[0];
var assembly = Assembly.LoadFrom(path);

foreach (var type in assembly.GetTypes())
{
    Console.WriteLine($"\nКласс: {type.Name}");

    var attributes = type.GetCustomAttributes(true);
    if (attributes.Length > 0)
    {
        Console.WriteLine("Атрибуты:");
        foreach (var attr in attributes){Console.WriteLine($"[{attr.GetType().Name}]");}
    }

    Console.WriteLine("Конструкторы:");
    foreach (var ctor in type.GetConstructors())
    {
        var parameters = string.Join(", ", 
            ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"    .ctor({parameters})");
    }
    Console.WriteLine("Методы:");
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        var parameters = string.Join(", ",method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"{method.ReturnType.Name} {method.Name}({parameters})");
    }
}
