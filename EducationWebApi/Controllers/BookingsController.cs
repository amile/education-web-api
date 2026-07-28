using EducationWebApi.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationWebApi;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> Get(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);

        return booking;
    }
}