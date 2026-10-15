namespace Lessons.Enum;

// 学习目标：使用独立的枚举类型表达选项，并区分解析成功、成员已定义和业务允许。
// 先预测 Run 中每个检查的结果，再只补全 TODO；保留检查代码验证理解。
internal static class Exercise
{
    public static void Run()
    {
        Check.That(TicketRules.GetLabel(TicketStatus.Open) == "待处理", "Open 标签正确");
        Check.That(TicketRules.GetLabel(TicketStatus.Processing) == "处理中", "Processing 标签正确");
        Check.That(TicketRules.GetLabel(TicketStatus.Closed) == "已关闭", "Closed 标签正确");
        Check.That(TicketRules.GetLabel(TicketStatus.Unknown) == "未知", "Unknown 标签正确");
        Check.Throws<ArgumentOutOfRangeException>(() => TicketRules.GetLabel((TicketStatus)99), "处理未定义枚举值");
        Check.That(TicketRules.TryParseStatus("processing", out var parsed) && parsed == TicketStatus.Processing, "忽略大小写解析名称");
        Check.That(TicketRules.TryParseStatus("1", out parsed) && parsed == TicketStatus.Open, "接受已定义数字");
        Check.That(!TicketRules.TryParseStatus("99", out parsed) && parsed == TicketStatus.Unknown, "拒绝未定义数字，并重置输出值");
        Check.That(!TicketRules.TryParseStatus("oops", out parsed) && parsed == TicketStatus.Unknown, "拒绝非法名称");
        foreach (var from in System.Enum.GetValues<TicketStatus>())
        foreach (var to in System.Enum.GetValues<TicketStatus>())
        {
            var expected = (from == TicketStatus.Open && to == TicketStatus.Processing)
                           || (from == TicketStatus.Processing && to == TicketStatus.Closed);
            Check.That(TicketRules.CanTransition(from, to) == expected, $"状态迁移 {from} → {to}");
        }
        Check.That(!TicketRules.CanTransition((TicketStatus)99, TicketStatus.Open), "未定义来源状态不能迁移");
    }
}
internal enum TicketStatus { Unknown = 0, Open = 1, Processing = 2, Closed = 3 }
internal static class TicketRules
{
    public static string GetLabel(TicketStatus status)
    {
        // TODO：使用 switch 表达式返回中文标签，未定义值抛 ArgumentOutOfRangeException。
        switch (status)
        {
            case TicketStatus.Unknown:
                return "未知";
            case TicketStatus.Open:
                return "待处理";
            case TicketStatus.Processing:
                return "处理中";
            case TicketStatus.Closed:
                return "已关闭";
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "未定义的状态值");
        }
        throw new NotImplementedException();
    }
    public static bool TryParseStatus(string input, out TicketStatus status)
    {
        // TODO：忽略大小写解析名称或数字，并验证已定义；失败时设为 Unknown，返回 false。
        // 当前命名空间末段叫 Enum，因此用 System.Enum 调用枚举工具方法。
        if (System.Enum.TryParse<TicketStatus>(input, true, out var parsed) && System.Enum.IsDefined(typeof(TicketStatus), parsed))
        {
            status = parsed;
            return true;
        }
        else
        {
            status = TicketStatus.Unknown;
            return false;
        }
        throw new NotImplementedException();
    }
    public static bool CanTransition(TicketStatus from, TicketStatus to)
    {
        // TODO：只允许 Open → Processing、Processing → Closed；其他组合全部 false。
        if (!System.Enum.IsDefined(typeof(TicketStatus), from) || !System.Enum.IsDefined(typeof(TicketStatus), to))
        {
            return false;
        }
        return (from == TicketStatus.Open && to == TicketStatus.Processing) || 
            (from == TicketStatus.Processing && to == TicketStatus.Closed);
        throw new NotImplementedException();
    }
}
