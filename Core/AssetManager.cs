using System.Reflection;

namespace XYEngine;

public static class AssetManager
{
	private const string LOG_NAME = "Asset Manager";
	private static readonly Dictionary<string, IAsset> cache = [];
	private static readonly Dictionary<string, IAsset> embeddedCache = [];
	
	public static T LoadAsset<T>(string path, IAssetConfig config = null, bool acceptGet = false) where T : IAsset, new()
	{
		if (cache.TryGetValue(path, out var value))
		{
			if (!acceptGet)
				XY.InternalLog(LOG_NAME, $"You are trying to load `{path}`, but it is already present in the cache!", TypeLog.Warning);
			
			return (T)value;
		}
		
		var fullPath = Path.Combine("assets", path);
		if (!File.Exists(fullPath))
			throw new FileNotFoundException($"Asset not found at path '{path}'");
		
		using var stream = new FileStream(fullPath, FileMode.Open);
		
		var asset = new T();
		asset.Load(new AssetProperty(stream, Path.GetExtension(fullPath).ToLower(), config));
		cache.Add(path, asset);
		
		return asset;
	}
	
	public static bool TryGetAsset<T>(string path, out T asset) where T : IAsset
	{
		if (cache.TryGetValue(path, out var value))
		{
			asset = (T)value;
			return true;
		}
		
		asset = default;
		return false;
	}
	
	public static T GetAsset<T>(string path) where T : IAsset
	{
		if (TryGetAsset<T>(path, out var asset))
			return asset;
		
		throw new NullReferenceException($"The asset '{path}' is not present in the cache. Please load it before retrieving it, or use " +
										 $"'{nameof(TryGetAsset)}' to check if it exists!");
	}
	
	public static void AddAsset<T>(string path, T asset) where T : IAsset { }
	
	public static void UnLoadAsset(string path)
	{
		if (cache.Remove(path, out var asset))
			asset.UnLoad();
	}
	
	public static bool TryGetEmbeddedAsset<T>(string path, out T asset) where T : IAsset
	{
		if (embeddedCache.TryGetValue(path, out var value))
		{
			asset = (T)value;
			return true;
		}
		
		asset = default;
		return false;
	}
	
	public static T GetEmbeddedAsset<T>(string path) where T : IAsset
	{
		if (TryGetEmbeddedAsset<T>(path, out var asset))
			return asset;
		
		throw new NullReferenceException($"The asset '{path}' is not present in the cache. Please load it before retrieving it, or use " +
										 $"'{nameof(TryGetEmbeddedAsset)}' to check if it exists!");
	}
	
	internal static T LoadEmbeddedAsset<T>(string path, IAssetConfig config = null) where T : IAsset, new()
	{
		if (embeddedCache.TryGetValue(path, out var value))
		{
			XY.InternalLog(LOG_NAME, $"You are trying to load `{path}`, but it is already present in the embedded cache!", TypeLog.Warning);
			return (T)value;
		}
		
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("XYEngine.assets." + path);
		if (stream == null)
			throw new FileNotFoundException($"Asset not found at path '{path}'");
		
		var asset = new T();
		asset.Load(new AssetProperty(stream, Path.GetExtension(path).ToLower(), config));
		embeddedCache.Add(path, asset);
		
		return asset;
	}
	
	internal static void UnLoadEmbeddedAsset(string path)
	{
		embeddedCache.Remove(path, out var asset);
		asset?.UnLoad();
	}
}

public interface IAsset
{
	internal void Load(AssetProperty property);
	internal void UnLoad();
}

public interface ICustomAsset
{
	internal void UnLoad();
}

public interface IAssetConfig;

public class AssetProperty(Stream stream, string extension, IAssetConfig config)
{
	public readonly Stream stream = stream;
	public readonly string extension = extension;
	public readonly IAssetConfig config = config;
}