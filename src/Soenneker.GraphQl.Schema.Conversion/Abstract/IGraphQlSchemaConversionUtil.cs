using System.Text.Json;

namespace Soenneker.GraphQl.Schema.Conversion.Abstract;

/// <summary>
/// A GraphQL schema conversion utility
/// </summary>
public interface IGraphQlSchemaConversionUtil
{
    /// <summary>
    /// Converts GraphQL introspection JSON into SDL.
    /// </summary>
    string Convert(string introspectionJson, bool includeDescriptions = true);

    /// <summary>
    /// Converts a parsed GraphQL introspection payload into SDL.
    /// </summary>
    string Convert(JsonDocument introspectionDocument, bool includeDescriptions = true);
}
