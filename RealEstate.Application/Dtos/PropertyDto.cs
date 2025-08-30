namespace RealEstate.Application.Dtos;

public record PropertyDto(
    string IdOwner,
    string Name,
    string AddressProperty,
    decimal PriceProperty,
    string Image
);
