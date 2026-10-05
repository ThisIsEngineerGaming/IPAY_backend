using IPAY.Domain.Entities.Media;
using IPAY.Domain.Interfaces.ForRepos.Media;
using IPAY.Infrastructure.Persistence;
using IPAY.Infrastructure.Persistence.Documents;
using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Repositories.Media
{
    public class GenreRepo(FirestoreDb database, string collectionName)
        : FirestoreRepository<Genre, GenreDocument>(
            database, collectionName, GenreDocument.FromEntity, document => document.ToEntity()),
          IGenreRepo
    {
        private readonly CollectionReference _genres = database.Collection(collectionName);

        public Task AddFilmAsync(int genreId, int filmId) => UpdateIfExistsAsync(genreId, "FilmIds", FieldValue.ArrayUnion(filmId));
        public Task RemoveFilmAsync(int genreId, int filmId) => UpdateIfExistsAsync(genreId, "FilmIds", FieldValue.ArrayRemove(filmId));
        public Task AddSeriesAsync(int genreId, int seriesId) => UpdateIfExistsAsync(genreId, "SerialIds", FieldValue.ArrayUnion(seriesId));
        public Task RemoveSeriesAsync(int genreId, int seriesId) => UpdateIfExistsAsync(genreId, "SerialIds", FieldValue.ArrayRemove(seriesId));

        public async Task ClearAsync(int genreId)
        {
            var document = _genres.Document(genreId.ToString());
            try
            {
                await document.UpdateAsync(new Dictionary<string, object>
                {
                    ["FilmIds"] = new List<int>(),
                    ["SerialIds"] = new List<int>()
                });
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                // missing genre: nothing to clear
            }
        }

        private async Task UpdateIfExistsAsync(int genreId, string field, FieldValue value)
        {
            try
            {
                await _genres.Document(genreId.ToString()).UpdateAsync(field, value);
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                // UpdateAsync fails on a missing document; a missing genre is a no-op, as before.
            }
        }
    }
}
