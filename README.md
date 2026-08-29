[![](https://img.shields.io/nuget/v/soenneker.graphql.schema.conversion.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graphql.schema.conversion/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.graphql.schema.conversion/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.graphql.schema.conversion/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.graphql.schema.conversion.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.graphql.schema.conversion/)

# Soenneker.GraphQl.Schema.Conversion

A GraphQL schema conversion utility.

## Install

```bash
dotnet add package Soenneker.GraphQl.Schema.Conversion
```

## Quick start

```csharp
using Soenneker.GraphQl.Schema.Conversion.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddGraphQlSchemaConversionUtilAsSingleton();
```

Adds `IGraphQlSchemaConversionUtil` as a singleton service.

## What you get

- `IGraphQlSchemaConversionUtil` — A GraphQL schema conversion utility.
- `GraphQlSchemaConversionUtilRegistrar` — A GraphQL schema conversion utility.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IGraphQlSchemaConversionUtil.Convert(introspectionJson, includeDescriptions)` | Converts GraphQL introspection JSON into SDL. | Returns `string`. |
| `IGraphQlSchemaConversionUtil.Convert(introspectionDocument, includeDescriptions)` | Converts a parsed GraphQL introspection payload into SDL. | Returns `string`. |
| `GraphQlSchemaConversionUtilRegistrar.AddGraphQlSchemaConversionUtilAsSingleton(services)` | Adds `IGraphQlSchemaConversionUtil` as a singleton service. | The same service collection, so additional registrations can be chained. |
| `GraphQlSchemaConversionUtilRegistrar.AddGraphQlSchemaConversionUtilAsScoped(services)` | Adds `IGraphQlSchemaConversionUtil` as a scoped service. | The same service collection, so additional registrations can be chained. |
