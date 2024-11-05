using System.Reflection;
using Xunit.Sdk;

namespace WebApiDapperNativeAOT.Testing.SeedWork;

public class ResetDatabaseAttribute : BeforeAfterTestAttribute
{
    public override void After(MethodInfo methodUnderTest)
    {
        base.After(methodUnderTest);
    }

    public override void Before(MethodInfo methodUnderTest)
    {
        TestServerFixture.ResetDatabase().Wait();
    }
}
