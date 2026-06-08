using Coffeeg.Dtos.User;
using Coffeeg.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TestFunctionApp
{
    public class UserFunction(IUserManagementService Service)
    {
        //private readonly ILogger<UserFunction> _logger;

        //public UserFunction(ILogger<UserFunction> logger)
        //{                                                                                                    
        //    _logger = logger;
        //}

        //[Function("Function1")]
        //public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        //{
        //    _logger.LogInformation("C# HTTP trigger function processed a request.");
        //    return new OkObjectResult("Welcome to Azure Functions!123");
        //}

        [Function("Register")]
        public async Task<IActionResult> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/user/register")]
            HttpRequest req)//,
            //[FromBody] RegisterUser user)
        {
            RegisterUser? user = null;
            try
            {
                user = await req.ReadFromJsonAsync<RegisterUser>();
            }
            catch (Exception ex)
            {
                // Log the deserialization error
                return new BadRequestObjectResult($"Invalid JSON: {ex.Message}");
            }

            var result = await Service.CreateUserAsync(user);

            if (!result.IsSuccess) return new BadRequestObjectResult("Could not register");

            return new OkObjectResult(result.Value);
        }
    }
}
