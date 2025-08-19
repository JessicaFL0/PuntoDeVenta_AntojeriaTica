namespace AntojeriaTica_Web.Models
{
    public class HomeIndexViewModel
    {
        public bool IsLogged { get; set; }
        public bool IsAdmin { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? NombreUsuario { get; set; }

        public DashboardModel? Dashboard { get; set; }
    }
}
