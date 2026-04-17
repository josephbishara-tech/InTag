using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InTagLogicLayer.Asset;
using InTagViewModelLayer.Asset;

namespace InTagWeb.Controllers.Api
{
    [Route("api/v1/tracking")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TrackingApiController : ControllerBase
    {
        private readonly IAssetTrackingService _trackingService;

        public TrackingApiController(IAssetTrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests()
            => Ok(await _trackingService.GetRequestsAsync());

        [HttpGet("requests/{id}")]
        public async Task<IActionResult> GetRequest(int id)
        {
            try { return Ok(await _trackingService.GetRequestByIdAsync(id)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost("requests/{id}/start")]
        public async Task<IActionResult> StartRequest(int id)
        {
            try { return Ok(await _trackingService.OpenRequestAsync(id)); }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("scan")]
        public async Task<IActionResult> SubmitScan([FromBody] TrackingScanSubmitVm model)
        {
            try { return Ok(await _trackingService.SubmitScanAsync(model)); }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("requests/{id}/complete")]
        public async Task<IActionResult> CompleteRequest(int id)
        {
            try { return Ok(await _trackingService.CompleteRequestAsync(id)); }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("lines/{lineId}/missing")]
        public async Task<IActionResult> MarkMissing(int lineId)
        {
            try { await _trackingService.MarkLineMissingAsync(lineId); return Ok(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        /// <summary>
        /// Scan barcode → find asset → return matching tracking line from a request
        /// </summary>
        [HttpGet("requests/{requestId}/find-by-scan")]
        public async Task<IActionResult> FindByBarcode(int requestId, [FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { error = "Scan code is required." });

            try
            {
                var result = await _trackingService.FindLineByScannedCodeAsync(requestId, code);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
