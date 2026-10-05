using IPAY.Application.Interfaces.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace IPAY.Infrastructure.Caching
{
    /// <summary>
    /// Keeps pending two-factor challenges in this server's memory. They only live a few minutes,
    /// so losing them on a restart just means "sign in again". If you ever run several backend
    /// instances behind a load balancer, swap this for Redis/Firestore (the interface stays the same).
    /// </summary>
    public class MemoryTwoFactorStore : ITwoFactorStore
    {
        private const string KeyPrefix = "2fa:";
        private readonly IMemoryCache _cache;

        public MemoryTwoFactorStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Save(TwoFactorChallenge challenge, TimeSpan lifetime) =>
            _cache.Set(KeyPrefix + challenge.Id, challenge, lifetime);

        public TwoFactorChallenge? Get(string challengeId) =>
            _cache.TryGetValue(KeyPrefix + challengeId, out TwoFactorChallenge? challenge) ? challenge : null;

        public void Remove(string challengeId) => _cache.Remove(KeyPrefix + challengeId);
    }
}
