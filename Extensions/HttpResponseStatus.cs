using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static TestFunctionApp.Middleware.AuthenticationMiddleware;

namespace TestFunctionApp.Extensions
{
    public static class HttpResponseStatus
    {

        public static void SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode)
        {
            // Uses internal types — acceptable for now; monitor SDK changes
            var coreAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Core");
            var featureType = coreAssembly.GetType("Microsoft.Azure.Functions.Worker.Context.Features.IFunctionBindingsFeature");
            var bindingsFeature = context.Features.Single(f => f.Key == featureType).Value;
            var resultProp = featureType.GetProperty("InvocationResult");

            var grpcAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Grpc");
            var responseType = grpcAssembly.GetType("Microsoft.Azure.Functions.Worker.GrpcHttpResponseData");
            var response = Activator.CreateInstance(responseType, context, statusCode);

            resultProp!.SetValue(bindingsFeature, response);
        }

        //public static ClaimsPrincipal? GetUserPrincipal(this HttpRequest req)
        //{
        //    // If you enhance middleware to also set req.HttpContext.User = principalFeature.Principal;
        //    return req.HttpContext.User.Identity?.IsAuthenticated == true ? req.HttpContext.User : null;
        //}

        public static ClaimsPrincipal? GetUserPrincipal(this FunctionContext context)
        {
            return context.Features.Get<JwtPrincipalFeature>()?.Principal;
        }

        public static ClaimsPrincipal? GetUserPrincipal(this HttpRequest req)
        {
            // Works if we successfully set it on the HttpContext (best effort)
            return req.HttpContext.User.Identity?.IsAuthenticated == true
                ? req.HttpContext.User
                : null;
        }
    }
}
