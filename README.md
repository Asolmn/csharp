# 有 Go 经验开发者的 C# 语言学习

本项目面向约三年 Go 后端经验的开发者，目标是快速掌握 C# 语言，为后续独立学习 Unity 打基础。当前只使用 .NET 10 控制台，不包含 Unity 功能，也不保证代码可直接用于某个 Unity 版本。

建议按 **class → struct → enum → interface** 学习：先理解引用和值，再表达选项，最后通过接口组织不同实现。每章理论可独立阅读，示例展示规则，练习验证迁移能力。

## 用 Codex 生成下一课

本项目提供 [csharp-lesson skill](.agents/skills/csharp-lesson/SKILL.md)，已固定本项目的学习背景与教程要求。调用时只需指定知识点：

```text
$csharp-lesson 泛型
$csharp-lesson 委托、lambda、event
$csharp-lesson async/await
```

每个知识点会生成独立的 `README.md`、完整 `Example.cs`、带 TODO 和自动检查的 `Exercise.cs`，并接入现有运行入口与课程索引。只讲 C#；保留已填写的练习，不自动生成答案。

Skill 安装在本项目 `.agents/skills/csharp-lesson/`。若调用列表没有显示新 skill，可重启 Codex 后重新打开本项目；本地目录与发现方式见 [Codex 官方说明](https://learn.chatgpt.com/docs/build-skills)。

## 如何利用已有 Go 经验

不重学分支、循环和一般业务拆分。重点回答这些语言差异：

| 已有 Go 经验 | C# 需要重点掌握 |
| --- | --- |
| struct 值与 *T | class 引用、struct 值、默认传参和 ref 的区别 |
| 未导出字段与方法 | 访问修饰符、属性访问器、构造函数和 readonly |
| 隐式接口满足 | 声明接口关系、public 实现、接口属性与多态 |
| 类型化常量和 iota | enum 编号、默认值、转换及校验 |
| 返回 error | 异常如何抛出与捕获；不把所有业务结果都设计为异常 |

每篇标出第一遍必学内容和后续深入内容。性能机制先理解语义，不提前展开优化或复杂框架设计。

## 本轮完成标准

- 能解释一段代码修改的是原对象、值副本，还是复制后的共享引用字段。
- 能通过构造函数、属性和方法维护合法状态。
- 能让调用方使用接口替换实现，知道它与 Go 方法集满足的区别。
- 能处理未定义枚举值，区分数据合法与业务允许。
- 能通过四份 Exercise 的已有检查，并说明各项检查对应哪条语言规则。

## 示例与练习

| 知识点 | 完整示例 | 独立讲解 | 待补全练习 |
| --- | --- | --- | --- |
| class | [角色生命值与引用传递](Lessons/Class/Example.cs) | [类与引用语义](Lessons/Class/README.md) | [商品库存](Lessons/Class/Exercise.cs) |
| struct | [坐标、伤害值与浅复制](Lessons/Struct/Example.cs) | [值类型与复制](Lessons/Struct/README.md) | [不可变积分](Lessons/Struct/Exercise.cs) |
| interface | [受伤接口与多态](Lessons/Interface/Example.cs) | [契约与依赖注入](Lessons/Interface/README.md) | [结算折扣策略](Lessons/Interface/Exercise.cs) |
| enum | [角色状态与组合效果](Lessons/Enum/Example.cs) | [具名常量与状态校验](Lessons/Enum/README.md) | [工单状态流转](Lessons/Enum/Exercise.cs) |

## 各篇知识讲解覆盖什么

四篇 README 是可以独立阅读的教程：先解释概念和语法，用短代码展示规则，再进入已有完整示例和练习。

| 教程 | 理论内容 |
| --- | --- |
| [class](Lessons/Class/README.md) | 类与对象、访问修饰符、字段与属性、构造函数、静态成员、引用传参区别、null、继承与多态 |
| [struct](Lessons/Struct/README.md) | 值复制、ref/out/in、默认值与构造函数、readonly、浅复制、集合副本、装箱、可空值类型 |
| [interface](Lessons/Interface/README.md) | 契约与实现、Go 对照、接口多态、属性、多接口、显式实现、依赖注入、抽象类对比 |
| [enum](Lessons/Enum/README.md) | 底层整数、编号、零值、类型转换、解析校验、switch、Flags 位运算、后端边界 |

## 如何运行

在终端进入项目目录：

```sh
cd /Users/zaijing/project/csharp
dotnet run -- all
```

这会运行四份完整示例。没有参数时也默认运行全部示例。

```sh
dotnet run -- class example
dotnet run -- struct example
dotnet run -- interface example
dotnet run -- enum example
```

先看对应文档，再读 `Example.cs` 并预测输出。接着打开对应 `Exercise.cs`，只补全 TODO 所在方法，删除这些方法里的 `throw new NotImplementedException();`，保留已有检查代码。

```sh
dotnet run -- class exercise
dotnet run -- struct exercise
dotnet run -- interface exercise
dotnet run -- enum exercise
# 四份都做完后统一验证：
dotnet run -- all exercise
```

练习骨架本身可以编译。没有填完时会提示“练习尚未完成”；结果不符合要求时显示失败原因；正确时逐条显示“通过”。未完成或检查失败会返回非零退出码。检查通过说明覆盖的场景符合预期，不等于生产环境完整测试。

## 先认识几个公共语法

- `namespace ...;` 是文件范围命名空间，用来组织类型，不等同于 Go 的 package/目录规则。同名 `Example` 在不同命名空间中互不冲突。
- `using` 引入命名空间。本项目启用 `ImplicitUsings`，所以 `Console` 等常见类型不需要逐文件导入。
- `var` 是编译时类型推断，接近 Go 的 `:=`，不是动态类型。
- `public` 表示公开，`private` 表示只在类型内部可用，`internal` 表示只在当前程序集内可用。不是按名字首字母大小写决定可见性。
- `static` 成员属于类型，不需要创建对象；`Example.Run()` 就是静态方法调用。
- `new` 创建对象或值；`new(...)` 可由上下文推断类型。
- `$"余额：{balance}"` 是字符串插值；`100m` 是 `decimal` 字面量，金额示例使用它避免二进制浮点表示常见十进制小数时的误差。
- `=>` 在这里表示表达式形式的方法体，类似省略了 `{ return ...; }`；返回 `void` 的方法也能使用它执行一条表达式。
- `throw` 抛异常；`try/catch` 捕获异常。本练习用异常表达参数错误，区别于 Go 常见的 `(result, error)`，也可以按业务需要设计返回值表示失败。
- `nameof(amount)` 得到字符串 `"amount"`，常用于异常参数名。

`Program.cs` 是唯一入口，它采用顶级语句；其余文件通过 `Run()` 被调用，不需要各自编写 `Main`。练习检查工具在 `Lessons/Check.cs`，入门时只需关注检查条件。

## 四者如何选择

| 类型 | 主要用途 | 与 Go 的联系 |
| --- | --- | --- |
| class | 带身份、生命周期和可变状态的实体或服务 | 引用共享行为可类比 `*T`，但 C# 默认仍然按值传递引用 |
| struct | 小型、自包含、适合复制的值 | 接近 Go 的 struct 值语义 |
| interface | 抽象能力，替换实现 | Go 隐式满足；C# 通常需声明 `: Ixxx` |
| enum | 有限选项、状态或位标志 | 接近自定义整数类型配合常量、`iota` |

示例中的角色与伤害是普通 C# 数据逻辑。练习保留库存、积分、折扣和工单等熟悉场景，目的是检验跨场景运用语言知识的能力。

## 四个类型之后的 C# 学习方向

后续可在独立章节依次学习：泛型与集合、委托与 lambda、event 与订阅/取消订阅、异常与 IDisposable、迭代器与 IEnumerable、async/await 与取消、基础分配与装箱分析。这是路线说明，本轮仍只完成四个类型，不引入额外依赖或游戏引擎功能。
