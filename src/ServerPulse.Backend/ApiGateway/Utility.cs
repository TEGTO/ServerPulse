using Shared;
using System.Text.Json;

namespace ApiGateway
{
    public static class Utility
    {
        public static void MergeJsonFiles(string[] filePaths, string outputPath)
        {
            var mergedDict = new Dictionary<string, JsonElement>();
            foreach (var filePath in filePaths)
            {
                var json = File.ReadAllText(filePath);

                if (json.TryToDeserialize(out Dictionary<string, JsonElement>? dict) && dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        if (mergedDict.ContainsKey(kvp.Key))
                        {
                            var existingValue = mergedDict[kvp.Key];
                            var newValue = kvp.Value;
                            mergedDict[kvp.Key] = MergeArrays(existingValue, newValue);
                        }
                        else
                        {
                            mergedDict[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            var mergedJson = JsonSerializer.Serialize(mergedDict, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputPath, mergedJson);
        }
        private static JsonElement MergeArrays(JsonElement existingValue, JsonElement newValue)
        {
            existingValue.GetRawText().TryToDeserialize(out List<JsonElement>? existingList);
            newValue.GetRawText().TryToDeserialize(out List<JsonElement>? newList);
            existingList?.AddRange(newList ?? []);
            return JsonDocument.Parse(JsonSerializer.Serialize(existingList)).RootElement;
        }
    }
}