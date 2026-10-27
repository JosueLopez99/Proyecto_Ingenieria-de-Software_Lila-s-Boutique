using System.ComponentModel.DataAnnotations;

public class NuevaPrendaDTO
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string CodigoIdentificador { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string? DescripcionBreve { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public string Categoria { get; set; } = string.Empty;

    // Una prenda nueva debe crearse con al menos una variante (talla/color)
    public List<NuevaVarianteDTO> Variantes { get; set; } = new();
}