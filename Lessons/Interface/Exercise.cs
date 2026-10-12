namespace Lessons.Interface;

// 学习目标：把已有 Go 接口经验映射到 C# 的声明关系和接口调用规则。
// 先预测 Run 中每个检查的结果，再只补全 TODO；保留检查代码验证理解。
internal static class Exercise
{
    public static void Run()
    {
        IDiscountPolicy normal = new NoDiscount();
        IDiscountPolicy vip = new VipDiscount();
        Check.That(new CheckoutService(normal).Checkout(100m) == 100m, "普通用户原价结算");
        Check.That(new CheckoutService(vip).Checkout(100m) == 80m, "VIP 八折结算");
        Check.That(new CheckoutService(vip).Checkout(0m) == 0m, "零金额可以结算");
        Check.Throws<ArgumentOutOfRangeException>(() => new CheckoutService(vip).Checkout(-1m), "拒绝负数订单金额");
        var fake = new FixedDiscount();
        Check.That(new CheckoutService(fake).Checkout(50m) == 7m, "服务使用注入的策略，不硬编码 VIP 判断");
        Check.That(fake.LastAmount == 50m, "服务把原始金额传给策略");
    }
    private sealed class FixedDiscount : IDiscountPolicy
    {
        public decimal LastAmount { get; private set; }
        public decimal Apply(decimal amount) { LastAmount = amount; return 7m; }
    }
}
internal interface IDiscountPolicy { decimal Apply(decimal amount); }
internal sealed class NoDiscount : IDiscountPolicy
{
    public decimal Apply(decimal amount)
    {
        // TODO：返回原价。金额校验由 CheckoutService 负责。
        throw new NotImplementedException();
    }
}
internal sealed class VipDiscount : IDiscountPolicy
{
    public decimal Apply(decimal amount)
    {
        // TODO：返回八折价格，本练习不处理金额舍入。
        throw new NotImplementedException();
    }
}
internal sealed class CheckoutService
{
    private readonly IDiscountPolicy _policy;
    public CheckoutService(IDiscountPolicy policy) => _policy = policy;
    public decimal Checkout(decimal amount)
    {
        // TODO：拒绝负数金额，再通过 _policy 计算结果。
        throw new NotImplementedException();
    }
}
