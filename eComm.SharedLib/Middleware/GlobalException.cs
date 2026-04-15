using eComm.SharedLib.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace eComm.SharedLib.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //Declare default variables
            string message = "Internal server error. Please try again";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "An error occurred while processing your request.";
            try
            {
                await next(context);
                //check if response is too many requests - 429 status code
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many requests made. Please try again later.";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }
                // if response is unauthorised -- 401 status code
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "You are not authorized to access this resource.";
                    statusCode = (int)StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // if response is forbidden -- 403 status code
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Forbidden";
                    message = "You do not have permission to access this resource.";
                    statusCode = (int)StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                // log original exceptions/file, debugger, console 
                LogException.LogExceptions(ex);

                // check if exception is timeout -- 408 request timeout
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Request Timeout";
                    message = "The request timed out. Try again later.";
                    statusCode = StatusCodes.Status408RequestTimeout;
                    // If Exception is Caught. 
                    // If none of the exceptions then do the default.
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // display scary-free message to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails
            {
                Detail = message,
                Status = statusCode,
                Title = title
            }), CancellationToken.None);
            return;
        }
    }
}
