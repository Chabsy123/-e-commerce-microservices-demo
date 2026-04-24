namespace eCommerce.SharedLibrary.Responses
{
    public class Response(bool flag = false, string message = null!)
    {
        public bool Flag { get; set; } = flag;
        public string Message { get; set; } = message;
    }
}
