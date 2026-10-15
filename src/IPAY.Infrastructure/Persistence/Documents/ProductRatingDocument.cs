using Google.Cloud.Firestore;

namespace IPAY.Infrastructure.Persistence.Documents;

/// <summary>
/// One user's rating of one product. The Firestore document id is "{ProductId}_{UserId}", which is what
/// guarantees a user can rate a product only once.
/// </summary>
[FirestoreData]
public class ProductRatingDocument
{
    [FirestoreProperty] public int ProductId { get; set; }
    [FirestoreProperty] public string UserId { get; set; } = string.Empty;
    [FirestoreProperty] public int Value { get; set; }
    [FirestoreProperty] public DateTime CreatedAt { get; set; }
}
