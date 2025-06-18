namespace AntojeriaTica_Api.Models
{
    public class UsuarioModel
    {
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Correo { get; set; }
        public string? Cedula { get; set; }
        public string? ContrasenaHash { get; set; }
        public string? Estado { get; set; }
        public int? IdRol { get; set; }
        public string? NombreRol { get; set; }
    }
}
