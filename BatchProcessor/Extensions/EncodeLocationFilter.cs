using Microsoft.AspNetCore.Mvc.Filters;

namespace BatchProcessor.Extensions
{
    public class EncodeLocationFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context) { }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (context.HttpContext.Response.Headers.TryGetValue("Location", out var values))
            {
                var location = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(location) && location.Any(c => c > 127))
                {
                    context.HttpContext.Response.Headers["Location"] = Uri.EscapeUriString(location);
                }
            }
        }
    }
}
