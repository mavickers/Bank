using LightPath.Bank.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace LightPath.Bank.Commands
{
    /// <summary>
    /// Returns a table of all embedded resources.
    /// </summary>
    public class TableCommand : IBankCommand
    {
        private readonly ConcurrentDictionary<string, BankEmbeddedResource> _cache;

        public TableCommand(ConcurrentDictionary<string, BankEmbeddedResource> cache)
        {
            _cache = cache;
        }

        public BankEmbeddedResource GetResource()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Example: pretty-print JSON
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Example: camelCase properties
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // Ignore nulls
            };

            var json = JsonSerializer.Serialize(_cache, options);
            var res = new BankEmbeddedResource
            {
                Assembly = GetType().Assembly,
                NameSpace = "Commands",
                FileName = "$table",
                ContentType = "application/json",
                Contents = Encoding.UTF8.GetBytes(json)
            };

            return res;
        }
    }
}
