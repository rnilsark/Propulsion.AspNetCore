namespace Propulsion.AspNetCore.CommandService
{
    using MassTransit.Mediator;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;

    internal class CommandDispatcher
    {
        private readonly string discriminator;

        public CommandDispatcher(string route)
        {
            discriminator = route.Split('/').Last();
        }

        public async Task DispatchAsync(HttpContext context)
        {
            string GetCommandTypeSegment(HttpRequest r) => r.Path.Value.Split('/').ToList().SkipWhile(segment => segment != discriminator).Skip(1).Single();

            var typeName = GetCommandTypeSegment(context.Request);
            if (TryGetTypeCached(typeName, out Type commandType))
            {
                var cmd = await JsonSerializer.DeserializeAsync(context.Request.Body, commandType);

                var mediator = context.RequestServices.GetRequiredService<IMediator>();
                await mediator.Send(cmd, commandType);

                context.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }

        private static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();

        private static bool TryGetTypeCached(string typeName, out Type cachedType)
        {
            cachedType = TypeCache.GetOrAdd(typeName, value => {
                TryGetType(typeName, out Type type);
                return type;
            });

            return cachedType != null;
        }

        private static bool TryGetType(string typeName, out Type t)
        {
            var referenceAssemblies = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load).ToList();
            var assemblies = referenceAssemblies != null && referenceAssemblies.Any() ? AppDomain.CurrentDomain.GetAssemblies().Union(referenceAssemblies) : AppDomain.CurrentDomain.GetAssemblies();

            t = assemblies.Select(assembly => assembly.GetType(typeName)).FirstOrDefault(type => type != null);
            return t != null;
        }
    }
}
