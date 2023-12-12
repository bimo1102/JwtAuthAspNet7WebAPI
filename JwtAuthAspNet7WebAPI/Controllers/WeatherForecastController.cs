using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        
        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            return Ok(Summaries);
        }
    }
}
