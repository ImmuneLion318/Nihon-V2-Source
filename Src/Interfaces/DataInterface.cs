using System.IO;
using Newtonsoft.Json;

namespace Nihon.Interfaces;

internal static class DataInterface
{
    public static T Read<T>(string Name) => JsonConvert.DeserializeObject<T>(File.ReadAllText($"{Name}.json"));

    public static void Save<T>(string Name, T Data) => File.WriteAllText($"{Name}.json", JsonConvert.SerializeObject(Data, Formatting.Indented));

    public static void Delete(string Name)
    {
        if (File.Exists($"{Name}.json"))
            File.Delete($"{Name}.json");
    }

    public static bool Exists(string Name) => File.Exists($"{Name}.json");
}