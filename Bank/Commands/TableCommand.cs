using LightPath.Bank.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;

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
            var json = JsonConvert.SerializeObject(_cache);
            var res = new BankEmbeddedResource()
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
