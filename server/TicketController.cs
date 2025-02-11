using Microsoft.AspNetCore.Mvc;

namespace server; 
[ApiController] 
[Route("api/tickets")]

public class TicketController:ControllerBase
{
    
    [HttpPost("submit")]
    public IActionResult SubmitTicket([FromBody] Ticket ticket)
    {
        try
        {
            // Simulate processing
            if (ticket == null)
            {
                return BadRequest(new { message = "Invalid ticket data." });
            }
            
            
            Console.WriteLine($"Ticket submitted: {ticket.Email} - {ticket.Issue} - {ticket.SelectedOption}");
            return Ok(new { message = "Issue submitted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Something went wrong.", error = ex.Message });
        }
    }
}