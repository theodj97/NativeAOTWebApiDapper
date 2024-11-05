namespace WebApiDapperNativeAOT.Testing.FuncionalTesting.API.Todo;

internal static class Routes
{
    internal static class Todo
    {
        public static string GetById(int id) => $"/todos/{id}";

        public static string Search(string[]? title = null, string[]? description = null, int? createdBy = null, int[]? assignedTo = null, DateTime[]? targetDate = null, bool? isComplete = null)
        {
            var queryParameters = new List<string>();

            if (title != null && title.Length > 0)
                queryParameters.AddRange(title.Select(t => $"title={Uri.EscapeDataString(t).Replace("+", "%20")}"));

            if (description != null && description.Length > 0)
                queryParameters.AddRange(description.Select(d => $"description={Uri.EscapeDataString(d).Replace("+", "%20")}"));

            if (createdBy.HasValue)
                queryParameters.Add($"createdBy={createdBy.Value}");

            if (assignedTo != null && assignedTo.Length > 0)
                queryParameters.AddRange(assignedTo.Select(a => $"assignedTo={a}"));

            if (targetDate != null && targetDate.Length > 0)
                queryParameters.AddRange(targetDate.Select(d => $"targetDate={Uri.EscapeDataString(d.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")).Replace("+", "%20")}"));

            if (isComplete.HasValue)
                queryParameters.Add($"isComplete={isComplete.Value.ToString().ToLower()}");

            var queryString = string.Join("&", queryParameters);
            return string.IsNullOrEmpty(queryString) ? "/todos" : $"/todos?{queryString}";
        }

        public static string Create() => "/todos";
    }
}
