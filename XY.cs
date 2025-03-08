using System.Reflection;
using System.Runtime.InteropServices;
using XYEngine.Scenes;

namespace XYEngine;

public enum TypeLog { Normal, Info, Warning, Error }

public static class XY
{
	private static bool gameIsRunning;
	
	public enum VersionState { Release, Stable, Preview, Dev }
	
	public const VersionState VERSION_STATE = VersionState.Dev;
	public static string version => Assembly.GetAssembly(typeof(XY)).GetName().Version.ToString();
	
	public static void LaunchGame<T>(string name, Action splashScreenAction = null) where T : Scene, new()
	{
		if (gameIsRunning)
		{
			InternalLog("XY", "You can only call 'LaunchGame' once!", TypeLog.Error);
			return;
		}
		
		string[] nativeLibsName = ["cimgui", "freetype", "glfw3", "soft_oal"];
		foreach (var lib in nativeLibsName)
			ResolveNativeLibrary(lib);
		
		AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		InternalLog($"  XYEngine v{version} - {VERSION_STATE}", TypeLog.Info);
		
		SplashScreen.startScene = typeof(T);
		SplashScreen.action = splashScreenAction;
		gameIsRunning = true;
		
		_ = new GameWindow(name);
	}
	
	public static void Log(object content, TypeLog type = TypeLog.Normal)
	{
		Console.Write("[");
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.Write("GAME");
		Console.ResetColor();
		Console.Write("] > ");
		if (type == TypeLog.Normal)
			Console.ResetColor();
		else
			Console.ForegroundColor = TypeLogColor(type);
		
		Console.WriteLine(content);
		Console.ResetColor();
	}
	
	internal static void InternalLog(object content, TypeLog type = TypeLog.Normal)
	{
		Console.Write("[");
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write("X");
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.Write("Y");
		Console.ResetColor();
		Console.Write("]   > ");
		if (type == TypeLog.Normal)
			Console.ResetColor();
		else
			Console.ForegroundColor = TypeLogColor(type);
		
		Console.WriteLine(content);
		Console.ResetColor();
	}
	
	internal static void InternalLog(string sender, object content, TypeLog type = TypeLog.Normal)
		=> InternalLog($"[{sender.ToUpper()}] {content}", type);
	
	private static ConsoleColor TypeLogColor(TypeLog type) => type switch
	{
		TypeLog.Info    => ConsoleColor.Green,
		TypeLog.Warning => ConsoleColor.Yellow,
		TypeLog.Error   => ConsoleColor.Red,
		_               => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};
	
	private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
	{
		var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packages", new AssemblyName(args.Name).Name + ".dll");
		return !File.Exists(assemblyPath) ? null : Assembly.LoadFrom(assemblyPath);
	}
	
	private static void ResolveNativeLibrary(string libraryName)
	{
		var nativeLibExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "{0}.dll" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "lib{0}.so" : "lib{0}.dylib";
		var nativeLibPath = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier, "native", string.Format(nativeLibExtension, libraryName));
		
		if (File.Exists(nativeLibPath))
			NativeLibrary.Load(nativeLibPath);
	}
}