using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application;
using Shared.Application.Dto;
using Shared.Domain.Types;
using WAPIDocument.Application.Dto.Document;
using WAPIDocument.Application.Services;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/documents")]
    [Produces("application/json")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        
        [HttpPost]
        [Authorize(Roles = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(typeof(DocumentCreateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] DocumentCreateRequest model, 
            CancellationToken cancellationToken)
        {
            DocumentCreateResponse result = await _documentService.CreateAsync(model, cancellationToken);
            return Ok(result);
        }
        
        [HttpGet("{id}")]
        [Authorize(Roles = $"{nameof(RoleType.Viewer)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(typeof(DocumentReadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAsync(
            [FromRoute] string id, 
            CancellationToken cancellationToken)
        {
            DocumentReadResponse? result = await _documentService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }
        
        [HttpGet]
        [AutoValidationActionFilter]
        [Authorize(Roles = $"{nameof(RoleType.Viewer)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(typeof(PageDto<DocumentReadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindPagedByFilterAsync(
            [FromQuery] DocumentFindPagedByFilterRequest model, 
            CancellationToken cancellationToken)
        {
            PageDto<DocumentReadResponse> result = await _documentService.FindPagedByFilterAsync(model, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(typeof(DocumentUpdateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] string id, 
            [FromBody] DocumentUpdateRequest model, 
            CancellationToken cancellationToken)
        {
            var result = await _documentService.UpdateAsync(
                id, 
                model, 
                cancellationToken);
            
            return Ok(result);
        }
        
        [HttpPut("{id}/status")]
        [Authorize(Roles = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(typeof(DocumentUpdateStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatusAsync(
            [FromRoute] string id, 
            [FromBody] DocumentStatus newStatus, 
            CancellationToken cancellationToken)
        {
            var result = await _documentService.UpdateStatusAsync(
                id, 
                newStatus, 
                cancellationToken);
            
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] string id,
            CancellationToken cancellationToken)
        {
            await _documentService.DeleteByIdAsync(
                id, 
                cancellationToken);
            
            return NoContent();
        }
        
        [HttpPost("{id}/generation")]
        [Authorize(Roles = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(typeof(DocumentGenerateFromResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateAsync(
            [FromRoute] string id,
            [FromBody] DocumentGenerateFromRequest model, 
            CancellationToken cancellationToken)
        {
            DocumentGenerateFromResponse result = await _documentService.GenerateFromAsync(id, model, cancellationToken);
            return Ok(result);
        }
        
        [HttpPost("{id}/attachments")]
        [Authorize(Roles = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AttachAsync(
            [FromRoute] string id,
            [FromBody] DocumentAttachRequest model, 
            CancellationToken cancellationToken)
        {
            await _documentService.AttachAsync(id, model, cancellationToken);
            return NoContent();
        }
    }
}
