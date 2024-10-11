using System.Text.Json;
using StbImageSharp;
using XYEngine.Graphics;

namespace XYEngine;

public static class Resources
{
    private const string RESOURCES_PATH = "Resources";
    
    public static Texture LoadTexture(string path)
    {
        var result = ImageResult.FromMemory(File.ReadAllBytes(Path.Combine(RESOURCES_PATH, "Textures", path)), ColorComponents.RedGreenBlueAlpha);
        return new Texture(result.Width, result.Height, GetPixelsInBytes(result.Data));
    }

    public static TextureAtlas LoadTextureAtlas(string path)
    {
        var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(RESOURCES_PATH, "Data", path + ".xy"))).RootElement;
        if (!data.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "TextureAtlas")
            throw new Exception("Le type est introuvable ou invalide !");

        var atlas = data.GetProperty("content").Deserialize<TextureAtlas.Data>();
        var image = ImageResult.FromMemory(File.ReadAllBytes(Path.Combine(RESOURCES_PATH, "Textures", atlas.texturePath)), ColorComponents.RedGreenBlueAlpha);

        return new TextureAtlas(image.Width, image.Height, GetPixelsInBytes(image.Data), atlas);
    }
    
    public static TextureSheet LoadTextureSheet(string path)
    {
        var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(RESOURCES_PATH, "Data", path + ".xy"))).RootElement;
        if (!data.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "TextureSheet")
            throw new Exception("Le type est introuvable ou invalide !");

        var atlas = data.GetProperty("content").Deserialize<TextureSheet.Data>();
        var image = ImageResult.FromMemory(File.ReadAllBytes(Path.Combine(RESOURCES_PATH, "Textures", atlas.texturePath)), ColorComponents.RedGreenBlueAlpha);

        return new TextureSheet(image.Width, image.Height, GetPixelsInBytes(image.Data), atlas);
    }

    public static Shader LoadShader(string pathName)
    {
        return new Shader(File.ReadAllText(Path.Combine(RESOURCES_PATH, "Shaders", pathName + ".vert")), 
                          File.ReadAllText(Path.Combine(RESOURCES_PATH, "Shaders", pathName + ".frag")));
    }

    private static Color[] GetPixelsInBytes(byte[] bytes)
    {
        var pixels = new Color[bytes.Length / 4];
        for (var i = 0; i < pixels.Length; i++)
        {
            var index = i * 4;
            pixels[i] = new Color(bytes[index], bytes[index + 1], bytes[index + 2], bytes[index + 3]);
        }

        return pixels;
    }
}