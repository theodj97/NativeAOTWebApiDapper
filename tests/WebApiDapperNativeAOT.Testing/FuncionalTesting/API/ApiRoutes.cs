namespace WebApiDapperNativeAOT.Testing.FuncionalTesting.API;

internal static class ApiRoutes
{
    internal static class Todo
    {
        public static string GetById(int id) => $"/todos/{id}";
    }

}
