using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GraphQl.Schema.Conversion.Abstract;

namespace Soenneker.GraphQl.Schema.Conversion.Registrars;

/// <summary>
/// Registers the GraphQL introspection-to-SDL converter.
/// </summary>
public static class GraphQlSchemaConversionUtilRegistrar
{
    /// <summary>
    /// Adds the stateless <see cref="IGraphQlSchemaConversionUtil"/> as a singleton service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddGraphQlSchemaConversionUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IGraphQlSchemaConversionUtil, GraphQlSchemaConversionUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGraphQlSchemaConversionUtil"/> as a scoped service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddGraphQlSchemaConversionUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IGraphQlSchemaConversionUtil, GraphQlSchemaConversionUtil>();

        return services;
    }
}
