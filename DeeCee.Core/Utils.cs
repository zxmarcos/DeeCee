namespace DeeCee.Core;

public static class Utils
{
    private static ConsoleColor _bgColor;
    private static ConsoleColor _fgColor;

    public static void SaveConsole()
    {
        _bgColor = Console.BackgroundColor;
        _fgColor = Console.ForegroundColor;
    }
    
    public static void RestoreConsole()
    {
        Console.BackgroundColor = _bgColor;
        Console.ForegroundColor = _fgColor;
    }

    public static void SetConsoleColor(ConsoleColor fg = ConsoleColor.Black, ConsoleColor bg = ConsoleColor.White)
    {
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
    }
}