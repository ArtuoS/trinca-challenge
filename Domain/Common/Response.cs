namespace Domain.Common
{
    public class Response
    {
        public Response()
        { }

        public Response(string message, bool isValid)
        {
            Message = message;
            IsValid = isValid;
        }

        public Response(string message, bool isValid, object data)
        {
            Message = message;
            IsValid = isValid;
            Data = data;
        }

        public string Message { get; set; }
        public bool IsValid { get; set; }
        public object Data { get; set; }
    }
}