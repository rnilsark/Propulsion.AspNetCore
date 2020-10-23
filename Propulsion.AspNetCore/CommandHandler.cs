namespace Propulsion.AspNetCore
{
    using MassTransit;
    using System.Threading.Tasks;

    public class CommandHandler : IConsumer<Command>
    {
        public Task Consume(ConsumeContext<Command> context)
        {
            return Task.CompletedTask;
        }
    }
}
