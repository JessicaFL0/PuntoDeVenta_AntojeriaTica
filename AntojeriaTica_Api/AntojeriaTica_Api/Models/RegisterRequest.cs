namespace AntojeriaTica_Api.Models
{
    public class RegisterRequest
    {
        public string? NombreCompleto { get; set; }
        public string? Correo { get; set; }
        public string? Cedula { get; set; }
        public string Contrasena { get; set; } = string.Empty;
        public string? Estado { get; set; }
        public int? IdRol { get; set; }
    }
}
