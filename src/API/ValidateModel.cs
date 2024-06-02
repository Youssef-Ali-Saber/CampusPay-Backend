using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API;

public class ValidateModel: ActionFilterAttribute
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		if (!context.ModelState.IsValid)
		{
			context.Result = new BadRequestObjectResult(context.ModelState);
		}
	}
}
	
	
