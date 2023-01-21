using System.Dynamic;
using System.IO;
using Avalonia.Controls.ApplicationLifetimes;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace MemeManager.Services.Implementations;

public class LifecycleService : ILifecycleService
{
    private readonly LayoutConfiguration _layoutConfig;
    private readonly ILogger _log;
    private readonly MemesConfiguration _memesConfig;

    public LifecycleService(ILogger logger, MemesConfiguration memesConfiguration,
        LayoutConfiguration layoutConfiguration)
    {
        _log = logger;
        _memesConfig = memesConfiguration;
        _layoutConfig = layoutConfiguration;
        App.ShuttingDown += HandleShutdown;
    }

    private void HandleShutdown(object? sender,
        ControlledApplicationLifetimeExitEventArgs controlledApplicationLifetimeExitEventArgs)
    {
        _log.LogInformation("Shutting down with exit code {ExitCode}",
            controlledApplicationLifetimeExitEventArgs.ApplicationExitCode);
        PersistConfigInYaml();
    }

    /// <summary>
    /// This is an stupid, terrible solution for writing YAML values. I couldn't figure out how to just update values
    /// in the appsettings file using purely YamlDotNet. Instead I convert it to JSON because JSON.NET is way more
    /// flexible. It is then converted back to YAML and saved.
    /// </summary>
    private void PersistConfigInYaml()
    {
        TextReader reader = File.OpenText("appsettings.yaml");
        var yaml = new YamlStream();
        yaml.Load(reader);
        // Get the root node of the document
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

        // Convert the YamlNode to JSON because JSON.NET offers way easier document manipulation
        var mappingJson = mapping.ToJson();
        var root = JObject.Parse(mappingJson);

        reader.Close();

        // Update each individual section that the user has the ability to change from the UI
        root["Memes"] = JObject.FromObject(_memesConfig);
        root["Layout"] = JObject.FromObject(_layoutConfig);

        // https://stackoverflow.com/a/42212102/1687436
        var expConverter = new ExpandoObjectConverter();
        dynamic? deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(root.ToString(), expConverter);
        var yamlSerializer = new YamlDotNet.Serialization.Serializer();
        string yamlString = yamlSerializer.Serialize(deserializedObject);

        // Overwrite appsettings.yaml with the updated values
        File.WriteAllText("appsettings.yaml", yamlString);
    }
}

internal static class YamlExtensions
{
    /// <summary>
    /// Converts a YamlNode to a JSON string.
    ///
    /// See https://github.com/aaubry/YamlDotNet/issues/131#issuecomment-628280069
    /// </summary>
    /// <param name="node">The YamlNode instance to convert.</param>
    /// <returns>The converted JSON string.</returns>
    public static string ToJson(this YamlNode node)
    {
        var stream = new YamlStream { new YamlDocument(node) };
        using var writer = new StringWriter();
        stream.Save(writer);

        using var reader = new StringReader(writer.ToString());
        var deserializer = new Deserializer();
        var yamlObject = deserializer.Deserialize(reader);
        var serializer = new SerializerBuilder()
            .JsonCompatible()
            .Build();
        return serializer.Serialize(yamlObject).Trim();
    }
}
