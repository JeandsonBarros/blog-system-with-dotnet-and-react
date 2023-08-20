using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.CustomExceptions
{
    public class NotFoundException: Exception
    {
        public NotFoundException(string message) : base(message){

        }
        public NotFoundException() : base("Not found!"){}
    }
}