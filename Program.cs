// 示例：dotnet run -- class example；练习：dotnet run -- class exercise
var topic = args.ElementAtOrDefault(0)?.ToLowerInvariant() ?? "all";
var mode = args.ElementAtOrDefault(1)?.ToLowerInvariant() ?? "example";
var lessons = new Dictionary<string, (Action Example, Action Exercise)>
{
    ["class"] = (Lessons.Class.Example.Run, Lessons.Class.Exercise.Run),
    ["struct"] = (Lessons.Struct.Example.Run, Lessons.Struct.Exercise.Run),
    ["interface"] = (Lessons.Interface.Example.Run, Lessons.Interface.Exercise.Run),
    ["enum"] = (Lessons.Enum.Example.Run, Lessons.Enum.Exercise.Run)
};
if ((topic != "all" && !lessons.ContainsKey(topic)) || mode is not ("example" or "exercise"))
{
    Console.WriteLine("用法：dotnet run -- <class|struct|interface|enum|all> [example|exercise]");
    Environment.ExitCode = 1;
    return;
}
foreach (var (name, lesson) in lessons)
{
    if (topic != "all" && topic != name) continue;
    Console.WriteLine($"\n=== {name} / {mode} ===");
    try
    {
        if (mode == "example") lesson.Example();
        else lesson.Exercise();
    }
    catch (NotImplementedException)
    {
        Console.WriteLine("练习尚未完成：请补全对应 Exercise.cs 中的 TODO。");
        Environment.ExitCode = 1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"运行失败：{ex.Message}");
        Environment.ExitCode = 1;
    }
}
