namespace Propulsion.AspNetCore.CommandService
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class CommandServiceMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string route;
        private readonly CommandDispatcher dispatcher;
        
        public CommandServiceMiddleware(RequestDelegate next, string route)
        {
            this.next = next;
            this.route = route;
            dispatcher = new CommandDispatcher(route);
        }

        public Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(route))
                return dispatcher.DispatchAsync(context); // Terminal for route.
            
            return next(context);
        }
    }

    public static class CommandServiceBuilderExtensions
    {
        public static IApplicationBuilder UseCommandService(this IApplicationBuilder app, string route)
        {
            return app.UseMiddleware<CommandServiceMiddleware>(route);
        }
    }
}
