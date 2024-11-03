using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shop.Core.Validators.Attributes
{
    public class ValidateIdAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.ContainsKey("id"))
            {
                var id = (int)context.ActionArguments["id"];
                if (id <= 0)
                {
                    context.Result = new BadRequestObjectResult("Id must be a positive number.");
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
