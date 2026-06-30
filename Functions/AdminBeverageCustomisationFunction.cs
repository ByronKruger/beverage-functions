using Coffeeg.Dtos.BeverageCustomisation;
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
    public class AdminBeverageCustomisationFunction(IAdminBeverageManagementService Service)
    {
        [Function("Add-Beverage-Type")]
        
        public async Task<IActionResult> CreateBeverageType(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beverage-admin/add-beverage-type")] 
            [Microsoft.Azure.Functions.Worker.Http.FromBody] AddBeverageType dto)
        {
            // 1.
            var result = await Service.CreateBeverageType(dto);

            if (result.IsSuccess)
            {
                return new OkObjectResult("");
            }
            else
            {
                return new BadRequestObjectResult(new { error = result.ErrorMessage });
            }

            // 2.
            //var result = await Service.CreateBeverageType(description);

            //if (result.IsSuccess)
            //    return CreatedAtAction(
            //    nameof(GetBeverageType),           // ← prefer a real GET action name
            //    new { id = result.Value.Id },      // route values (must match route template of the GET action)
            //    result.Value                       // response body (usually the created DTO)
            //);
            //else
            //    return BadRequest(new { error = result.ErrorMessage });
        }

        [Function("Add-Ingredients")]
        public async Task<IActionResult> CreateIngredients(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "beverage-admin/add-ingredients")]
            [Microsoft.Azure.Functions.Worker.Http.FromBody] AddIngredients dto)
        {
            var result = await Service.CreateIngredients(dto);

            if (result.IsSuccess)
            {
                return new OkObjectResult("");
            }
            else
            {
                return new BadRequestObjectResult(new { error = result.ErrorMessage });
            }
        }
    }
}
