namespace XYEngine.Debugs;

public enum TypeLog
{
    Normal, Info, Warning,
    Error
}

public static class Debug
{
    public static void Log(object content, TypeLog type = TypeLog.Normal)
    {
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("GAME");
        Console.ResetColor();
        Console.Write("] > ");
        Console.ForegroundColor = TypeLogColor(type);
        Console.WriteLine(content);
        Console.ResetColor();
    }

    internal static void LogIntern(object content, TypeLog type = TypeLog.Normal)
    {
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("X");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("Y");
        Console.ResetColor();
        Console.Write("]   > ");
        Console.ForegroundColor = TypeLogColor(type);
        Console.WriteLine(content);
        Console.ResetColor();
    }

    private static ConsoleColor TypeLogColor(TypeLog type)
        => type switch
        {
            TypeLog.Normal => ConsoleColor.White,
            TypeLog.Info => ConsoleColor.Green,
            TypeLog.Warning => ConsoleColor.Yellow,
            TypeLog.Error => ConsoleColor.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}