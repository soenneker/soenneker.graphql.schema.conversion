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
    /// <param name="introspectionJson">Introspection JSON for the convert operation.</param>
    /// <param name="includeDescriptions">Whether descriptions.</param>
    /// <returns>The text produced by convert.</returns>
    string Convert(string introspectionJson, bool includeDescriptions = true);

    /// <summary>
    /// Converts a parsed GraphQL introspection payload into SDL.
    /// </summary>
    /// <param name="introspectionDocument">Introspection Document for the convert operation.</param>
    /// <param name="includeDescriptions">Whether descriptions.</param>
    /// <returns>The text produced by convert.</returns>
    string Convert(JsonDocument introspectionDocument, bool includeDescriptions = true);
}
