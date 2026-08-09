// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LunaFlow.EngineTests")]
[assembly: InternalsVisibleTo("LunaFlow.ContentTests")]
// Phase2_12Spike (phase-2-12-lua-worldmodule-stress-test-spec.md item 9's full-session-playthrough
// verification) needs to drive the real GameEngineCore singleton the same way
// D20SystemHandlerTests.cs does — Start()/Stop()/Submit() aren't reachable through the public
// IEngineHost surface, only through the internal GameEngineCore type itself. Same access need as the
// two test assemblies above, same minimal grant (a build-time visibility declaration only, no
// runtime behavior change).
[assembly: InternalsVisibleTo("Phase2_12Spike")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "<Pending>", Scope = "namespace", Target = "~N:LunaFlow.Engine")]
