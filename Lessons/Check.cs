namespace Lessons;

internal static class Check
{
    public static void That(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException($"检查失败：{description}");
        Console.WriteLine($"通过：{description}");
    }
    public static void Throws<T>(Action action, string description) where T : Exception
    {
        try { action(); }
        catch (T) { Console.WriteLine($"通过：{description}"); return; }
        throw new InvalidOperationException($"检查失败：{description}，预期抛出 {typeof(T).Name}");
    }
}
