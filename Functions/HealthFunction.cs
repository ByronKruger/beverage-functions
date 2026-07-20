using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFunctionApp.Helpers;

namespace TestFunctionApp.Functions
{
    public class HealthFunction(IConfiguration config)
    {
        [Function("Health")]
        [AllowAnonymous]
        public async Task<IActionResult> Health(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
            HttpRequest req)
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                        ?? config["DefaultConnection"];
            return new OkObjectResult(connectionString);
        }
    }
}
