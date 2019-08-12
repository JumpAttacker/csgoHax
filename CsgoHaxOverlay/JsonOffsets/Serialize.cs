using Newtonsoft.Json;

namespace CsgoHaxOverlay.JsonOffsets
{
    public static class Serialize
    {
        public static string ToJson(this GitHubOffsets self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}