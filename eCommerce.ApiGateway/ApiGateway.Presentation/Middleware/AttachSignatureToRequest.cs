namespace ApiGateway.Presentation.Middleware
{
    public class AttachSignatureToRequest(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //Attach specific header to the request 
            context.Request.Headers["Api-Gateway"]= "Signed";
            await next(context);
        }
    }
}
