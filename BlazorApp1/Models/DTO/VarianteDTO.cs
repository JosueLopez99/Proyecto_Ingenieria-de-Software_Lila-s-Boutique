namespace BlazorApp1.Models.DTO
{
    public class VarianteDTO
    {
        public int IdVariante { get; set; }
        public string CodigoPrenda { get; set; } = string.Empty;
        public string NombrePrenda { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int StockFisicoActual { get; set; }
        public int LimiteAlerta { get; set; }
    }
}
