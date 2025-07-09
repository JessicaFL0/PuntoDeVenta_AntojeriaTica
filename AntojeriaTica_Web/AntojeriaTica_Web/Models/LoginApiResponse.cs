namespace AntojeriaTica_Web.Models
{
    public class LoginApiResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public LoginUser? User { get; set; }
    }

    public class LoginUser
    {
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Correo { get; set; }
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
    }
}
