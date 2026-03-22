using Soenneker.GraphQl.Schema.Conversion.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.GraphQl.Schema.Conversion.Tests;

[Collection("Collection")]
public sealed class GraphQlSchemaConversionUtilTests : FixturedUnitTest
{
    private readonly IGraphQlSchemaConversionUtil _util;

    public GraphQlSchemaConversionUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IGraphQlSchemaConversionUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
