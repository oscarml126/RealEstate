using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealEstate.Domain;

public class Property
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string IdOwner { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string AddressProperty { get; set; } = default!;
    public decimal PriceProperty { get; set; }
    public string Image { get; set; } = default!;

    // Normalizados (minúsculas/sin tildes) - usados si existen
    public string? NameNorm { get; set; }
    public string? AddressNorm { get; set; }
}
