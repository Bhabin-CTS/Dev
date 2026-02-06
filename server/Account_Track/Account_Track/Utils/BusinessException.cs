namespace Account_Track.Utils
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; } 

        public BusinessException(string code, string message)
            : base(message)
        {
            ErrorCode = code;
        }
    }
}
