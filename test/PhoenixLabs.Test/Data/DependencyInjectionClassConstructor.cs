using System;
using TUnit.Core.Interfaces;

namespace PhoenixLabs.Test;

public class DependencyInjectionClassConstructor : IClassConstructor
{
    public T Create<T>(ClassConstructorMetadata classConstructorMetadata) where T : class
    {
        Console.WriteLine("You can also control how your test classes are new'd up, giving you lots of power and the ability to utilise tools such as dependency injection");

        // Example code commented out to avoid compilation errors
        // Uncomment and implement these classes if you want to use dependency injection
        /*
        if (typeof(T) == typeof(AndEvenMoreTests))
        {
            return (new AndEvenMoreTests(new DataClass()) as T)!;
        }
        */

        // Default implementation - create instance using default constructor
        return Activator.CreateInstance<T>();
    }
}
