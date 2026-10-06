namespace IPAY.Application.DTOs.Auth
{
    public class VerifyCodeDto
    {
        public string ChallengeId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ResendCodeDto
    {
        public string ChallengeId { get; set; } = string.Empty;
    }
}
