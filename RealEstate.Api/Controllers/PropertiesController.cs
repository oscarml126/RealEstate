using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Abstractions;
using RealEstate.Application.Dtos;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyRepository _repo;

    public PropertiesController(IPropertyRepository repo) => _repo = repo;

    // GET /api/properties?name=&address=&priceMin=&priceMax=&page=1&pageSize=12
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? name,
        [FromQuery] string? address,
        [FromQuery] decimal? priceMin,
        [FromQuery] decimal? priceMax,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var res = await _repo.FindAsync(name, address, priceMin, priceMax, page, pageSize, ct);
        return Ok(new { items = res.Items, total = res.Total, page = res.Page, pageSize = res.PageSize });
    }

    // GET /api/properties/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? NotFound(new { error = "Property not found" }) : Ok(p);
    }

    // ------------------------ CREACIÓN MANUAL (JSON) ------------------------
    // POST /api/properties  -> body JSON con todos los campos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePropertyRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (request.PriceProperty < 0)
        {
            ModelState.AddModelError(nameof(request.PriceProperty), "PriceProperty must be >= 0");
            return ValidationProblem(ModelState);
        }

        var created = await _repo.CreateAsync(request, ct);
        return Created($"{Request.Path}", created);
    }

    // ------------------------ CREACIÓN POR CAMPOS (FORM) --------------------
    // POST /api/properties/form -> application/x-www-form-urlencoded o multipart/form-data
    // Campos: IdOwner, Name, AddressProperty, PriceProperty, Image
    [HttpPost("form")]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public async Task<IActionResult> CreateFromForm(
        [FromForm] string IdOwner,
        [FromForm] string Name,
        [FromForm] string AddressProperty,
        [FromForm] decimal PriceProperty,
        [FromForm] string Image,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(IdOwner)) ModelState.AddModelError(nameof(IdOwner), "Required");
        if (string.IsNullOrWhiteSpace(Name)) ModelState.AddModelError(nameof(Name), "Required");
        if (string.IsNullOrWhiteSpace(AddressProperty)) ModelState.AddModelError(nameof(AddressProperty), "Required");
        if (PriceProperty < 0) ModelState.AddModelError(nameof(PriceProperty), "PriceProperty must be >= 0");
        if (string.IsNullOrWhiteSpace(Image)) ModelState.AddModelError(nameof(Image), "Required");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var req = new CreatePropertyRequest
        {
            IdOwner = IdOwner,
            Name = Name,
            AddressProperty = AddressProperty,
            PriceProperty = PriceProperty,
            Image = Image
        };

        var created = await _repo.CreateAsync(req, ct);
        return Created($"{Request.Path}", created);
    }

    // ------------------------ CREACIÓN AUTOMÁTICA ---------------------------
    // POST /api/properties/auto?count=N  -> genera N propiedades (1..50)
    [HttpPost("auto")]
    public async Task<IActionResult> CreateAuto([FromQuery] int count = 1, CancellationToken ct = default)
    {
        var items = await _repo.CreateAutoAsync(count, ct);
        return Created($"{Request.Path}", new { inserted = items.Count, items });
    }

    // ------------------------ SEED -----------------------------------------
    // POST /api/properties/seed  -> poblar con 3 registros demo (si está vacía)
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var inserted = await _repo.SeedAsync(ct);
        return Ok(new { inserted });
    }
}
