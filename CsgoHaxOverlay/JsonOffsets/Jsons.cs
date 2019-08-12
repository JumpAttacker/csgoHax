using Newtonsoft.Json;

namespace CsgoHaxOverlay.JsonOffsets
{
    public partial class GitHubOffsets
    {
        public static GitHubOffsets FromJson(string json) => JsonConvert.DeserializeObject<GitHubOffsets>(json, Converter.Settings);
    }
}