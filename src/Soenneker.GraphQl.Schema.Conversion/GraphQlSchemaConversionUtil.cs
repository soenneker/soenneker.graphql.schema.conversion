using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Soenneker.GraphQl.Schema.Conversion.Abstract;
using Soenneker.Utils.PooledStringBuilders;
using Soenneker.Extensions.String;

namespace Soenneker.GraphQl.Schema.Conversion;

/// <inheritdoc cref="IGraphQlSchemaConversionUtil"/>
public sealed class GraphQlSchemaConversionUtil: IGraphQlSchemaConversionUtil
{
    public GraphQlSchemaConversionUtil()
    {
    }

    public string Convert(string introspectionJson, bool includeDescriptions = true)
    {
        if (introspectionJson.IsNullOrWhiteSpace())
            throw new ArgumentException("Introspection JSON is required.", nameof(introspectionJson));

        using JsonDocument document = JsonDocument.Parse(introspectionJson);
        return Convert(document, includeDescriptions);
    }

    public string Convert(JsonDocument introspectionDocument, bool includeDescriptions = true)
    {
        ArgumentNullException.ThrowIfNull(introspectionDocument);

        JsonElement schema = GetSchema(introspectionDocument.RootElement);
        var sections = new List<string>();

        string? schemaDefinition = BuildSchemaDefinition(schema);

        if (!string.IsNullOrWhiteSpace(schemaDefinition))
            sections.Add(schemaDefinition);

        if (TryGetArray(schema, "directives", out JsonElement directives))
        {
            foreach (JsonElement directive in directives.EnumerateArray())
            {
                if (ShouldSkipDirective(directive))
                    continue;

                sections.Add(BuildDirective(directive, includeDescriptions));
            }
        }

        if (!TryGetArray(schema, "types", out JsonElement types))
            throw new InvalidOperationException("The introspection schema did not contain a 'types' array.");

        foreach (JsonElement type in types.EnumerateArray())
        {
            if (ShouldSkipType(type))
                continue;

            string kind = GetRequiredString(type, "kind");

            string definition = kind switch
            {
                "SCALAR" => BuildScalar(type, includeDescriptions),
                "OBJECT" => BuildObject(type, includeDescriptions),
                "INTERFACE" => BuildInterface(type, includeDescriptions),
                "UNION" => BuildUnion(type, includeDescriptions),
                "ENUM" => BuildEnum(type, includeDescriptions),
                "INPUT_OBJECT" => BuildInputObject(type, includeDescriptions),
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(definition))
                sections.Add(definition);
        }

        return sections.Count == 0 ? string.Empty : string.Join("\n\n", sections) + "\n";
    }

    private static JsonElement GetSchema(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("The introspection payload root must be a JSON object.");

        if (root.TryGetProperty("data", out JsonElement data) && data.ValueKind == JsonValueKind.Object &&
            data.TryGetProperty("__schema", out JsonElement schemaFromData))
        {
            return schemaFromData;
        }

        if (root.TryGetProperty("__schema", out JsonElement schema))
            return schema;

        if (root.TryGetProperty("types", out _))
            return root;

        throw new InvalidOperationException("Unable to locate the GraphQL introspection schema payload.");
    }

