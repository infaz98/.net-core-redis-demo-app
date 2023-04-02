using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RedisNetCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _distributedCache;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        [Route("GetWeatherForecast/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var jsonData = await _distributedCache.GetStringAsync(id.ToString());

                if (jsonData is null)
                {
                    return NoContent();
                }
                return Ok(JsonSerializer.Deserialize<WeatherForecast>(jsonData));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }


        [HttpPost]
        [Route("SaveWeatherForecast")]
        public async Task<IActionResult> Post([FromBody] WeatherForecast weatherForecast)
        {

            try
            {
                var options = new DistributedCacheEntryOptions();

                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120);
                options.SlidingExpiration = TimeSpan.FromSeconds(120);
                var jsonData = JsonSerializer.Serialize(weatherForecast);
                await _distributedCache.SetStringAsync(weatherForecast.Id.ToString(), jsonData, options);
                return CreatedAtAction(actionName: nameof(Get), routeValues: new { id = weatherForecast.Id }, value: weatherForecast);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete]
        [Route("DeleteWeatherForecast/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _distributedCache.RemoveAsync(id.ToString());
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}