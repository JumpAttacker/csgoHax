using System.Collections.Generic;
using Newtonsoft.Json;

namespace CsgoHaxOverlay.JsonOffsets
{
    public partial class GitHubOffsets
    {
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("signatures")]
        public Dictionary<string, int> Signatures { get; set; }

        [JsonProperty("netvars")]
        public Dictionary<string, int> Netvars { get; set; }
    }
}