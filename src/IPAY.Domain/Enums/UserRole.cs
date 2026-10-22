namespace IPAY.Domain.Enums
{
    // Role carried by every user and emitted as the JWT role claim.
    // Matches the Guest -> Customer -> Seller hierarchy (Admin is a separate role).
    public enum UserRole
    {
        Guest = 0,//unathorized
        Customer = 1,//authorized
        Seller = 2,//have permission to post product(and edit only his)!!!
        Admin = 3,//has all rights
        Moderator=4//limited rights(for istance, can't delete ban or make moderators/admins)
    }
}
