using Coffeeg.Interfaces.Services;
using Coffeeg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestFunctionApp.Functions
{
    public class BeverageCustomisationFunction(IBeverageCustomisationService Service)
    {
        [Function("Recent-Beverage-Customisations")]
        public async Task<IActionResult> RecentBeverageCustomisations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beverage-customisation/recent-beverage-customisations")]
            HttpRequestData req)
        {
            return new OkObjectResult(await Service.GetRecentBeverageCustomisations());
        }
    }
}
