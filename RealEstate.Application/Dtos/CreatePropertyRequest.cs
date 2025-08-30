using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.Dtos;

public class CreatePropertyRequest
{
    [Required, MinLength(1)]
    public string IdOwner { get; set; } = default!;

    [Required, MinLength(1)]
    public string Name { get; set; } = default!;

    [Required, MinLength(1)]
    public string AddressProperty { get; set; } = default!;

    // Usaremos validación extra en el Controller para precio >= 0
    public decimal PriceProperty { get; set; }

    [Required, MinLength(1)]
    public string Image { get; set; } = default!;
}
