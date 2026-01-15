using System.Reflection;

namespace XYEngine;

public static class Vault
{
	private const string LOG_NAME = "Vault";
	
	private static readonly Dictionary<string, AssetReference> assets = [];
	private static readonly Dictionary<string, AssetReference> embeddedAssets = [];
	
	public static bool AddAsset<T>(string name, T asset) where T : IAsset
	{
		if (!assets.TryAdd(name, new AssetReference(asset)))
		{
			XY.InternalLog(LOG_NAME, $"You are trying to add asset `{name}`, but it is already present in the cache!",
						   TypeLog.Warning);
			return false;
		}
		
		return true;
	}
	
	public static void RemoveAsset(string name)
	{
		if (assets.Remove(name, out var reference))
			reference.asset.Destroy();
	}
	
	public static T GetAsset<T>(string name) where T : IAsset => TryGetAsset<T>(name, out var asset)
		? asset
		: throw new NullReferenceException(
			$"The asset '{name}' is not present in the cache. Use '{nameof(TryGetAsset)}' to check if it exists!");
	
	public static bool TryGetAsset<T>(string name, out T asset) where T : IAsset
	{
		if (assets.TryGetValue(name, out var value))
		{
			asset = (T)value.asset;
			return true;
		}
		
		asset = default;
		return false;
	}
	
	public static async Task<T> LoadResource<T>(string name, string path, IResourceConfig config = null)
		where T : IResource, new()
	{
		if (assets.TryGetValue(name, out var reference))
		{
			XY.InternalLog(LOG_NAME, $"You are trying to add asset `{name}`, but it is already present in the cache!",
						   TypeLog.Warning);
			return (T)reference.asset;
		}
		
		var fullPath = Path.Combine("assets", path);
		
		if (!File.Exists(fullPath))
			throw new FileNotFoundException($"Asset not found at path '{path}'");
		
		try
		{
			await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read,
													FileShare.ReadWrite | FileShare.Delete, 4096, true);
			
			var asset = new T();
			var ressource = new Resource(path, Path.GetExtension(fullPath).ToLower(), stream, config);
			asset.Load(ressource);
			
			assets[name] = new AssetReference(asset, ressource);
			return asset;
		}
		catch (Exception ex)
		{
			XY.InternalLog(LOG_NAME, $"Failed to load resource asynchronously at path '{path}'.\n{ex.Message}",
						   TypeLog.Error);
			throw;
		}
	}
	
	public static T GetEmbeddedAsset<T>(string name) where T : IAsset
	{
		if (!embeddedAssets.TryGetValue(name, out var value))
			throw new NullReferenceException(
				$"The asset '{name}' is not present in the cache. Use '{nameof(TryGetAsset)}' to check if it exists!");
		
		return (T)value.asset;
	}
	
	public static void RemoveEmbeddedAsset(string name)
	{
		if (embeddedAssets.Remove(name, out var reference))
			reference.asset.Destroy();
	}
	
	internal static T LoadEmbeddedResource<T>(string name, string path, IResourceConfig config = null)
		where T : IResource, new()
	{
		if (embeddedAssets.TryGetValue(name, out var value))
		{
			XY.InternalLog(LOG_NAME, $"You are trying to add asset `{name}`, but it is already present in the cache!",
						   TypeLog.Warning);
			return (T)value.asset;
		}
		
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("XYEngine.assets." + path);
		
		if (stream == null)
			throw new FileNotFoundException($"Asset not found at path '{path}'");
		
		try
		{
			var asset = new T();
			var resource = new Resource(path, Path.GetExtension(path).ToLower(), stream, config);
			
			asset.Load(resource);
			embeddedAssets.Add(name, new AssetReference(asset, resource));
			return asset;
		}
		catch (Exception ex)
		{
			XY.InternalLog(LOG_NAME, $"Failed to load embedded resource asynchronously at path '{path}'.\n{ex.Message}",
						   TypeLog.Error);
			throw;
		}
	}
	
	public static bool ReloadResource(string name, string sourcePath)
	{
		assets.TryGetValue(name, out var reference);
		
		if (reference.asset is not IResource iResource)
			throw new FileNotFoundException($"Asset '{name}' is not a resource !");
		
		var resource = reference.resource;
		if (!File.Exists(sourcePath))
			throw new FileNotFoundException($"Asset not found at path '{sourcePath}'");
		
		resource.onHotReload = true;
		try
		{
			using var stream = new MemoryStream();
			using (var fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read,
												   FileShare.ReadWrite | FileShare.Delete))
			{
				fileStream.CopyTo(stream);
				stream.Position = 0;
			}
			
			iResource.Load(resource);
			File.Copy(sourcePath, Path.Combine("assets", resource.path), true);
			return true;
		}
		catch
		{
			var fullPath = Path.Combine("assets", resource.path);
			
			if (!File.Exists(fullPath))
				throw new FileNotFoundException($"Asset not found at path '{resource.path}'");
			
			using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read,
											  FileShare.ReadWrite | FileShare.Delete);
			iResource.Load(resource);
			return false;
		}
	}
	
	public static bool ReloadEmbeddedResource(string name, string sourcePath)
	{
		embeddedAssets.TryGetValue(name, out var reference);
		if (reference.asset is not IResource iResource)
			throw new FileNotFoundException($"Asset '{name}' is not a resource !");
		
		var resource = reference.resource;
		if (!File.Exists(sourcePath))
			throw new FileNotFoundException($"Asset not found at path '{sourcePath}'");
		
		resource.onHotReload = true;
		try
		{
			using var stream = new MemoryStream();
			using (var fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read,
												   FileShare.ReadWrite | FileShare.Delete))
			{
				fileStream.CopyTo(stream);
				stream.Position = 0;
			}
			
			iResource.Load(resource);
			File.Copy(sourcePath, Path.Combine("assets", resource.path), true);
			return true;
		}
		catch
		{
			var fullPath = Path.Combine("assets", resource.path);
			
			if (!File.Exists(fullPath))
				throw new FileNotFoundException($"Asset not found at path '{resource.path}'");
			
			using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read,
											  FileShare.ReadWrite | FileShare.Delete);
			iResource.Load(resource);
			return false;
		}
	}
}

public interface IAsset
{
	protected internal void Destroy() { }
}

internal class AssetReference(IAsset asset, Resource resource = null)
{
	public readonly IAsset asset = asset;
	public readonly Resource resource = resource;
}

public class Resource(string path, string extension, Stream stream, IResourceConfig config, bool onHotReload = false)
{
	public readonly string path = path;
	public readonly string extension = extension;
	public readonly Stream stream = stream;
	public readonly IResourceConfig config = config;
	public bool onHotReload = onHotReload;
}

public interface IResourceConfig;

public interface IResource : IAsset
{
	void Load(Resource ressource);
}