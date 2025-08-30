using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.Dtos;

public class CreatePropertyFormRequest
{
    [Required, MinLength(1)]
    public string IdOwner { get; set; } = default!;

    [Required, MinLength(1)]
    public string Name { get; set; } = default!;

    [Required, MinLength(1)]
    public string AddressProperty { get; set; } = default!;

    [Range(0, double.MaxValue)]
    public decimal PriceProperty { get; set; }

    [Required, MinLength(1)]
    public string Image { get; set; } = default!;
}
