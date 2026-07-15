Certainly! Here's a distilled and well-formatted **Markdown version** of the output product definition and conventions for inclusion in your LunaFlow documentation:

---

# 📦 Rule Output Conventions (GeneratorServices → NRules)

This document describes the conventions and structure used to compile LunaFlow rule modules into assemblies consumable by the [NRules](https://github.com/NRules/NRules) engine.

## ✅ Output Product

The rule generation pipeline produces a compiled `.dll` containing classes compatible with **NRules.Fluent.Dsl.Rule**. These classes can be loaded directly into an `NRules.RuleRepository`.

### Example Output Rule

```csharp
public class CharacterInjuryRule : NRules.Fluent.Dsl.Rule
{
    public override void Define()
    {
        Character c = null;

        When()
            .Match<Character>(() => c, c => c.Wounds > 5);

        Then()
            .Do(ctx => c.Status = "injured");
    }
}
```

---

## 📁 Output Directory Layout

```
build/
├── MyRuleModule.dll           # Final compiled rules
├── MyRuleModule.pdb           # Debug symbols
├── Generated/                 # Optional: generated source code
│   ├── CharacterInjuryRule.cs
│   └── QuestTimerRule.cs
└── metadata.json              # Optional: rule metadata (tags, names, etc.)
```

---

## 📐 Output Conventions

| Convention        | Description                                               |
| ----------------- | --------------------------------------------------------- |
| **Namespace**     | `LunaFlow.Rules.Generated.[ModuleName]`                   |
| **Class Names**   | Follow `[Rule(Name = "...")]` or filename; must be public |
| **Base Class**    | All rules inherit from `NRules.Fluent.Dsl.Rule`           |
| **Define Method** | Must override `public void Define()`                      |
| **Constructors**  | Must include a public parameterless constructor           |
| **Attributes**    | Optional `[Rule]`, `[Tag]`, etc. add metadata for tooling |

---

## 🔗 Loading the Rules into NRules

Use standard NRules APIs to load compiled rule assemblies:

```csharp
var repository = new RuleRepository();
repository.Load(x => x.From("MyRuleModule.dll"));

var factory = repository.Compile();
var session = factory.CreateSession();
```

Rules will automatically register and participate in the engine session.

---

## 💡 Notes

* Rules must be **self-contained** in public classes with a default constructor.
* LunaFlow supports optional `.cs` generation and metadata emission for debugging.
* Attributes like `[Rule]` and `[Tag]` are not required by NRules but help LunaFlow tooling organize and document rule modules.

---

Let me know if you'd like this integrated into your existing DocFX markdown tree, or if you want a corresponding `toc.yml` entry.
