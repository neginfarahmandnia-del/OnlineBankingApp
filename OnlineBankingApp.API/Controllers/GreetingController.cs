using Microsoft.AspNetCore.Mvc;
using OnlineBankingApp.Application.Interfaces;
using OnlineBankingApp.Domain.Models;

namespace OnlineBankingApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreetingController : ControllerBase
{
    private readonly IGreetingService _greetingService;

    public GreetingController(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    [HttpGet("{name}")]
    public ActionResult<Greeting> GetGreeting(string name)
    {
        var result = _greetingService.GetGreeting(name);
        return Ok(result);
    }
}
