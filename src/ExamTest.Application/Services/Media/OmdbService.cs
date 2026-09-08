using System.Net.Http.Json;
using ExamTest.Application.DTOs.Media;

namespace ExamTest.Application.Services.Media;

public sealed class OmdbService : IOmdbService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public OmdbService(HttpClient httpClient, string apiKey, string baseUrl)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<OmdbSearchResponseDto> SearchAsync(
        string query,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("A search query is required.", nameof(query));

        page = Math.Clamp(page, 1, 100);

        var url = BuildUrl(new Dictionary<string, string>
        {
            ["apikey"] = _apiKey,
            ["s"] = query.Trim(),
            ["page"] = page.ToString()
        });

        return await _httpClient.GetFromJsonAsync<OmdbSearchResponseDto>(url, cancellationToken)
            ?? new OmdbSearchResponseDto { Response = "False", Error = "OMDb returned an empty response." };
    }

    public async Task<OmdbFilmDto> GetByImdbIdAsync(
        string imdbId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imdbId))
            throw new ArgumentException("An IMDb ID is required.", nameof(imdbId));

        var url = BuildUrl(new Dictionary<string, string>
        {
            ["apikey"] = _apiKey,
            ["i"] = imdbId.Trim(),
            ["plot"] = "full"
        });

        return await _httpClient.GetFromJsonAsync<OmdbFilmDto>(url, cancellationToken)
            ?? new OmdbFilmDto { Response = "False", Error = "OMDb returned an empty response." };
    }

    private string BuildUrl(Dictionary<string, string> parameters)
    {
        var query = string.Join("&", parameters.Select(x =>
            $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

        return $"{_baseUrl}/?{query}";
    }
}
