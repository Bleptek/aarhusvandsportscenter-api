using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aarhusvandsportscenter.Api.Tests.TestUtils
{
    public static class HttpExtensions
    {
        public static StringContent ToHttpStringContent(this object obj)
        {
            return new StringContent(
                JsonSerializer.Serialize(obj),
                Encoding.UTF8,
                "application/json");
        }

        public static async Task<T> DeserializeHttpResponse<T>(this HttpResponseMessage httpResponseMessage)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<T>(json, options);
            return obj;
        }
    }
}