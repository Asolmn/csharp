# interface 知识讲解：契约、实现、多态与依赖注入

本文先讲接口本身的语法和类型规则，再解释如何用它组织后端代码。项目示例和练习放在最后；阅读理论部分不依赖 Example.cs。

## 面向有编程开发经验读者的阅读路线

目标：理解 C# 接口契约、声明实现关系和接口调用规则，能通过接口替换具体实现。

- **第一遍掌握**：第 1～5 节与第 7 节：声明实现关系、public 成员、接口属性、接口变量和依赖传入。
- **第二遍再学**：第 6 节显式接口实现、第 8 节抽象类对比，以及默认接口方法和泛型约束。
- **理解自检**：如果新增一个实现类型，调用方需要修改什么？哪些地方可以保持不变？

本课面向具有一定年限编程开发工作经验的读者，使用 .NET 控制台验证 C# 语言规则。已掌握的循环、分支、函数和集合基础可快速略读；重点理解 C# 的语法、行为规则及容易与已有经验混淆的地方。文中的 Go 对照作为可选补充，不熟悉 Go 可跳过。游戏名称只提供语境，不引入 Unity API、组件或编辑器操作。

## 1. interface 是什么，解决什么问题

**接口是一种契约，规定一个类型需要向使用者提供哪些能力。** 使用者只依赖这份契约，就不必了解实现的内部细节。

例如，订单服务需要“发送通知”，但不一定关心它由邮件、短信还是测试替身完成。接口规定 Send 的参数和返回类型，实现类决定怎么发送。

```csharp
public interface IMessageSender
{
    void Send(string message);
}
```

接口也是一种类型，可以用作变量、参数、字段和返回值的类型，但不能直接实例化：`new IMessageSender()` 无法编译。接口以 `I` 开头是命名约定，不是编译器强制要求。

## 2. 声明和实现接口

```csharp
public class ConsoleSender : IMessageSender
{
    public void Send(string message)
    {
        Console.WriteLine(message);
    }
}
```

`: IMessageSender` 声明实现关系；`public void Send(string message)` 提供实现。普通无方法体的接口成员隐含 public；常见的隐式成员实现需要一个兼容的 public 实例成员，名字、参数和返回类型要满足契约。

```csharp
IMessageSender sender = new ConsoleSender();
sender.Send("订单已完成");
```

赋值没有把 ConsoleSender 对象变成另一种对象，只是用接口类型的变量访问它。对象的实际类型仍是 ConsoleSender。

## 3. 和 Go 的根本差异：结构满足与声明关系

Go 只要求方法集满足：

```go
type Sender interface { Send(message string) }
type ConsoleSender struct{}
func (ConsoleSender) Send(message string) {}
var sender Sender = ConsoleSender{}
```

C# 即使存在完全相同的方法，也不会单凭“长得像”建立接口关系：

```csharp
public class OtherSender
{
    public void Send(string message) { }
}
// IMessageSender sender = new OtherSender(); // 编译错误：未声明实现该接口
```

需要让 OtherSender 声明 `: IMessageSender`，或通过适配器包装它。派生类可以继承基类已建立的接口实现关系，不必重复声明。

注意术语：C# 所说的“隐式接口实现”通常是通过 public 成员实现契约，并不意味着像 Go 一样自动满足接口。下一节的“显式接口实现”则是另一种成员实现语法。

## 4. 接口变量、实际类型与多态

```csharp
public class PrefixSender : IMessageSender
{
    public void Send(string message) => Console.WriteLine($"[通知] {message}");
    public void Reset() { }
}
```

```csharp
IMessageSender sender = new PrefixSender();
sender.Send("hello"); // 调用 PrefixSender.Send
// sender.Reset();    // 编译错误：接口没有声明 Reset
```

编译器根据变量的静态类型检查能访问哪些成员；运行时根据实际对象选择对应的接口实现。因此同一段 `sender.Send(...)` 可以表现出不同的具体行为，这就是接口多态。

如果确实需要额外能力，可用 `is` 模式匹配：

```csharp
if (sender is PrefixSender concrete)
    concrete.Reset();
```

它在目的上类似 Go 的类型断言检查。但业务层如果到处判断具体实现，往往说明接口抽象不够合适；优先把真正需要的能力表达进契约，而不是不断向下转换。

## 5. 属性、多接口与接口继承

接口可以声明属性，规定访问能力，不负责为实例分配普通字段：

```csharp
public interface IEntity
{
    int Id { get; }
}
public class Customer : IEntity
{
    public int Id { get; private set; }
    public Customer(int id) { Id = id; }
}
```

`IEntity.Id` 只要求读取能力，具体类可以保留自己的私有 setter。接口无法规定实现一定使用自动属性还是计算属性。

一个类可以实现多个接口；接口也能继承多个接口：

```csharp
public interface IReader { string Read(); }
public interface IWriter { void Write(string value); }
public interface IStore : IReader, IWriter { }

public class MemoryStore : IStore
{
    private string _value = "";
    public string Read() => _value;
    public void Write(string value) { _value = value; }
}
```

实现 IStore 就需要满足其继承来的 Read 和 Write 契约。类如果同时继承基类并实现接口，基类写在前面，例如 `class Cache : BaseCache, IReader, IWriter`。

## 6. 显式接口实现：指定通过哪个接口访问

当多个接口存在同名成员且需要不同含义，或不希望成员作为类的普通公开 API 时，可以显式实现：

