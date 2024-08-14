namespace WebApiDapperNativeAOT.Routes;

public static class RoutesConfiguration
{
    public static void MapRoutes(this WebApplication app)
    {
        TodoRoutes.TodoRoute(app);
    }
}
