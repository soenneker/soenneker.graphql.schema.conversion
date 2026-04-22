using Soenneker.GraphQl.Schema.Conversion.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.GraphQl.Schema.Conversion.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class GraphQlSchemaConversionUtilTests : HostedUnitTest
{
    private readonly IGraphQlSchemaConversionUtil _util;

    public GraphQlSchemaConversionUtilTests(Host host) : base(host)
    {
        _util = Resolve<IGraphQlSchemaConversionUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
