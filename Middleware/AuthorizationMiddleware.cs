using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestFunctionApp.Extensions;
using static TestFunctionApp.Middleware.AuthenticationMiddleware;

namespace TestFunctionApp.Middleware
{
    public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AuthorizationMiddleware> _logger;

        public AuthorizationMiddleware(IAuthorizationService authorizationService, ILogger<AuthorizationMiddleware> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (IsAnonymous(context))
            {
                _logger.LogDebug("Anonymous endpoint - skipping auth");
                await next(context);
                return;
            }

            var principalFeature = context.Features.Get<JwtPrincipalFeature>();
            if (principalFeature == null)
            {
                await next(context);
                return;
            }

            // Example: look for a custom attribute or default policy
            var targetMethod = GetTargetMethod(context);
            var authorizeAttr = targetMethod?.GetCustomAttribute<AuthorizeAttribute>();

            if (authorizeAttr != null)
            {
                var policyName = authorizeAttr.Policy ?? "Default"; // or handle Roles directly
                var result = await _authorizationService.AuthorizeAsync(
                    principalFeature.Principal, policyName);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Authorization failed for policy {Policy}", policyName);
                    context.SetHttpResponseStatusCode(HttpStatusCode.Forbidden);
                    return;
                }
            }

            await next(context);
        }

        private static bool IsAnonymous(FunctionContext context)
        {
            var method = GetTargetMethod(context);  // Reuse the reflection helper you already have
            return method?.GetCustomAttribute<AllowAnonymousAttribute>() != null ||
                   method?.DeclaringType?.GetCustomAttribute<AllowAnonymousAttribute>() != null;
        }

        public static MethodInfo? GetTargetMethod(FunctionContext context)
        {
            // Reflection to find the actual Function method (for attribute reading)
            var entryPoint = context.FunctionDefinition.EntryPoint;
            var assembly = Assembly.LoadFrom(context.FunctionDefinition.PathToAssembly);
            var typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));
            var methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);
            var type = assembly.GetType(typeName);
            return type?.GetMethod(methodName);
        }
    }
}
