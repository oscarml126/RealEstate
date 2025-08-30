using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using RealEstate.Api.Controllers;
using RealEstate.Application.Abstractions;
using RealEstate.Application.Dtos;

namespace RealEstate.Tests.Controllers;

[TestFixture]
public class PropertiesControllerTests
{
    [Test]
    public async Task Get_ReturnsOkWithItems()
    {
        var mock = new Mock<IPropertyRepository>();
        mock.Setup(r => r.FindAsync(null, null, null, null, 1, 12, default))
            .ReturnsAsync(new PagedResult<PropertyDto>(
                new List<PropertyDto> {
                    new("own-1","Depto","Calle 1",100,"img")
                }, 1, 1, 12));

        var controller = new PropertiesController(mock.Object);
        var result = await controller.Get(null, null, null, null, 1, 12, default) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetById_NotFound_WhenMissing()
    {
        var mock = new Mock<IPropertyRepository>();
        mock.Setup(r => r.GetByIdAsync("badid", default)).ReturnsAsync((PropertyDto?)null);

        var controller = new PropertiesController(mock.Object);
        var result = await controller.GetById("badid", default);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
