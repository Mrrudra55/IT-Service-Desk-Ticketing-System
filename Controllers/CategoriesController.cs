using ITServiceDesk.DTOs.Categories;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public CategoriesController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Retrieves active ticket categories (Hardware, Software, Network, etc.) for selection dropdowns.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetCategories()
    {
        var categories = await _ticketService.GetCategoriesAsync();
        return Ok(categories);
    }
}

