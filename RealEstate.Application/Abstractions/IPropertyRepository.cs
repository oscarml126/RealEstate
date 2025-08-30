using RealEstate.Application.Dtos;

namespace RealEstate.Application.Abstractions;

public record PagedResult<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);

public interface IPropertyRepository
{
    Task<PagedResult<PropertyDto>> FindAsync(
        string? name, string? address, decimal? priceMin, decimal? priceMax,
        int page, int pageSize, CancellationToken ct = default);

    Task<PropertyDto?> GetByIdAsync(string id, CancellationToken ct = default);

    Task<PropertyDto> CreateAsync(CreatePropertyRequest request, CancellationToken ct = default);

    // NUEVO: crear N propiedades automáticamente (1..50)
    Task<IReadOnlyList<PropertyDto>> CreateAutoAsync(int count, CancellationToken ct = default);

    Task<int> SeedAsync(CancellationToken ct = default);
}
