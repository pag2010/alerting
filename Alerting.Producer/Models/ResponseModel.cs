using System;

namespace Alerting.Producer.Models
{
    public class ResponseModel
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public object Data { get; set; }

        public ResponseModel() { }
        public ResponseModel(object data)
        {
            Data = data;
        }
        public ResponseModel(string message)
        {
            Success = false;
            Message = message;
        }
    }
}
