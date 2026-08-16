namespace EnergyShare_v3.Web.Models.User
{
    //Email, Role et Status n'y figurent pas car UpdateMyUserProfile ne les modifie pas.
    public class UpdateUserProfileFormModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SocieteName { get; set; }
        public string? NumeroEntreprise { get; set; }
    }
}
