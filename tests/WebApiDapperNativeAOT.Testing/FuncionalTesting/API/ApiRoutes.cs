namespace WebApiDapperNativeAOT.Testing.FuncionalTesting.API;

internal static class ApiRoutes
{
    internal static class Todo
    {
        public static string GetById(int id) => $"/todos/{id}";

        public static string Search(string[]? title = null, string[]? description = null, int? createdBy = null, int[]? assignedTo = null, DateTime[]? targetDate = null, bool? isComplete = null)
        {
            var queryParameters = new List<string>();

            if (title != null && title.Length != 0)
                queryParameters.Add($"title={string.Join("&title=", title)}");

            if (description != null && description.Length != 0)
                queryParameters.Add($"description={string.Join("&description=", description)}");

            if (createdBy.HasValue)
                queryParameters.Add($"createdBy={createdBy.Value}");

            if (assignedTo != null && assignedTo.Length != 0)
                queryParameters.Add($"assignedTo={string.Join("&assignedTo=", assignedTo)}");

            if (targetDate != null && targetDate.Length != 0)
                queryParameters.Add($"targetDate={string.Join("&targetDate=", targetDate.Select(d => d.ToString("o")))}");

            if (isComplete.HasValue)
                queryParameters.Add($"isComplete={isComplete.Value}");

            if (queryParameters.Count == 0)
                return "/todos";

            var queryString = string.Join("&", queryParameters);
            return $"/todos?{queryString}";
        }
    }
}
