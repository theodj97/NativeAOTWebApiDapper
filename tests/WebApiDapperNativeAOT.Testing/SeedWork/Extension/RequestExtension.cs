using System.Text;
using System.Text.Json;

namespace WebApiDapperNativeAOT.Testing.SeedWork.Extension;

public static class RequestExtension
{
    public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string url, T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await client.PostAsync(url, content);
    }

    public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string url, T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await client.PutAsync(url, content);
    }
}
