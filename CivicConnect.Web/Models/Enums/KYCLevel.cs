namespace CivicConnect.Web.Models.Enums
{
    public enum KYCLevel
    {
        Unverified = 0,
        EmailVerified = 1,
        PhoneVerified = 2,
        PendingReview = 3,
        Verified = 4,
        Rejected = 5
    }
}
