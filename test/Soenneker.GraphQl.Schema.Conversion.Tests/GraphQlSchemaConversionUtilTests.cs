using System.Threading.Tasks;
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
    public async Task Converts_one_of_input_object()
    {
        const string introspectionJson = """
                                         {
                                           "data": {
                                             "__schema": {
                                               "queryType": { "name": "Query" },
                                               "mutationType": null,
                                               "subscriptionType": null,
                                               "directives": [],
                                               "types": [
                                                 {
                                                   "kind": "OBJECT",
                                                   "name": "Query",
                                                   "fields": []
                                                 },
                                                 {
                                                   "kind": "INPUT_OBJECT",
                                                   "name": "Choice",
                                                   "isOneOf": true,
                                                   "inputFields": [
                                                     {
                                                       "name": "id",
                                                       "type": { "kind": "SCALAR", "name": "ID", "ofType": null },
                                                       "defaultValue": null
                                                     }
                                                   ]
                                                 }
                                               ]
                                             }
                                           }
                                         }
                                         """;

        string result = _util.Convert(introspectionJson);

        await Assert.That(result).Contains("input Choice @oneOf");
        await Assert.That(result).Contains("id: ID");
    }
}
