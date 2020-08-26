using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

namespace BlazorApp13.Server.Pages
{
    public class LogoutModel : PageModel
    {
        public IEnumerable<KeyValuePair<string, StringValues>> Parameters { get; private set; }

        public void OnGet()
        {
            Parameters = HttpContext.Request.HasFormContentType ? (IEnumerable<KeyValuePair<string, StringValues>>)HttpContext.Request.Form : HttpContext.Request.Query;
        }
    }
}
