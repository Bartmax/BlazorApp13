using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlazorApp13.Server.ViewModels.Authorization
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}