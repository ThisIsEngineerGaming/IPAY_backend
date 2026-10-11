using Google.Cloud.Firestore;
using IPAY.Application.Interfaces.Auth;
using IPAY.Domain.Entities.Auth;
using IPAY.Domain.Interfaces.ForRepos.Auth;
using IPAY.Infrastructure.Persistence.Documents;

namespace IPAY.Infrastructure.Repositories.Auth
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CollectionReference _collection;

        public RefreshTokenRepository(FirestoreDb db)
        {
            _collection = db.Collection("refreshTokens");
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _collection.Document(token.Id)
                .SetAsync(RefreshTokenDocument.FromEntity(token));
        }

        public async Task<RefreshToken?> GetByIdAsync(string id)
        {
            var snap = await _collection.Document(id).GetSnapshotAsync();
            return snap.Exists
                ? snap.ConvertTo<RefreshTokenDocument>().ToEntity()
                : null;
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            await _collection.Document(token.Id)
                .SetAsync(RefreshTokenDocument.FromEntity(token));
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            var query = _collection
                .WhereEqualTo("UserId", userId)
                .WhereEqualTo("IsRevoked", false);

            var snap = await query.GetSnapshotAsync();
            foreach (var doc in snap.Documents)
            {
                var entity = doc.ConvertTo<RefreshTokenDocument>().ToEntity();
                entity.IsRevoked = true;
                await UpdateAsync(entity);
            }
        }
    }
}
