using System.Text.Json;

namespace Soenneker.GraphQl.Schema.Conversion.Abstract;

/// <summary>
/// Converts GraphQL introspection responses to Schema Definition Language (SDL).
/// </summary>
public interface IGraphQlSchemaConversionUtil
{
    /// <summary>
    /// Converts a GraphQL introspection response into SDL.
    /// </summary>
    /// <param name="introspectionJson">A JSON document containing <c>data.__schema</c>, <c>__schema</c>, or a schema object.</param>
    /// <param name="includeDescriptions">Whether to emit GraphQL descriptions in the SDL.</param>
    /// <returns>The converted schema, terminated with a newline; or an empty string when the schema has no emitted definitions.</returns>
    string Convert(string introspectionJson, bool includeDescriptions = true);

    /// <summary>
    /// Converts a parsed GraphQL introspection response into SDL.
    /// </summary>
    /// <param name="introspectionDocument">A JSON document containing <c>data.__schema</c>, <c>__schema</c>, or a schema object.</param>
    /// <param name="includeDescriptions">Whether to emit GraphQL descriptions in the SDL.</param>
    /// <returns>The converted schema, terminated with a newline; or an empty string when the schema has no emitted definitions.</returns>
    string Convert(JsonDocument introspectionDocument, bool includeDescriptions = true);
}
