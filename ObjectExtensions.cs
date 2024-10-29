using System;
using System.Text.Json;

namespace Frigate_Helper
{
    /// <summary>
    /// Provides extension methods for objects.
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Serializes the object to JSON and prints it to the console.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="object">The object to serialize and print.</param>
        /// <returns>The original object.</returns>
        public static TObject DumpToConsole<TObject>(this TObject @object)
        {
            var output = "NULL";
            if (@object != null)
            {
                // Serialize the object to JSON with indentation.
                output = JsonSerializer.Serialize(@object, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            
            // Print the type and serialized output to the console.
            Console.WriteLine($"[{@object?.GetType().Name}]:\r\n{output}");
            return @object;
        }
    }
}
