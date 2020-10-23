namespace Propulsion.AspNetCore.Tests
{
    using MassTransit;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    [TestClass]
    public class Sending_command
    {
        private readonly HttpClient client;
        private readonly TestServer testServer;

        public Sending_command()
        {
            testServer = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureTestServices(services =>
                {
                    services.AddMassTransit(x => x.UsingInMemory());
                }));

            client = testServer.CreateClient();
        }

        [TestMethod]
        public async Task Should_result_in_successful_statusCode()
        {
            var response = await client.PostAsync($"/api/commands/{typeof(Command).FullName}", CreateHttpContent(new Command()));
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        private static HttpContent CreateHttpContent(object command)
        {
            var commandString = JsonSerializer.Serialize(command);
            return new StringContent(commandString, Encoding.UTF8, "application/json"); ;
        }

        [TestCleanup]
        public void CleanUp()
        {
            testServer.Dispose();
        }
    }
}
