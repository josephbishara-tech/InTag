using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Document;
using InTagViewModelLayer.Document;
using InTagWeb.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InTagWeb.Controllers.Api
{
    [ApiController]
    [Route("api/v1/documents")]
    [Authorize]
    [RequireModule(PlatformModule.Document)]
    public class DocumentsApiController : ControllerBase
    {
        private readonly IDocumentService _docService;

        public DocumentsApiController(IDocumentService docService)
        {
            _docService = docService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DocumentFilterVm filter)
        {
            var result = await _docService.GetAllAsync(filter);
            return Ok(new { data = result.Items, meta = new { result.TotalCount, result.Page, result.PageSize, result.TotalPages } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(new { data = await _docService.GetByIdAsync(id) }); }
            catch (KeyNotFoundException) { return NotFound(new { errors = new[] { "Document not found." } }); }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DocumentCreateVm model)
        {
            try
            {
                var doc = await _docService.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = doc.Id }, new { data = doc });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            try { return Ok(new { data = await _docService.CheckOutAsync(id) }); }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id, [FromBody] RevisionCreateVm model)
        {
            try
            {
                model.DocumentId = id;
                return Ok(new { data = await _docService.CheckInAsync(id, model) });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPost("{id}/revisions")]
        public async Task<IActionResult> CreateRevision(int id, [FromBody] RevisionCreateVm model)
        {
            try
            {
                model.DocumentId = id;
                return Ok(new { data = await _docService.CreateRevisionAsync(model) });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPost("revisions/{revisionId}/approve")]
        public async Task<IActionResult> Approve(int revisionId, [FromBody] RevisionApprovalVm model)
        {
            try
            {
                model.RevisionId = revisionId;
                return Ok(new { data = await _docService.ApproveRevisionAsync(model) });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPost("{id}/publish")]
        public async Task<IActionResult> Publish(int id)
        {
            try { return Ok(new { data = await _docService.PublishAsync(id) }); }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpGet("due-for-review")]
        public async Task<IActionResult> DueForReview()
        {
            var docs = await _docService.GetDueForReviewAsync();
            return Ok(new { data = docs });
        }
    }
}
