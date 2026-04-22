using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Application.Dto;
using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Services;
using WAPIDocument.Controllers;
using Xunit;

namespace WAPIDocument.Api.Tests.Controllers;

public class DocumentsControllerTests
{
    private readonly Mock<IDocumentService> _service = new();

    private DocumentsController CreateSut() => new(_service.Object);

    [Fact]
    public async Task Post_Returns_Ok_With_Create_Response()
    {
        var request = new DocumentCreateRequest { Currency = "EUR" };
        var response = new DocumentCreateResponse { Id = "new-id" };
        _service.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await CreateSut().CreateAsync(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetAsync_Returns_Ok_With_Read_Response()
    {
        var expected = new DocumentReadResponse { Id = "1" };
        _service.Setup(s => s.GetByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await CreateSut().GetAsync("1", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task FindAsync_Returns_Ok_With_Page_Dto()
    {
        var page = new PageDto<DocumentReadResponse>
        {
            CurrentPage = 1,
            PageSize = 10,
            TotalItems = 0,
            TotalPages = 0,
            Items = new List<DocumentReadResponse>()
        };
        _service
            .Setup(s => s.FindPagedByFilterAsync(It.IsAny<DocumentFindPagedByFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        var result = await CreateSut().FindPagedByFilterAsync(new DocumentFindPagedByFilterRequest(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(page, ok.Value);
    }

    [Fact]
    public async Task Put_Returns_NoContent()
    {
        _service
            .Setup(s => s.UpdateAsync("1", It.IsAny<DocumentUpdateRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await CreateSut().UpdateAsync("1", new DocumentUpdateRequest(), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _service.Verify();
    }

    [Fact]
    public async Task Delete_Returns_NoContent()
    {
        _service
            .Setup(s => s.DeleteByIdAsync("1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await CreateSut().DeleteAsync("1", CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _service.Verify();
    }

    [Fact]
    public async Task GetAsync_Propagates_KeyNotFoundException()
    {
        _service
            .Setup(s => s.GetByIdAsync("x", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateSut().GetAsync("x", CancellationToken.None));
    }
}
