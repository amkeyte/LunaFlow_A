#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## IRuleCompiler Interface

Defines the interface responsible for compiling a generated rule unit into a loadable
assembly\. Compile\-time MVP is in\-memory Roslyn compilation — see the "Compiler MVP
semantics" decision in rule\-pipeline\.md — chosen specifically because the phase\-2 done
bar \(B4\) requires a generated rule to load and fire in the same test run, which a
write\-\.cs\-to\-disk\-only compiler can't satisfy without an out\-of\-process build step\.

```csharp
public interface IRuleCompiler
```

Derived  
&#8627; [RoslynRuleCompiler](../RoslynRuleCompiler/index.md 'LunaFlow\.Rules\.GeneratorServices\.RoslynRuleCompiler')

| Methods | |
| :--- | :--- |
| [Compile\(RuleCodeUnit\)](Compile(RuleCodeUnit).md 'LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler\.Compile\(LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\)') | Compiles a generated code unit into a loadable assembly\. Throws on compilation failure \(diagnostics included in the exception message\) — there is no partial\-success return value in the MVP\. |
