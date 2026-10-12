# enum 知识讲解：枚举类型、转换、校验与位标志

本文先解释枚举本身的定义和使用规则，再把它应用到角色和工单状态。文中的短代码块用于说明概念，不要求直接拼接成同一个程序。

## 面向有编程开发经验读者的阅读路线

目标：使用独立的枚举类型表达选项，并区分解析成功、成员已定义和业务允许。

- **第一遍掌握**：第 1～7 节：编号、零值、强制转换、TryParse 与 IsDefined、switch。
- **第二遍再学**：第 8～9 节 Flags 位运算；第 10 节协议和持久化边界按需阅读。
- **理解自检**：为什么一个 enum 变量仍然可能保存没有声明过的数字？

本课面向具有一定年限编程开发工作经验的读者，使用 .NET 控制台验证 C# 语言规则。已掌握的循环、分支、函数和集合基础可快速略读；重点理解 C# 的语法、行为规则及容易与已有经验混淆的地方。文中的 Go 对照作为可选补充，不熟悉 Go 可跳过。游戏名称只提供语境，不引入 Unity API、组件或编辑器操作。

## 1. enum 是什么：把数字含义写进类型

**enum 是由一组具名整数常量构成的值类型。** 它让“状态 2”变成 `OrderState.Paid`，使调用者能读懂含义，并把不同种类的选项区分开。

```csharp
public enum OrderState
{
    Unknown = 0,
    Pending = 1,
    Paid = 2,
    Shipped = 3
}
```

```csharp
OrderState state = OrderState.Paid;
Console.WriteLine(state); // Paid
```

`OrderState` 是类型，`Paid` 是成员，成员要通过类型名访问。enum 不能在内部像 class 一样定义普通方法和属性；显示文案、校验和业务行为可以放到其他类的方法或扩展方法里。

“具名选项有限”不表示变量只能存这些成员，未定义数字的问题会在后文说明。

## 2. 与 Go 的类型化常量和 iota 对照

```go
type OrderState int
const (
    Unknown OrderState = iota
    Pending
    Paid
    Shipped
)
```

Go 通过自定义整数类型配合常量表达类似含义，C# 则提供独立的 enum 语法。它不是 Go 的无类型常量，也不能当作普通 int 随意混用。

不同枚举即使数值相同，也属于不同类型：

```csharp
public enum PaymentState { Unknown = 0, Done = 1 }
// OrderState state = PaymentState.Done; // 编译错误
```

这能避免把支付状态误传给订单状态。但使用强制转换仍可以绕过部分类型约束，所以输入边界依然需要校验。

## 3. 底层类型与成员编号规则

enum 底层默认是 int，也可以指定 byte、sbyte、short、ushort、int、uint、long、ulong，不能用 string、bool 或浮点类型。

```csharp
public enum Priority : byte
{
    Low,       // 0：首个未指定成员从 0 开始
    Normal = 10,
    High       // 11：前一个成员的数值加 1
}
```

数值必须处于底层整数范围内。成员可以不连续，也可以声明相同数值的别名，但别名会让“把数值还原成唯一名称”的假设失效，应谨慎使用。

对数据库、消息队列或 API 中长期保存的状态，应显式指定编号并保持已有编号稳定。调整代码中的成员顺序，不应改变既有数据含义。字符串名称和数据库显示文案也应区别对待，不能随意重命名后假设外部协议仍兼容。

## 4. 默认值与可空枚举

```csharp
OrderState state = default;
Console.WriteLine((int)state); // 0
OrderState? optional = null;
```

普通 enum 的默认值是底层整数 0；不是“第一个声明成员”。即使没有定义数值为 0 的成员，default 仍然是 0。

因此通常保留 `Unknown = 0` 或 `None = 0`。若“未知”与“未提供”需要区分，可以使用 `OrderState?`，即 Nullable<OrderState>：null 表示没有提供状态，Unknown 是一个实际枚举值。

## 5. 数字、名称与枚举之间的转换

