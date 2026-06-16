using System.Text.Json;
using System.Text.Json.Schema;

namespace AIGuard.NET.Schema;

/// <summary>
/// Extracts JSON Schema from C# types using .NET 9's built-in schema generation.
/// </summary>
public static class SchemaExtractor
{
    /// <summary>
    /// Generates a JSON Schema representation for type T.
    /// </summary>
    public static string GenerateJsonSchema<T>(JsonSerializerOptions? options = null)
    {
        options ??= JsonSerializerOptions.Default;
        var node = options.GetJsonSchemaAsNode(typeof(T));
        return node.ToString();
    }
}
