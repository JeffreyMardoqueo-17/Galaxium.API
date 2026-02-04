using AutoMapper;
using Galaxium.Api.DTOs.StockEntry;
using Galaxium.Api.Entities;
using Galaxium.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galaxium.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockEntryController : ControllerBase
    {
        private readonly IStockEntryService _stockEntryService;
        private readonly IMapper _mapper;

        public StockEntryController(
            IStockEntryService stockEntryService,
            IMapper mapper)
        {
            _stockEntryService = stockEntryService;
            _mapper = mapper;
        }

        // ===============================
        // POST: api/StockEntry
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<StockEntryResponseDTO>> CreateStockEntry(
            [FromBody] StockEntryCreateDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockEntryEntity = _mapper.Map<StockEntry>(request);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            if (!int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user id.");

            stockEntryEntity.UserId = userId;

            var createdEntry = await _stockEntryService
                .CreateStockEntryAsync(stockEntryEntity);

            var response = _mapper.Map<StockEntryResponseDTO>(createdEntry);

            return CreatedAtAction(
                nameof(GetStockEntryById),
                new { id = response.Id },
                response
            );
        }

        // ===============================
        // GET: api/StockEntry/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StockEntryResponseDTO>> GetStockEntryById(int id)
        {
            var entry = await _stockEntryService.GetByIdStockEntryAsync(id);

            if (entry == null)
                return NotFound();

            return Ok(_mapper.Map<StockEntryResponseDTO>(entry));
        }

        // ===============================
        // GET: api/StockEntry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockEntryResponseDTO>>> GetAllStockEntries()
        {
            var entries = await _stockEntryService.GetStockEntriesAsync();

            var response = _mapper.Map<IEnumerable<StockEntryResponseDTO>>(entries);

            return Ok(response);
        }
    }
}
