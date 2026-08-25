using System;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        Assembly asm =
            Assembly.LoadFrom("OpenAgent.Interop.dll");

        Type? type =
            asm.GetType("OpenAgent.Interop.IDaSvrCharacter")
            ??
            asm.GetTypes()
               .FirstOrDefault(
                    t => t.Name == "IDaSvrCharacter");

        if (type == null)
        {
            Console.WriteLine(
                "IDaSvrCharacter not found.");

            return;
        }

        Console.WriteLine("================================");
        Console.WriteLine(type.FullName);
        Console.WriteLine("================================");
        Console.WriteLine();

        Console.WriteLine("========== METHODS ==========");
        Console.WriteLine();

        foreach (MethodInfo method in
                 type.GetMethods())
        {
            Console.WriteLine("--------------------------------");

            Console.WriteLine(
                $"Name : {method.Name}");

            Console.WriteLine(
                $"Return : {method.ReturnType.Name}");

            Console.WriteLine(
                $"Signature : {method}");

            ParameterInfo[] parameters =
                method.GetParameters();

            if (parameters.Length == 0)
            {
                Console.WriteLine(
                    "Parameters : None");
            }
            else
            {
                Console.WriteLine(
                    "Parameters :");

                foreach (ParameterInfo p in parameters)
                {
                    Console.WriteLine(
                        $"   {p.ParameterType.Name} {p.Name}");
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("========== PROPERTIES ==========");
        Console.WriteLine();

        foreach (PropertyInfo property in
                 type.GetProperties())
        {
            Console.WriteLine(
                $"{property.PropertyType.Name} {property.Name}");
        }

        Console.WriteLine();
        Console.WriteLine("========== EVENTS ==========");
        Console.WriteLine();

        foreach (EventInfo ev in
                 type.GetEvents())
        {
            Console.WriteLine(
                $"{ev.EventHandlerType?.Name} {ev.Name}");
        }

        Console.WriteLine();
        Console.WriteLine("Finished.");
        Console.ReadLine();
    }
}