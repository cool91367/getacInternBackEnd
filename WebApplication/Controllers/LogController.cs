using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly string todayLogFile = "bin/Debug/netcoreapp2.2/logs/customLog/controller-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

        [AllowAnonymous]
        [HttpGet("UserTrack")]
        public IActionResult UserTrack()
        {
            try
            {
                using (StreamReader sr = new StreamReader(todayLogFile))
                {
                    // Read the stream to a string, and write the string to the console.
                    String log = sr.ReadToEnd();
                    return Ok(log);
                }
            }
            catch (IOException e)
            {
                return BadRequest("The file could not be read: " + e.Message);
            }
        }
    }
}