    private static string? BuildSchemaDefinition(JsonElement schema)
    {
        string? queryType = GetNestedName(schema, "queryType");
        string? mutationType = GetNestedName(schema, "mutationType");
        string? subscriptionType = GetNestedName(schema, "subscriptionType");

        if (queryType is null && mutationType is null && subscriptionType is null)
            return null;

        bool useShorthand =
            queryType is "Query" &&
            (mutationType is null || mutationType == "Mutation") &&
            subscriptionType is null;

        if (useShorthand)
            return null;

        using var sb = new PooledStringBuilder();

        sb.AppendLine("schema {");

        if (queryType is not null)
        {
            sb.Append("  query: ");
            sb.AppendLine(queryType);
        }

        if (mutationType is not null)
        {
            sb.Append("  mutation: ");
            sb.AppendLine(mutationType);
        }

        if (subscriptionType is not null)
        {
            sb.Append("  subscription: ");
            sb.AppendLine(subscriptionType);
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string BuildDirective(JsonElement directive, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(directive, "description"), includeDescriptions);

            string name = GetRequiredString(directive, "name");
            sb.Append("directive @");
            sb.Append(name);

            if (TryGetArray(directive, "args", out JsonElement args) && args.GetArrayLength() > 0)
            {
                sb.Append('(');
                sb.Append(JoinInputValues(args.EnumerateArray(), includeDescriptionsInline: false));
                sb.Append(')');
            }

            if (GetOptionalBoolean(directive, "isRepeatable") == true)
                sb.Append(" repeatable");

            if (TryGetArray(directive, "locations", out JsonElement locations) && locations.GetArrayLength() > 0)
            {
                sb.Append(" on ");
                sb.Append(string.Join(" | ", locations.EnumerateArray().Select(static location => location.GetString()).Where(static value => !string.IsNullOrWhiteSpace(value))!));
            }

            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildScalar(JsonElement type, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(type, "description"), includeDescriptions);

            string name = GetRequiredString(type, "name");
            sb.Append("scalar ");
            sb.Append(name);

            string? specifiedByUrl = GetOptionalString(type, "specifiedByURL");

            if (!string.IsNullOrWhiteSpace(specifiedByUrl))
            {
                sb.Append(" @specifiedBy(url: ");
                sb.Append(ToGraphQlString(specifiedByUrl));
                sb.Append(')');
            }

            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildObject(JsonElement type, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(type, "description"), includeDescriptions);

            string name = GetRequiredString(type, "name");
            sb.Append("type ");
            sb.Append(name);

            string implementsClause = BuildImplementsClause(type);

            if (!string.IsNullOrWhiteSpace(implementsClause))
            {
                sb.Append(' ');
                sb.Append(implementsClause);
            }

            sb.AppendLine(" {");

            if (TryGetArray(type, "fields", out JsonElement fields))
            {
                foreach (JsonElement field in fields.EnumerateArray())
                {
                    AppendDescription(ref sb, GetOptionalString(field, "description"), includeDescriptions, 1);
                    sb.Append("  ");
                    sb.Append(BuildField(field));
                    sb.AppendLine();
                }
            }

            sb.Append('}');
            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildInterface(JsonElement type, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(type, "description"), includeDescriptions);

            string name = GetRequiredString(type, "name");
            sb.Append("interface ");
            sb.Append(name);

            string implementsClause = BuildImplementsClause(type);

            if (!string.IsNullOrWhiteSpace(implementsClause))
            {
                sb.Append(' ');
                sb.Append(implementsClause);
            }

            sb.AppendLine(" {");

            if (TryGetArray(type, "fields", out JsonElement fields))
            {
                foreach (JsonElement field in fields.EnumerateArray())
                {
                    AppendDescription(ref sb, GetOptionalString(field, "description"), includeDescriptions, 1);
                    sb.Append("  ");
                    sb.Append(BuildField(field));
                    sb.AppendLine();
                }
            }

            sb.Append('}');
            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildUnion(JsonElement type, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(type, "description"), includeDescriptions);

            string name = GetRequiredString(type, "name");
            sb.Append("union ");
            sb.Append(name);

            if (TryGetArray(type, "possibleTypes", out JsonElement possibleTypes))
            {
                string[] names = possibleTypes.EnumerateArray()
                                              .Select(static typeRef => GetTypeReferenceName(typeRef))
                                              .Where(static value => !string.IsNullOrWhiteSpace(value))
                                              .Cast<string>()
                                              .ToArray();

                if (names.Length > 0)
                {
                    sb.Append(" = ");
                    sb.Append(string.Join(" | ", names));
                }
            }

            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildEnum(JsonElement type, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(type, "description"), includeDescriptions);

            string name = GetRequiredString(type, "name");
            sb.Append("enum ");
            sb.Append(name);
            sb.AppendLine(" {");

            if (TryGetArray(type, "enumValues", out JsonElement enumValues))
            {
                foreach (JsonElement enumValue in enumValues.EnumerateArray())
                {
                    AppendDescription(ref sb, GetOptionalString(enumValue, "description"), includeDescriptions, 1);

                    sb.Append("  ");
                    sb.Append(GetRequiredString(enumValue, "name"));
                    AppendDeprecatedDirective(ref sb, enumValue);
                    sb.AppendLine();
                }
            }

            sb.Append('}');
            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildInputObject(JsonElement type, bool includeDescriptions)
    {
        var sb = new PooledStringBuilder();

        try
        {
            AppendDescription(ref sb, GetOptionalString(type, "description"), includeDescriptions);

            string name = GetRequiredString(type, "name");
            sb.Append("input ");
            sb.Append(name);
            sb.AppendLine(" {");

            if (TryGetArray(type, "inputFields", out JsonElement inputFields))
            {
                foreach (JsonElement inputField in inputFields.EnumerateArray())
                {
                    AppendDescription(ref sb, GetOptionalString(inputField, "description"), includeDescriptions, 1);
                    sb.Append("  ");
                    sb.Append(BuildInputValue(inputField));
                    sb.AppendLine();
                }
            }

            sb.Append('}');
            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildField(JsonElement field)
    {
        string name = GetRequiredString(field, "name");
        string type = FormatTypeReference(field.GetProperty("type"));

        var sb = new PooledStringBuilder();

        try
        {
            sb.Append(name);

            if (TryGetArray(field, "args", out JsonElement args) && args.GetArrayLength() > 0)
            {
                sb.Append('(');
                sb.Append(JoinInputValues(args.EnumerateArray(), includeDescriptionsInline: false));
                sb.Append(')');
            }

            sb.Append(": ");
            sb.Append(type);
            AppendDeprecatedDirective(ref sb, field);

            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string BuildInputValue(JsonElement inputValue)
    {
        var sb = new PooledStringBuilder();

        try
        {
            sb.Append(GetRequiredString(inputValue, "name"));
            sb.Append(": ");
            sb.Append(FormatTypeReference(inputValue.GetProperty("type")));

            string? defaultValue = GetOptionalString(inputValue, "defaultValue");

            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                sb.Append(" = ");
                sb.Append(defaultValue);
            }

            AppendDeprecatedDirective(ref sb, inputValue);

            return sb.ToStringAndDispose();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static string JoinInputValues(IEnumerable<JsonElement> inputValues, bool includeDescriptionsInline)
    {
        if (includeDescriptionsInline)
            throw new NotSupportedException("Inline descriptions are not supported for SDL argument rendering.");

        return string.Join(", ", inputValues.Select(BuildInputValue));
    }

    private static string BuildImplementsClause(JsonElement type)
    {
        if (!TryGetArray(type, "interfaces", out JsonElement interfaces) || interfaces.GetArrayLength() == 0)
            return string.Empty;

        string[] names = interfaces.EnumerateArray()
                                   .Select(static typeRef => GetTypeReferenceName(typeRef))
                                   .Where(static value => !string.IsNullOrWhiteSpace(value))
                                   .Cast<string>()
                                   .Distinct(StringComparer.Ordinal)
                                   .ToArray();

        return names.Length == 0 ? string.Empty : $"implements {string.Join(" & ", names)}";
    }

    private static void AppendDeprecatedDirective(ref PooledStringBuilder sb, JsonElement element)
    {
        if (GetOptionalBoolean(element, "isDeprecated") != true)
            return;

        string? reason = GetOptionalString(element, "deprecationReason");

        if (reason.IsNullOrWhiteSpace() || reason == "No longer supported")
        {
            sb.Append(" @deprecated");
            return;
        }

        sb.Append(" @deprecated(reason: ");
        sb.Append(ToGraphQlString(reason));
        sb.Append(')');
    }

    private static string FormatTypeReference(JsonElement typeRef)
    {
        string kind = GetRequiredString(typeRef, "kind");

        return kind switch
        {
            "NON_NULL" => $"{FormatTypeReference(typeRef.GetProperty("ofType"))}!",
            "LIST" => $"[{FormatTypeReference(typeRef.GetProperty("ofType"))}]",
            _ => GetTypeReferenceName(typeRef) ?? throw new InvalidOperationException("The GraphQL type reference did not contain a name.")
        };
    }

    private static string? GetTypeReferenceName(JsonElement typeRef)
    {
        if (typeRef.ValueKind != JsonValueKind.Object)
            return null;

        if (typeRef.TryGetProperty("name", out JsonElement name) && name.ValueKind == JsonValueKind.String)
            return name.GetString();

        if (typeRef.TryGetProperty("ofType", out JsonElement ofType))
            return GetTypeReferenceName(ofType);

        return null;
    }

    private static void AppendDescription(ref PooledStringBuilder sb, string? description, bool includeDescriptions, int indentLevel = 0)
    {
        if (!includeDescriptions || description.IsNullOrWhiteSpace())
            return;

        string indent = new(' ', indentLevel * 2);
        string normalized = description.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);

        sb.Append(indent);
        sb.AppendLine("\"\"\"");

        foreach (string line in normalized.Split('\n'))
        {
            sb.Append(indent);
            sb.AppendLine(EscapeBlockString(line));
        }

        sb.Append(indent);
        sb.AppendLine("\"\"\"");
    }

    private static string EscapeBlockString(string value) => value.Replace("\"\"\"", "\\\"\"\"", StringComparison.Ordinal);

    private static string ToGraphQlString(string value) => JsonSerializer.Serialize(value);

    private static bool ShouldSkipType(JsonElement type)
    {
        string? name = GetOptionalString(type, "name");

        if (string.IsNullOrWhiteSpace(name))
            return true;

        if (name.StartsWith("__", StringComparison.Ordinal))
            return true;

        return name is "String" or "Boolean" or "Int" or "Float" or "ID";
    }

    private static bool ShouldSkipDirective(JsonElement directive)
    {
        string? name = GetOptionalString(directive, "name");

        return name is "skip" or "include" or "deprecated" or "specifiedBy" or "oneOf";
    }

    private static bool TryGetArray(JsonElement element, string propertyName, out JsonElement array)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out array) &&
            array.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        array = default;
        return false;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out JsonElement value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString()!;
        }

        throw new InvalidOperationException($"The introspection payload was missing the required string property '{propertyName}'.");
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out JsonElement value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static bool? GetOptionalBoolean(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out JsonElement value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static string? GetNestedName(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out JsonElement nested) &&
            nested.ValueKind == JsonValueKind.Object)
        {
            return GetOptionalString(nested, "name");
        }

        return null;
    }
}
