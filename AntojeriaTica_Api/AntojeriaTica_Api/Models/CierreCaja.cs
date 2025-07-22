public class CierreCaja
{
    public int IdMovimiento { get; set; }
    public int IdCierre { get; set; }
    public DateTime Fecha { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal MontoFisico { get; set; }
    public string? NotaJustificativa { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
}
