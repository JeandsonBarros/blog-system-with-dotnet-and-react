using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class PageResponse<T>
    {
        public PageResponse(T data, int page, int size, int totalRecords)
        {
            Page = page;
            Size = size;
            TotalRecords = totalRecords;
            Data = data;
            TotalPages = totalRecords / size == 0 ? 1 : totalRecords / size;
            
        }
       
        public T Data { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}