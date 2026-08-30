[![](https://img.shields.io/nuget/v/soenneker.graphql.schema.conversion.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graphql.schema.conversion/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graphql.schema.conversion/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graphql.schema.conversion/actions/workflows/publish-package.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graphql.schema.conversion/build-and-test.yml?style=for-the-badge&label=build)](https://github.com/soenneker/soenneker.graphql.schema.conversion/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graphql.schema.conversion/codeql.yml?style=for-the-badge&label=codeql)](https://github.com/soenneker/soenneker.graphql.schema.conversion/actions/workflows/codeql.yml)
[![](https://img.shields.io/nuget/dt/soenneker.graphql.schema.conversion.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graphql.schema.conversion/)

# Soenneker.GraphQl.Schema.Conversion

Converts a GraphQL introspection response into Schema Definition Language (SDL). Use it when a service exposes introspection JSON but a generator, registry, or source-control workflow needs a `.graphql` schema.

## Installation

```bash
dotnet add package Soenneker.GraphQl.Schema.Conversion
```

## Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Soenneker.GraphQl.Schema.Conversion.Abstract;
using Soenneker.GraphQl.Schema.Conversion.Registrars;

services.AddGraphQlSchemaConversionUtilAsSingleton();

IGraphQlSchemaConversionUtil converter =
    serviceProvider.GetRequiredService<IGraphQlSchemaConversionUtil>();
```

The converter is synchronous and stateless. Singleton registration is the usual choice; scoped registration is also available with `AddGraphQlSchemaConversionUtilAsScoped()`.

## Convert an introspection response

```csharp
string introspectionJson = await File.ReadAllTextAsync("introspection.json");
string schema = converter.Convert(introspectionJson);

await File.WriteAllTextAsync("schema.graphql", schema);
```

Pass `includeDescriptions: false` when the output should exclude schema, type, field, enum-value, and input-field descriptions:

```csharp
string schema = converter.Convert(introspectionJson, includeDescriptions: false);
```

If the response is already parsed, pass its `JsonDocument` directly:

```csharp
using JsonDocument document = JsonDocument.Parse(introspectionJson);
string schema = converter.Convert(document);
```

The input may be a normal GraphQL response (`data.__schema`), an object containing `__schema`, or the schema object itself. The converter emits schema operation mappings, custom scalars and directives, objects, interfaces, unions, enums, input objects, type modifiers, default values, deprecations, `@specifiedBy`, and `@oneOf` metadata. Introspection types and built-in scalar/directive definitions are omitted while references to them remain valid SDL.

Malformed JSON throws `JsonException`. A payload without a recognizable schema, a `types` array, or another required introspection property throws `InvalidOperationException`.
