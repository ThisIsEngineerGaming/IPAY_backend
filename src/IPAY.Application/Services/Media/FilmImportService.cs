using System.Globalization;
using AutoMapper;
using IPAY.Application.DTOs.Media;
using IPAY.Application.Interfaces.Media;
using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos;


//namespace IPAY.Application.Services.Media;

//public sealed class FilmImportService(
//    IOmdbService omdb,
//    IFilmRepo films,
//    IRepository<Genre> genres,
//    IMapper mapper) : IFilmImportService
//{
//    public async Task<FilmImportResult> ImportAsync(string imdbId, CancellationToken cancellationToken = default)
//    {
//        if (string.IsNullOrWhiteSpace(imdbId))
//            throw new ArgumentException("An IMDb ID is required.", nameof(imdbId));

//        imdbId = imdbId.Trim();

//        var existing = await films.GetByImdbIdAsync(imdbId);
//        if (existing is not null)
//            return new FilmImportResult(FilmImportStatus.AlreadyImported, mapper.Map<FilmDto>(existing));

//        var omdbFilm = await omdb.GetByImdbIdAsync(imdbId, cancellationToken);
//        if (string.Equals(omdbFilm.Response, "False", StringComparison.OrdinalIgnoreCase))
//            return new FilmImportResult(FilmImportStatus.NotFoundInOmdb);

//        if (!string.Equals(omdbFilm.Type, "movie", StringComparison.OrdinalIgnoreCase))
//            return new FilmImportResult(FilmImportStatus.NotAMovie);

//        // Link only genres that already exist (case-insensitive by name). Missing ones are reported, not created.
//        var existingGenres = await genres.GetAllAsync();
//        var genreIdByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
//        foreach (var genre in existingGenres)
//            genreIdByName.TryAdd(genre.Name.Trim(), genre.Id);

//        var genreIds = new List<int>();
//        var unmatched = new List<string>();
//        foreach (var name in SplitList(omdbFilm.Genre))
//        {
//            if (genreIdByName.TryGetValue(name, out var genreId))
//            {
//                if (!genreIds.Contains(genreId)) genreIds.Add(genreId);
//            }
//            else
//            {
//                unmatched.Add(name);
//            }
//        }

//        var request = new SaveFilmDto
//        {
//            Name = Clean(omdbFilm.Title) ?? string.Empty,
//            Description = Clean(omdbFilm.Plot) ?? string.Empty,
//            Year = ParseYear(omdbFilm.Year),
//            Rating = ParseRating(omdbFilm.ImdbRating),
//            Director = Clean(omdbFilm.Director) ?? string.Empty,
//            AgeRating = Clean(omdbFilm.Rated) ?? string.Empty,
//            PosterUrl = Clean(omdbFilm.Poster) ?? string.Empty,
//            VideoUrl = string.Empty, // OMDb has no video; uploaded separately
//            GenreIds = genreIds,
//            ImdbId = Clean(omdbFilm.ImdbId) ?? imdbId
//        };

//        var created = await films.AddAsync(mapper.Map<Film>(request));
//        return new FilmImportResult(FilmImportStatus.Imported, mapper.Map<FilmDto>(created), unmatched);
//    }

//    /// OMDb marks missing values as "N/A".
//    private static string? Clean(string? value) =>
//        string.IsNullOrWhiteSpace(value) || value.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase)
//            ? null
//            : value.Trim();

//    private static IEnumerable<string> SplitList(string? value) =>
//        Clean(value)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
//        ?? [];

//    /// OMDb years can be "2010" or "2010–2015"; take the first four digits.
//    private static int ParseYear(string? value) =>
//        Clean(value) is { Length: >= 4 } year && int.TryParse(year.AsSpan(0, 4), out var parsed) ? parsed : 0;

//    private static double ParseRating(string? value) =>
//        double.TryParse(Clean(value), NumberStyles.Float, CultureInfo.InvariantCulture, out var rating) ? rating : 0;
//}
