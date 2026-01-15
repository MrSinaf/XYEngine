namespace XYEngine.Debugs;

internal class DebugHotReload
{
	public static readonly Dictionary<string, string> hotReloadList = [];
	public static string assetsSourcePath;
	
	private static FileSystemWatcher watches;
	
	public static bool actif
	{
		get;
		set
		{
			field = value;
			watches.EnableRaisingEvents = value;
		}
	}
	
	internal static void Init()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while (dir.Parent != null)
		{
			dir = dir.Parent;
			if (dir.Name != "bin")
				continue;
			
			assetsSourcePath = Path.Combine(dir.Parent.FullName, "assets");
			if (!Directory.Exists(assetsSourcePath))
				return;
			
			watches = new FileSystemWatcher(assetsSourcePath);
			watches.IncludeSubdirectories = true;
			watches.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | 
								   NotifyFilters.CreationTime | NotifyFilters.Size;
			watches.Changed += (_, args) =>
			{
				if (args.FullPath.EndsWith('~'))
					return;
				
				var relativePath = args.FullPath.Replace(assetsSourcePath + "\\", "");
				hotReloadList[relativePath] = args.FullPath;
			};
			return;
		}
	}
	
	internal static void Update()
	{
		foreach (var (path, source) in hotReloadList)
		{
			try
			{
				if (Vault.ReloadResource(path, source))
					XY.InternalLog("HOT RELOAD", $"'{path}' reloaded.", TypeLog.Info);
			}
			catch
			{
				hotReloadList.Remove(path);
				throw;
			}
		}
		
		hotReloadList.Clear();
	}
}