using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestFunctionApp.Extensions;
using TestFunctionApp.Helpers;

namespace TestFunctionApp.Middleware
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly TokenValidationParameters _validationParameters;

        public AuthenticationMiddleware(IConfiguration config, ILogger<AuthenticationMiddleware> logger)
        {
            _logger = logger;
            _validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                //ValidIssuer = config["Jwt:Issuer"],
                //ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]!))
            };
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // Only run for HTTP triggers
            if (!context.FunctionDefinition.InputBindings.Values.Any(b => b.Type == "httpTrigger"))
            {
                await next(context);
                return;
            }

            if (IsAnonymous(context))
            {
                _logger.LogDebug("Anonymous endpoint - skipping auth");
                await next(context);
                return;
            }

            try
            {
                var requestData = await context.GetHttpRequestDataAsync();
                if (requestData == null || !TryGetBearerToken(requestData, out var token))
                {
                    _logger.LogWarning("No valid Bearer token found in request");
                    context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                    return;
                }

                // Extra safety check
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                {
                    _logger.LogWarning("Token failed CanReadToken() check. Raw value starts with: {Start}",
                        token.Length > 20 ? token[..20] : token);
                    context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                    return;
                }

                var principal = handler.ValidateToken(token, _validationParameters, out _);

                _logger.LogInformation("JWT validated successfully for subject: {Subject}. Setting feature.",
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");

                // Store the feature
                context.Features.Set(new JwtPrincipalFeature(principal, token));

                var httpRequestData = await context.GetHttpRequestDataAsync(); // already have this in improved version
                                                                               // The real HttpContext for your function parameter might be accessible via features in some versions

                //await next(context);

                // Store for later use (best practice)
                context.Features.Set(new JwtPrincipalFeature(principal, token));

                await next(context);
            }
            catch (SecurityTokenMalformedException ex)
            {
                _logger.LogWarning(ex, "Malformed JWT token received");
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT validation failed");
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in authentication middleware");
                context.SetHttpResponseStatusCode(HttpStatusCode.InternalServerError);
            }
        }
        private static bool IsAnonymous(FunctionContext context)
        {
            var method = GetTargetMethod(context);  // Reuse the reflection helper you already have
            return method?.GetCustomAttribute<AllowAnonymousAttribute>() != null ||
                   method?.DeclaringType?.GetCustomAttribute<AllowAnonymousAttribute>() != null;
        }

        // Feature to carry the principal
        public record JwtPrincipalFeature(ClaimsPrincipal Principal, string Token);

        private static bool TryGetBearerToken(HttpRequestData requestData, out string token)
        {
            token = string.Empty;

            if (!requestData.Headers.TryGetValues("Authorization", out var authHeaders))
                return false;

            var authHeader = authHeaders.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return false;

            token = authHeader["Bearer ".Length..].Trim();
            return !string.IsNullOrWhiteSpace(token);
        }
        private static MethodInfo? GetTargetMethod(FunctionContext context)
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













I 