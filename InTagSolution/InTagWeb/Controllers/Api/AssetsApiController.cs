using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InTagEntitiesLayer.Enums;
using InTagLogicLayer.Asset;
using InTagViewModelLayer.Asset;
using InTagWeb.Filters;

namespace InTagWeb.Controllers.Api
{
    [ApiController]
    [Route("api/v1/assets")]
    [Authorize]
    [RequireModule(PlatformModule.Asset)]
    public class AssetsApiController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsApiController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AssetFilterVm filter)
        {
            var result = await _assetService.GetAllAsync(filter);
            return Ok(new { data = result.Items, meta = new { result.TotalCount, result.Page, result.PageSize, result.TotalPages } });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var asset = await _assetService.GetByIdAsync(id);
                return Ok(new { data = asset });
            }
            catch (KeyNotFoundException) { return NotFound(new { errors = new[] { "Asset not found." } }); }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssetCreateVm model)
        {
            try
            {
                var asset = await _assetService.CreateAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = asset.Id }, new { data = asset });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateVm model)
        {
            try
            {
                var asset = await _assetService.UpdateAsync(id, model);
                return Ok(new { data = asset });
            }
            catch (KeyNotFoundException) { return NotFound(new { errors = new[] { "Asset not found." } }); }
            catch (InvalidOperationException ex) { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _assetService.SoftDeleteAsync(id);
                return Ok(new { data = "Asset deleted." });
            }
            catch (KeyNotFoundException) { return NotFound(new { errors = new[] { "Asset not found." } }); }
            catch (InvalidOperationException ex) { return BadRequest(new { errors = new[] { ex.Message } }); }
        }

        [HttpPost("{id}/depreciate")]
        public async Task<IActionResult> Depreciate(int id, [FromQuery] int year, [FromQuery] int month, [FromQuery] decimal? units = null)
        {
            try
            {
                var result = await _assetService.RunDepreciationAsync(id, year, month, units);
                return Ok(new { data = result });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] AssetStatus status)
        {
            try
            {
                var asset = await _assetService.ChangeStatusAsync(id, status);
                return Ok(new { data = asset });
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                return BadRequest(new { errors = new[] { ex.Message } });
            }
        }
    }
}