```csharp
OrderState state = OrderState.Paid;
int code = (int)state;       // 2：枚举转数字
OrderState fromCode = (OrderState)2;
string name = state.ToString(); // Paid
```

转换不是业务校验：

```csharp
OrderState invalid = (OrderState)99; // 可以编译
Console.WriteLine(System.Enum.IsDefined(invalid)); // False
```

下面这些常用 API 作用不同：

| API | 用途 |
| --- | --- |
| `Enum.Parse<T>(text)` | 解析文本；失败可能抛异常 |
| `Enum.TryParse<T>(text, true, out value)` | 尝试解析，true 表示忽略大小写；通过 bool 报告能否解析 |
| `Enum.IsDefined(value)` | 检查值是否对应已声明成员 |
| `Enum.GetValues<T>()` | 获取成员对应的枚举值 |
| `Enum.GetNames<T>()` | 获取成员名称字符串 |

项目命名空间末段叫 Enum，因此项目中写 `System.Enum` 避免名称冲突。循环查看选项可以写：

```csharp
foreach (var item in System.Enum.GetValues<OrderState>())
    Console.WriteLine($"{item}: {(int)item}");
```

不要把 GetValues 的顺序当成业务展示顺序或始终当成声明顺序，它按底层数值的无符号大小排序；需要固定 UI 顺序时应显式定义。

## 6. TryParse 成功为什么还不够

TryParse 既接受名称，也接受可转换的数字字符串。例如 `"Paid"` 和 `"2"` 都能得到 Paid，`"99"` 也能解析成底层数值为 99 的枚举。

普通状态解析可以这样写：

```csharp
static bool TryReadState(string input, out OrderState state)
{
    if (System.Enum.TryParse<OrderState>(input, true, out var parsed)
        && System.Enum.IsDefined(parsed))
    {
        state = parsed;
        return true;
    }

    state = OrderState.Unknown;
    return false;
}
```

`out` 是输出参数，方法正常返回前必须赋值；这里同时返回“是否成功”和解析结果，目的上类似 Go 的 `(value, ok)`，但语法不同。

这份校验要求“解析后的值已定义”，不是严格的“输入只能是单个成员名”：TryParse 还支持逗号分隔的名称组合。如果 API 只允许名称、禁止数字或组合，需增加针对输入形式的白名单规则。即使 Unknown 是已定义值，业务也可以另外要求不允许 Unknown；类型合法与业务可接受是两个层次。

## 7. switch：把枚举映射成行为或文案

```csharp
static string GetLabel(OrderState state) => state switch
{
    OrderState.Unknown => "未知",
    OrderState.Pending => "待支付",
    OrderState.Paid => "已支付",
    OrderState.Shipped => "已发货",
    _ => throw new ArgumentOutOfRangeException(nameof(state))
};
```

switch 表达式根据输入匹配分支，每个 `=>` 右侧产生结果。`_` 匹配没有被前面覆盖的值；即使写完所有已命名成员，也要考虑强制转换产生的未定义数字。

枚举定义不会自动限制状态迁移。例如“已支付可以发货”需要业务规则：

```csharp
static bool CanShip(OrderState state) => state == OrderState.Paid;
```

状态之间也不能仅靠大小比较决定是否合法。枚举编号是编码，不一定代表阶段顺序；取消、退款等分支尤其如此。

## 8. Flags：一组选项如何组合成一个值

普通状态一般一次选择一个；权限可能同时具有读取和写入。此时让每个选项占一个独立二进制位：

```csharp
[Flags]
public enum Permission
{
    None = 0,
    Read = 1,    // 001
    Write = 2,   // 010
    Execute = 4  // 100
}
```

`[Flags]` 是特性，声明枚举按位组合的意图，也会影响字符串格式化；真正执行组合的是位运算。

```csharp
Permission granted = Permission.Read | Permission.Write; // 011
bool canRead = (granted & Permission.Read) != 0;          // True
granted |= Permission.Execute;                          // 添加权限：111
granted &= ~Permission.Write;                           // 移除权限：101
```

