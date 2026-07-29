//using Coffeeg.Dtos;
//using Coffeeg.Interfaces.Services;
//using Coffeeg.Services;
using Coffeeg.Dtos;
using Coffeeg.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TestFunctionApp.Extensions;
using static TestFunctionApp.Middleware.AuthenticationMiddleware;

namespace TestFunctionApp.Functions
{
    public class BeverageCustomisationFunction(IBeverageCustomisationService Service, ILogger<BeverageCustomisationFunction> logger)
    {
        [Function("Recent-Beverage-Customisations")]
        public async Task<IActionResult> RecentBeverageCustomisations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beverage-customisation/recent-beverage-customisations")]
            HttpRequestData req)
        {
            return new OkObjectResult(await Service.GetRecentBeverageCustomisations());
        }

        [Function("Add-Beverage-Customisation")]
        public async Task<IActionResult> CreateBeverageCustomisation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beverage-customisation/add-customisation")]
            [Microsoft.Azure.Functions.Worker.Http.FromBody] CreateBeverageCustomisationDto dto,
            Microsoft.AspNetCore.Http.HttpRequest req,
            FunctionContext context)
        {
            Console.WriteLine("============================================== CONSOLE: FUNCTION STARTED =====");
            System.Diagnostics.Trace.WriteLine("==============================================  TRACE: FUNCTION STARTED =====");
            logger.LogError("==============================================  ILOGGER: FUNCTION STARTED =====");

            logger.LogError("==============================================  FUNCTION STARTED === UserId will be extracted next");
            //var principal = req.Identities?.FirstOrDefault(); // ClaimsIdentity collection
            // Or build ClaimsPrincipal
            //if (principal != null && principal.IsAuthenticated)
            //{
            //    // Use it
            //}
            var user = req.HttpContext.User;

            //var result = await Service.CreateBeverageCustomisation(dto);
            //return result.IsSuccess ? new OkObjectResult(result.Value) : new BadRequestObjectResult(result.ErrorMessage);
            //return new BadRequestObjectResult("");


            // Direct feature lookup for debugging
            var feature = context.Features.Get<JwtPrincipalFeature>();
            var principal = feature?.Principal ?? context.GetUserPrincipal();

            //logger.LogInformation("Inside function - Feature present: {HasFeature}, Principal null: {IsNull}",
            //    feature != null, principal == null);

            if (principal == null)
            {
                logger.LogWarning("No principal found in function");
                return new UnauthorizedResult();
            }

            var userId = principal.FindFirst("oid")?.Value
              ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? principal.FindFirst("sub")?.Value;

            logger.LogError("Extracted UserId = '{UserId}' (Length = {Len})",
                userId, userId?.Length ?? 0);

            //return new OkObjectResult(new { message = $"Hello {userId}, you have access." });

            //dto.UserId = userId;
            var dto2 = dto with { UserId = userId };

            logger.LogError("About to call Service.CreateBeverageCustomisation");

            var result = await Service.CreateBeverageCustomisation(dto2);

            logger.LogError("Service returned. Success = {Success}", result.IsSuccess);

            return result.IsSuccess ? new OkObjectResult(result.Value) : new BadRequestObjectResult(result.ErrorMessage);
        }

        [Function("Get-Beverage-Types")]
        public async Task<IActionResult> GetBeverageTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beverage-customisation/beverage-types")]
            HttpRequestData req)
        {
            var result = await Service.GetBeverageTypesAsync();
            return new OkObjectResult(result.Value);
        }

        [Function("Get-User-Customisations")]
        public async Task<IActionResult> GetUserBeverageCustomisations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "beverage-customisation/customisations")]
            HttpRequestData req)
        {
            var userId = req.Query.Get("userId");

            var result = await Service.GetUserBeverageCustomisations(userId);

            return new OkObjectResult(result.Value);
        }
    }
}
