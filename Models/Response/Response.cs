using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class Response
    {
        public Response() { }
        public Response(string message, bool success)
        {
            Message = message;
            Success = success;
        }
        public Response(string message, bool success, string details)
        {
            Message = message;
            Success = success;
            Details = details;
        }

        public string? Message { get; set; }
        public string? Details { get; set; }
        public bool Success { get; set; } = true;
        public DateTime Date { get; } = DateTime.Now;
    }

    public class Response<T> : Response
    {
        public Response() { }
        public Response(T data) { Data = data; }
        public Response(T data, string message, bool success) : base(message, success)
        {
            Data = data;
        }
        public Response(T data, string message, bool success, string details) : base(message, success, details)
        {
            Data = data;
        }

        public T Data { get; set; }

    }

}