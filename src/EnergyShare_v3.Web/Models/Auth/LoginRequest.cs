namespace EnergyShare_v3.Web.Models.Auth
{         /*Jwt*/
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
