namespace IPAY.Domain.Enums
{
    // Role carried by every user and emitted as the JWT role claim.
    // Matches the Guest -> Customer -> Seller hierarchy (Admin is a separate role).
    public enum UserRole
    {
        Guest = 0,
        Customer = 1,
        Seller = 2,
        Admin = 3,
        Moderator=4
    }
}
