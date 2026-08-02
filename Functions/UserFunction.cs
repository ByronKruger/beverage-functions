using Coffeeg.Dtos.User;
using Coffeeg.Entities;
using Coffeeg.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using TestFunctionApp.Helpers;

namespace TestFunctionApp.Functions
{
    public class UserFunction(IUserManagementService Service, IBeverageCustomisationService BService)
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

        [Function("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/login")]
            [Microsoft.Azure.Functions.Worker.Http.FromBody] LogInUser user)
        {
            var result = await Service.CheckUserPasswordAsync(user);

            return result.IsSuccess ? new OkObjectResult(result.Value) : new BadRequestObjectResult(result.ErrorMessage);
        }

        [Function("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/register")]
            //HttpRequest req)//,
            [Microsoft.Azure.Functions.Worker.Http.FromBody] RegisterUser user)
        {
            //RegisterUser? user = null;
            //try
            //{
                //user = await req.ReadFromJsonAsync<RegisterUser>();
            //}
            //catch (Exception ex)
            //{
                // Log the deserialization error
                //return new BadRequestObjectResult($"Invalid JSON: {ex.Message}");
            //}

            var result = await Service.CreateUserAsync(user);

            if (!result.IsSuccess) return new BadRequestObjectResult("Could not register");

            return new OkObjectResult(result.Value);
        }

        [Function("Get-Users")]
        public async Task<ActionResult<List<User>>> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/users")] 
            HttpRequestData req)
        {

            var name = req.Query.Get("name");

            var userSearchResult = await BService.GetUserByNames(name);

            return userSearchResult.IsSuccess ? 
                new OkObjectResult(userSearchResult.Value) : 
                new NotFoundObjectResult(userSearchResult.ErrorMessage);
        }
    }
}
