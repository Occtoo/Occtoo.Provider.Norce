using System;

namespace Occtoo.Provider.Norce.Model
{
    public class LogMessageModel
    {
        public LogMessageModel(string message, string stackTrace, bool isError)
        {
            DateTime currentTime = DateTime.Now;

            Message = message;
            StackTrace = stackTrace;
            IsError = isError;
            DateTimeString = DateTime.Now.ToString();
        }

        public string DateTimeString { get; set; }
        public string StackTrace { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }

}
