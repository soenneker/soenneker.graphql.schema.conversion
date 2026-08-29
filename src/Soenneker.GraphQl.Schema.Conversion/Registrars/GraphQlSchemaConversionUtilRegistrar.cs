using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.GraphQl.Schema.Conversion.Abstract;

namespace Soenneker.GraphQl.Schema.Conversion.Registrars;

/// <summary>
/// A GraphQL schema conversion utility
/// </summary>
public static class GraphQlSchemaConversionUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGraphQlSchemaConversionUtil"/> as a singleton service. <para/>
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddGraphQlSchemaConversionUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<IGraphQlSchemaConversionUtil, GraphQlSchemaConversionUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGraphQlSchemaConversionUtil"/> as a scoped service. <para/>
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddGraphQlSchemaConversionUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<IGraphQlSchemaConversionUtil, GraphQlSchemaConversionUtil>();

        return services;
    }
}
