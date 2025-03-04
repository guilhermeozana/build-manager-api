using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DataController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetData()
        {
            var data = new[]
            {
                new
                {
                    id = 1,
                    week = new[]
                    {
                        new { day = "Mon", SucceedValue = 35, FailValue = 10 },
                        new { day = "Tue", SucceedValue = 70, FailValue = 15 },
                        new { day = "Wed", SucceedValue = 35, FailValue = 35 },
                        new { day = "Thu", SucceedValue = 50, FailValue = 20 },
                        new { day = "Fri", SucceedValue = 60, FailValue = 35 },
                        new { day = "Sat", SucceedValue = 20, FailValue = 10 },
                        new { day = "Sun", SucceedValue = 20, FailValue = 10 }
                    },
                    month = new[]
                    {
                        new { day = 1, SucceedValue = 10, FailValue = 15 },
                        new { day = 2, SucceedValue = 20, FailValue = 20 },
                        new { day = 3, SucceedValue = 55, FailValue = 10 },
                        new { day = 4, SucceedValue = 25, FailValue = 18 },
                        new { day = 5, SucceedValue = 55, FailValue = 30 },
                        new { day = 6, SucceedValue = 45, FailValue = 22 },
                        new { day = 7, SucceedValue = 35, FailValue = 10 },
                        new { day = 8, SucceedValue = 70, FailValue = 15 },
                        new { day = 9, SucceedValue = 35, FailValue = 35 },
                        new { day = 10, SucceedValue = 50, FailValue = 20 },
                        new { day = 11, SucceedValue = 60, FailValue = 35 },
                        new { day = 12, SucceedValue = 20, FailValue = 10 },
                        new { day = 13, SucceedValue = 20, FailValue = 10 },
                        new { day = 14, SucceedValue = 40, FailValue = 18 },
                        new { day = 15, SucceedValue = 100, FailValue = 25 },
                        new { day = 16, SucceedValue = 100, FailValue = 15 },
                        new { day = 17, SucceedValue = 100, FailValue = 20 },
                        new { day = 18, SucceedValue = 60, FailValue = 28 },
                        new { day = 19, SucceedValue = 100, FailValue = 12 },
                        new { day = 20, SucceedValue = 35, FailValue = 15 },
                        new { day = 21, SucceedValue = 50, FailValue = 22 },
                        new { day = 22, SucceedValue = 65, FailValue = 30 },
                        new { day = 23, SucceedValue = 45, FailValue = 18 },
                        new { day = 24, SucceedValue = 30, FailValue = 15 },
                        new { day = 25, SucceedValue = 20, FailValue = 10 },
                        new { day = 26, SucceedValue = 35, FailValue = 15 },
                        new { day = 27, SucceedValue = 55, FailValue = 25 },
                        new { day = 28, SucceedValue = 40, FailValue = 20 },
                        new { day = 29, SucceedValue = 25, FailValue = 12 },
                        new { day = 30, SucceedValue = 55, FailValue = 22 }
                    }
                }
            };

            return Ok(data);
        }
    }
}
