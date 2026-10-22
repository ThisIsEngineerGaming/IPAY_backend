namespace IPAY.Application.DTOs.Media;
//importing film from omdb
public enum FilmImportStatus
{
    Imported,
    AlreadyImported,
    NotFoundInOmdb,
    NotAMovie
}

/// The stored film (Imported) or the existing one (AlreadyImported)
/// OMDb genre names that have no matching genre in our database. They are NOT created.
public sealed record FilmImportResult(
    FilmImportStatus Status,
    FilmDto? Film = null,
    IReadOnlyList<string>? UnmatchedGenres = null);
