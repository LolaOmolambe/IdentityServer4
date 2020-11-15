using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Identity.ConsoleClient
{
    class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
        private static async Task MainAsync()
        {
            //get all the endpoints of identity server
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
            if(disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            //Grab bearer token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "IdentityApi"
            });

            if(tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            //Consume User Api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var customerInfo = new StringContent(
                JsonConvert.SerializeObject(
                    new { Id = 1, FirstName = "Lola", LastName = "Omolambeee" }
                ), Encoding.UTF8, "application/json");

            var createUser = await apiClient.PostAsync("http://localhost:53409/api/users", customerInfo);

            if(createUser.IsSuccessStatusCode)
            {
                Console.WriteLine(createUser.StatusCode);
            }

            var getUser = await apiClient.GetAsync("http://localhost:53409/api/users"); 
            if(!getUser.IsSuccessStatusCode)
            {
                Console.WriteLine(getUser.StatusCode);
            }
            else
            {
                var content = await getUser.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
            Console.Read();
        }
    }
}