| 运算 | 含义 |
| --- | --- |
| `a | b` | 合并位 |
| `a & b` | 取共有位 |
| `~b` | 按位取反，常与 & 配合清除指定权限 |
| `a ^ b` | 翻转 b 指定的位 |

检查组合权限时，要区分“任意一个”和“全部”：

```csharp
var required = Permission.Read | Permission.Write;
bool hasAny = (granted & required) != 0;
bool hasAll = (granted & required) == required;
```

`granted.HasFlag(required)` 也表示全部包含。required 为 None/0 时，全包含判断为 true，因为没有要求任何位；检查是否完全没有权限应使用 `granted == Permission.None`。

## 9. Flags 的合法性校验与普通 enum 不同

如果只声明 Read=1 和 Write=2，组合值 3 可以是合法权限，但没有一个成员显式命名为 3，`Enum.IsDefined` 就会返回 false。因此不能直接用它判断所有 Flags 组合。

```csharp
var allowed = Permission.Read | Permission.Write | Permission.Execute;
bool valid = (granted & ~allowed) == Permission.None;
```

这个判断检查是否存在允许掩码之外的位。[Flags] 不会自动执行校验，也不会阻止 `(Permission)128`。不要给普通订单状态加 Flags 后就按位组合，不同用途需要不同规则。

## 10. 后端开发中使用 enum 的边界

适合稳定且有限的选项，如订单阶段、任务优先级和权限位。不适合频繁由运营配置的类别，或每个分支需要携带不同数据的联合类型。

传输或持久化时，明确采用数字还是名称、如何处理未知值和未来新增状态。不能把 ToString 输出当成永远稳定的外部协议，也不能让非法数值进入核心业务后才偶然触发异常。

本节应掌握：enum 是独立值类型、数值编号和零值、转换不等于校验、switch 映射，以及 Flags 的位组合与掩码校验。

## 配套示例与练习

以下将前面的语言知识应用到项目代码。命令均在 `/Users/zaijing/project/csharp` 目录执行。

### 完整示例：单一状态与可组合效果

阅读 [Example.cs](Example.cs)。CharacterState 表达一个当前状态，StatusEffects 用不同的位表达同时存在的效果。

```sh
dotnet run -- enum example
```

预期输出：

```text
状态：Attacking，底层数字：3，标签：攻击
输入 Attacking：是否合法 = True
输入 99：是否合法 = False
输入 typo：是否合法 = False
存活状态可以攻击：True
死亡状态可以攻击：False
组合效果：Burning, Poisoned，包含燃烧：True
```

CanAttack 是额外的业务判断，不是枚举自动提供的规则。`state is A or B` 是模式匹配，表示匹配任一列出的值；这里也可写 `state == A || state == B`。状态机框架不属于本课要求。

完成示例后，练习刻意换一个题材，检查你能否迁移同一套语言规则。业务场景无需掌握额外框架。

### 自己练习：工单状态

打开 [Exercise.cs](Exercise.cs)，已提供 `TicketStatus` 和验证入口。补全三个方法：

1. `GetLabel`：Unknown → 未知，Open → 待处理，Processing → 处理中，Closed → 已关闭；未定义值抛 `ArgumentOutOfRangeException`。
2. `TryParseStatus`：忽略大小写，接受名称或已定义的数字；成功输出枚举并返回 true。失败必须输出 Unknown 并返回 false。
3. `CanTransition`：只允许 Open → Processing、Processing → Closed。包括同状态、Unknown、未定义数字在内的其他组合都返回 false。

```sh
dotnet run -- enum exercise
```

检查覆盖标签、大小写、数字解析、无效输入，以及所有已定义状态之间的迁移组合。

### 自测问题

- 为什么 `TryParse` 成功还不够？
- 没写零成员时，`default` 会变成第一个成员吗？
- `[Flags]` 能阻止非法数字进入程序吗？

答案要点：可解析的数字不一定已定义；default 仍是底层零值，不保证等于第一个声明成员；Flags 只是声明组合意图，仍需自己校验。
