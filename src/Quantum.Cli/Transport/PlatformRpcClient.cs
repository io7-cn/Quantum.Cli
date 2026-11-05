using Quantum.Cli.Commands;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Quantum.Cli.Transport;

internal static class PlatformRpcClient
{
    internal static async Task<JsonElement> SendAsync(HttpClient http, string method, object parameters, string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "rpc");
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        request.Content = JsonContent.Create(new { jsonrpc = "2.0", id = Guid.NewGuid().ToString("N"), method, @params = parameters }, options: JsonSerializerOptions.Web);
        using var response = await http.SendAsync(request);
        var payload = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new CliException($"{method} failed with HTTP {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("error", out var rpcError))
        {
            throw new CliException(rpcError.GetProperty("message").GetString() ?? $"{method} failed.");
        }

        var result = document.RootElement.GetProperty("result");
        if (result.TryGetProperty("isSuccess", out var success) && !success.GetBoolean())
        {
            throw new CliException(result.TryGetProperty("message", out var message) ? message.GetString() ?? $"{method} failed." : $"{method} failed.");
        }

        return result.TryGetProperty("value", out var value) ? value.Clone() : result.Clone();
    }
}
