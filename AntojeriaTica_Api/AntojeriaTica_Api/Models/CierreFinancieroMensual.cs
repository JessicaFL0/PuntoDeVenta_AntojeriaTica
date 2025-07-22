namespace AntojeriaTica_Api.Models
{
    public class CierreFinancieroMensual
    {
            public int Mes { get; set; }
            public int Anio { get; set; }
            public decimal TotalIngresos { get; set; }  // Solo lectura
            public decimal TotalEgresos { get; set; }   // Solo lectura
            public decimal UtilidadNeta { get; set; }   // Solo lectura
            public DateTime FechaGeneracion { get; set; }
            public string GeneradoPor { get; set; }
            public string ComentarioJustificativo { get; set; }
        }

        public class RegistrarCierreMensualDto
        {
            public int Mes { get; set; }
            public int Anio { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string? Comentario { get; set; }
        }
    }

