using System.Net.Http.Json;
using StudyRhythm.Core.Contracts;

namespace StudyRhythm.Mobile.Services;

/// <summary>Усі запити екрана «Сон» до нашого Web API зібрано в одному місці.</summary>
public sealed class SleepApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<SleepEntryResponse>> GetEntriesAsync(
        string apiAddress,
        CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<List<SleepEntryResponse>>(
            CreateUri(apiAddress), cancellationToken);
        return result ?? [];
    }

    public async Task<SleepEntryResponse> CreateEntryAsync(
        string apiAddress,
        CreateSleepEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            CreateUri(apiAddress), request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SleepEntryResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Сервер не повернув збережений запис.");
    }

    public async Task DeleteEntryAsync(
        string apiAddress,
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(
            CreateUri(apiAddress, $"/{id}"), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static Uri CreateUri(string apiAddress, string suffix = "")
    {
        if (string.IsNullOrWhiteSpace(apiAddress) ||
            !Uri.TryCreate(apiAddress.Trim(), UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Адреса API має починатися з http:// або https://.");
        }

        return new Uri(baseUri.GetLeftPart(UriPartial.Authority) + "/api/sleep" + suffix);
    }
}