```csharp
public interface IFileOutput { void Write(string text); }
public interface IAuditOutput { void Write(string text); }

public class Output : IFileOutput, IAuditOutput
{
    void IFileOutput.Write(string text) => Console.WriteLine($"文件：{text}");
    void IAuditOutput.Write(string text) => Console.WriteLine($"审计：{text}");
}
```

```csharp
var output = new Output();
((IFileOutput)output).Write("hello");
((IAuditOutput)output).Write("hello");
// output.Write("hello"); // 无法编译：类没有普通公开 Write 成员
```

显式实现写成 `接口名.成员名`，不加 public 等访问修饰符，只能经由对应的接口访问。两次转换对于这个 class 都没有复制对象，只是选择访问契约。如果一个 public 方法就足以满足两个同签名契约，也可以共享同一个实现。

## 7. 接口与依赖注入不是同一个概念

接口是一种类型机制；依赖注入是一种把依赖从外部交给对象的组织方式。二者经常配合，但也可以向构造函数注入一个具体类。

```csharp
public class ReportService
{
    private readonly IMessageSender _sender;

    public ReportService(IMessageSender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public void Finish()
    {
        _sender.Send("报表生成完成");
    }
}
```

```csharp
var service = new ReportService(new ConsoleSender());
service.Finish();
```

服务声明自己需要的能力，调用方负责创建并选择实现。测试可以传入只记录消息的假实现，不必真的发短信。这个过程不依赖 ASP.NET Core 容器或任何第三方框架，与 Go 的构造函数接收接口参数很接近。

`readonly` 限制 `_sender` 字段在构造完成后重新赋值，不保证 sender 对象本身不可变或线程安全。

## 8. interface 与 abstract class 如何选择

| 对比项 | interface | abstract class |
| --- | --- | --- |
| 核心用途 | 定义能力契约 | 为一组派生类共享基础实现 |
| 能否直接 new | 不能 | 不能 |
| 普通实例字段和构造函数 | 不提供实例字段或实例构造函数 | 可以提供 |
| 多个契约/基类 | 类可实现多个接口 | 类只能继承一个直接基类 |
| struct 能否参与 | 可以实现接口 | 不能继承自定义抽象类 |
| 共享行为 | 可有默认实现，但有使用限制 | 可有普通、virtual、abstract 成员 |

需要“能保存”“能发送”等能力边界时优先考虑接口；多个相关类确实共享状态、构造规则和基础行为时再考虑抽象类。不要因为两个类碰巧有同名方法，就强行抽出共同基类。

## 9. 常见误区和进一步学习的边界

- **接口不是空壳类。** 它定义调用契约，不能直接创建实例；实际状态由实现类型保存。
- **接口不只允许方法。** 也能声明属性、事件和索引器；这里先掌握方法和属性。
- **接口并非绝对不能有实现。** 现代 C# 支持默认接口方法。若类未自行声明同名成员，默认实现通常要通过接口类型调用，不能因此认为它成为类的普通成员。
- **接口不只给 class 用。** struct 也可以实现；转换成接口变量通常会装箱并复制值，修改行为要考虑这份副本。
- **接口变量也可以为空。** 可用 `IMessageSender?` 表达允许 null；契约不会自动消除空引用风险。
- **泛型约束不同于声明一个接口变量。** `where T : IMessageSender` 约束 T 必须满足接口，可留到泛型章节；对于值类型，合适的泛型调用还能避免一些装箱。

本课先能独立声明接口、实现它、用接口参数替换实现，并理解 Go 的隐式满足与 C# 接口声明的差异即可。

## 配套示例与练习

以下将前面的语言知识应用到项目代码。命令均在 `/Users/zaijing/project/csharp` 目录执行。

### 完整示例：统一调用不同的受伤对象

阅读 [Example.cs](Example.cs)。IDamageable 声明读取生命值和接受伤害的能力，Enemy 与 Crate 各自实现，Weapon 只调用接口。

```sh
dotnet run -- interface example
```

预期输出：

```text
敌人受伤，剩余生命值：70
木箱受伤，剩余耐久：0
敌人：70，木箱：0
```

重点观察 `: IDamageable`、接口的 getter 与类的 private setter、接口类型可见的成员范围。业务只是少量数值运算，不涉及碰撞、场景或框架。

完成示例后，练习刻意换一个题材，检查你能否迁移同一套语言规则。业务场景无需掌握额外框架。

### 自己练习：结算折扣策略

打开 [Exercise.cs](Exercise.cs)，骨架提供了 `IDiscountPolicy`、两个实现类和 `CheckoutService`。补全：

1. `NoDiscount.Apply` 返回原金额。
2. `VipDiscount.Apply` 返回八折金额，使用 `decimal` 字面量 `0.8m`。本题不处理币种和舍入。
3. `CheckoutService.Checkout` 拒绝负数金额，抛 `ArgumentOutOfRangeException`；合法金额传给 `_policy.Apply`，返回策略结果。

```sh
dotnet run -- interface exercise
```

不要在服务里写 `if (_policy is VipDiscount)`，也不要自己 `new VipDiscount()`。调用方已经选择好了策略，服务的职责是使用注入的接口。

检查中还有一个固定返回 7 的 `FixedDiscount`，用于验证：服务确实委托给传入的实现，并传递原始金额。这比只测试“100 变成 80”更能验证你理解了接口的用途。

### 自测问题

- 仅有 `Apply` 方法但没有声明 `: IDiscountPolicy` 的类，能赋给接口变量吗？
- 新增节日折扣时，是否必须修改 `CheckoutService`？
- 依赖注入一定要安装框架吗？

答案要点：不能隐式满足接口；无需修改服务，只需新增实现并在组装时传入；不需要，构造函数传参就能完成。
