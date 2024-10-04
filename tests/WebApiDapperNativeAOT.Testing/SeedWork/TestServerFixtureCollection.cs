using System.Diagnostics.CodeAnalysis;

namespace WebApiDapperNativeAOT.Testing.SeedWork;

[ExcludeFromCodeCoverage]
[CollectionDefinition(nameof(TestServerFixtureCollection))]
public class TestServerFixtureCollection : ICollectionFixture<TestServerFixture>
{
}
