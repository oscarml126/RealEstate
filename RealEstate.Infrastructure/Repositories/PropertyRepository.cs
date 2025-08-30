using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using RealEstate.Application.Abstractions;
using RealEstate.Application.Dtos;
using RealEstate.Application.Utils;
using RealEstate.Domain;

namespace RealEstate.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly IMongoCollection<Property> _col;

    public PropertyRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<Property>("properties");
    }

    public async Task<PagedResult<PropertyDto>> FindAsync(
        string? name, string? address, decimal? priceMin, decimal? priceMax,
        int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var nameNorm = TextNormalizer.Normalize(name);
        var addrNorm = TextNormalizer.Normalize(address);

        var fb = Builders<Property>.Filter;
        var filters = new List<FilterDefinition<Property>>();

        if (!string.IsNullOrEmpty(nameNorm))
        {
            // Usa NameNorm (prefijo) o Name (case-insensitive) como fallback
            filters.Add(fb.Or(
                fb.Regex(x => x.NameNorm!, new BsonRegularExpression("^" + RegexEscape(nameNorm))),
                fb.Regex(x => x.Name, new BsonRegularExpression(new Regex("^" + RegexEscape(name ?? ""), RegexOptions.IgnoreCase)))
            ));
        }

        if (!string.IsNullOrEmpty(addrNorm))
        {
            filters.Add(fb.Or(
                fb.Regex(x => x.AddressNorm!, new BsonRegularExpression("^" + RegexEscape(addrNorm))),
                fb.Regex(x => x.AddressProperty, new BsonRegularExpression(new Regex("^" + RegexEscape(address ?? ""), RegexOptions.IgnoreCase)))
            ));
        }

        if (priceMin.HasValue) filters.Add(fb.Gte(x => x.PriceProperty, priceMin.Value));
        if (priceMax.HasValue) filters.Add(fb.Lte(x => x.PriceProperty, priceMax.Value));

        var filter = filters.Count > 0 ? fb.And(filters) : fb.Empty;

        var find = _col.Find(filter)
                       .SortBy(x => x.Name)
                       .Skip((page - 1) * pageSize)
                       .Limit(pageSize)
                       .Project(p => new PropertyDto(p.IdOwner, p.Name, p.AddressProperty, p.PriceProperty, p.Image));

        var itemsTask = find.ToListAsync(ct);
        var totalTask = _col.CountDocumentsAsync(filter, cancellationToken: ct);

        await Task.WhenAll(itemsTask, totalTask);

        return new PagedResult<PropertyDto>(itemsTask.Result, totalTask.Result, page, pageSize);
    }

    public async Task<PropertyDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _)) return null;

        var p = await _col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        return p is null ? null : new PropertyDto(p.IdOwner, p.Name, p.AddressProperty, p.PriceProperty, p.Image);
    }

    public async Task<PropertyDto> CreateAsync(CreatePropertyRequest request, CancellationToken ct = default)
    {
        var entity = new Property
        {
            Id = ObjectId.GenerateNewId().ToString(),
            IdOwner = request.IdOwner,
            Name = request.Name,
            AddressProperty = request.AddressProperty,
            PriceProperty = request.PriceProperty,
            Image = request.Image
        };

        entity = Enrich(entity);
        await _col.InsertOneAsync(entity, cancellationToken: ct);

        return new PropertyDto(entity.IdOwner, entity.Name, entity.AddressProperty, entity.PriceProperty, entity.Image);
    }

    public async Task<IReadOnlyList<PropertyDto>> CreateAutoAsync(int count, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 50);

        var rnd = new Random();
        var names = new[] { "Apto Centro", "Casa Norte", "Loft Chicó", "Studio Parque", "Penthouse Sur", "Dúplex Cedritos" };
        var streets = new[] { "Cra", "Cl", "Av", "Trans" };
        var zones = new[] { "Bogotá", "Medellín", "Cali", "Barranquilla", "Bucaramanga" };

        var list = new List<Property>(capacity: count);
        for (int i = 0; i < count; i++)
        {
            var name = names[rnd.Next(names.Length)];
            var addr = $"{streets[rnd.Next(streets.Length)]} {rnd.Next(1, 160)} #{rnd.Next(1, 100)}-{rnd.Next(1, 100)}, {zones[rnd.Next(zones.Length)]}";
            var price = rnd.Next(120, 1200) * 1_000_000m; // 120M .. 1200M
            var imgSeed = rnd.Next(1, 9999);

            var entity = new Property
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IdOwner = $"own-{rnd.Next(100, 999)}",
                Name = name,
                AddressProperty = addr,
                PriceProperty = price,
                Image = $"https://picsum.photos/seed/{imgSeed}/600/400"
            };

            list.Add(Enrich(entity));
        }

        await _col.InsertManyAsync(list, cancellationToken: ct);

        return list
            .Select(p => new PropertyDto(p.IdOwner, p.Name, p.AddressProperty, p.PriceProperty, p.Image))
            .ToList();
    }

    public async Task<int> SeedAsync(CancellationToken ct = default)
    {
        var count = await _col.CountDocumentsAsync(FilterDefinition<Property>.Empty, cancellationToken: ct);
        if (count > 0) return 0;

        var demo = new[]
        {
            new Property { Id = ObjectId.GenerateNewId().ToString(), IdOwner = "own-001", Name = "Apto Centro", AddressProperty = "Cra 7 #12-34, Bogotá", PriceProperty = 350_000_000, Image = "https://picsum.photos/seed/1/600/400" },
            new Property { Id = ObjectId.GenerateNewId().ToString(), IdOwner = "own-002", Name = "Casa Norte",  AddressProperty = "Cl 150 #20-50, Bogotá", PriceProperty = 890_000_000, Image = "https://picsum.photos/seed/2/600/400" },
            new Property { Id = ObjectId.GenerateNewId().ToString(), IdOwner = "own-003", Name = "Loft Chicó",  AddressProperty = "Cra 11 #86-15, Bogotá", PriceProperty = 620_000_000, Image = "https://picsum.photos/seed/3/600/400" }
        }.Select(Enrich).ToList();

        await _col.InsertManyAsync(demo, cancellationToken: ct);
        return demo.Count;
    }

    private static Property Enrich(Property p)
    {
        p.NameNorm = TextNormalizer.Normalize(p.Name);
        p.AddressNorm = TextNormalizer.Normalize(p.AddressProperty);
        return p;
    }

    private static string RegexEscape(string value) => Regex.Escape(value);
}
