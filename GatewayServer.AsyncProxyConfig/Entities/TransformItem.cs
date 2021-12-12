using Newtonsoft.Json;

namespace GatewayServer.AsyncProxyConfig.Entities
{
    public class TransformItem
    {
        [JsonProperty("key")]
        public string Key { get; set; } = "";

        [JsonProperty("value")]
        public IReadOnlyDictionary<string, string> Value { get; set; } = new Dictionary<string, string>();
    }
}
