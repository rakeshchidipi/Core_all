using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Core_all.Controllers.Logging
{
    public interface IRequestLogger
    {
        void logrequestresponse(HttpContext _context, string Request, string Response);
        void setrequestid(HttpContext _context);
    }
}
