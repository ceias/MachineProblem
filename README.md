# MachineProblem — Dream OS

Open **MachineProblem.slnx** in Visual Studio 2026 with .NET 10 support and the **.NET desktop development** workload. Set **MachineProblem** as the startup project and press **F5**. Use any demo username and password `dream10`.

`MachineProblem/MachineProblem.csproj` builds the native WinForms sources under `DreamOS.WinForms` and references `DreamOS.Core` as a separate library. It intentionally excludes the original template `Program.cs` / form and the nested test sources. These original files remain on disk. Do not add all nested C# files to the outer project: doing so creates conflicting namespaces and multiple entry points.

Alternatively, open `MachineProblem/DreamOS.sln` and start **DreamOS.WinForms**. Both entry projects use the same desktop sources.

```powershell
dotnet build MachineProblem.slnx -c Release
dotnet run --project MachineProblem/MachineProblem.csproj
dotnet run --project MachineProblem/DreamOS.Tests -c Release
```

See [the full Dream OS guide](MachineProblem/README.md) for the nine exercises, reference formulas, saved-data paths, OOP architecture, widgets and Windows acceptance checks. Linux can cross-compile this Windows target and run core regression checks; desktop interaction requires Windows.
