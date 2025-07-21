namespace AntojeriaTica_Api.Models
{
    public class Impuesto
    {
        public int IdImpuesto { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } 
        public decimal Porcentaje { get; set; }
        public bool AplicaEnRestaurante { get; set; }
        public bool EsExonerado { get; set; }
        public bool Estado { get; set; }
    }
}
