using System.ComponentModel.DataAnnotations;

public class NuevaVarianteDTO
{
    [Required]
    public string Talla { get; set; } = string.Empty;
    [Required]
    public string Color { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "El stock inicial no puede ser negativo.")]
    public int StockInicial { get; set; }
    public int LimiteAlerta { get; set; } = 10;
}