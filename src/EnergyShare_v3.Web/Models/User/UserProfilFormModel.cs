namespace EnergyShare_v3.Web.Models.User
{
    public class UserProfileFormModel
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SocieteName { get; set; }
        public string? NumeroEntreprise { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
