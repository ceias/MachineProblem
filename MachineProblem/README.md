# Dream OS ♥

A native **C# Windows Forms / .NET 10** mini desktop for an OOP classroom project. Open [DreamOS.sln](DreamOS.sln) in Visual Studio on Windows. The startup project is **DreamOS.WinForms**.

## Run on Windows

1. Install **Visual Studio 2026** with a version compatible with your .NET 10 SDK feature band and the **.NET desktop development** workload. Install a stable .NET 10 SDK. `global.json` allows stable 10.0 feature bands from 10.0.100 onward; cloud validation used 10.0.403.
2. Open `DreamOS.sln`, set `DreamOS.WinForms` as the startup project, and press **F5**.
3. Sign in with any nonempty demo username and password **`dream10`**.

From a Windows terminal in this checkout:

```powershell
dotnet build DreamOS.sln -c Release
dotnet run --project DreamOS.Tests -c Release
dotnet run --project DreamOS.WinForms -c Release
```

To create a Windows x64 framework-dependent distributable (requires the .NET 10 Desktop Runtime on the receiving Windows machine):

```powershell
dotnet publish DreamOS.WinForms -c Release -r win-x64 --self-contained false -o artifacts/win-x64
```

Distribute the **whole output folder**, including `Assets/Guide.jpg`, rather than just the executable.

## Desktop and exercises

Click a shortcut or choose a Start menu entry. Each app has one MDI child window; opening it again focuses it. Native title-bar buttons close, minimize and maximize. Taskbar buttons restore/focus apps. Start also offers cascade, tile, notifications, logout and exit. The supplied original girl-and-teddy image stays beside guidance that responds to mouse hover and keyboard focus.

| Application | Behavior |
| --- | --- |
| Special Pizza Builder | Size, crust, six toppings, drink, dining mode, itemized computation, build summary, clear toppings/all |
| 3 Digits Lotto | Unique digits, minimum PHP 10 demo bet, random nonrepeating draw, ordered jackpot and any-order consolation |
| Tuition Fees | Course/year selection, total and lab units, exact reference miscellaneous-fee table |
| Employee Payroll | Sample employee lookup, A/B/C rates, civil-status tax, SSS brackets and net pay |
| Hotel Billing | Customer, dates, all nine room rates, minimum one day, payment surcharges |
| Elections 2010 | Demo voter validation, vote confirmation, one vote per ID, persistent tally, ties/winner and confirmed reset |
| Password Entry | Username/password validation, retry/cancel, successful transition to appliance store |
| Appliance Store | Full reference catalog, quantities, multi-item cart, row removal, cash/charge totals, tendered/change, new transaction, File/Help menus |
| About + Author | Project information, author popup and close |

**Desktop Widgets** provides a live clock, calendar, saved notes, reminders, notification history and a local WAV player with play, loop and stop. Supply your own permitted audio. Reminder timers and notification history last for the current session; keep the widget window open for reminders. Notes prompt to save on close/logout/exit.

Keyboard: Tab/Shift+Tab move focus, Enter activates the login button, Space activates focused buttons; standard Windows MDI navigation applies. Both focus and hover update the guide.

## Reference rules and demo data

Calculations follow the original screenshots in `DreamOS/05_Original_References`, rather than the generated design board. PHP amounts use `decimal` and two-decimal rounding away from zero. Payroll uses continuous SSS intervals: below 1,000 = 1%; [1,000, 5,000) = 3%; [5,000, 10,000) = 5%; 10,000+ = 7%. Payroll days are limited to a 31-day pay period.

The supplied screenshots do not specify a usable password or full employee/voter roster. Both login flows therefore use the documented demo password `dream10`; employee names are sample records. Demo voter IDs are `12345`, `10001`, `10002`, `10003`. Classroom candidate labels follow the reference: Noli, Val, Lester, Gwapo. Tallies start at zero. Authentication, voting and lotto are local educational demonstrations; no real accounts, public elections, stakes or prizes are involved.

Notes and votes are stored atomically under `%LOCALAPPDATA%\DreamOS` as `notes.json` and `classroom-votes.json`. This is per Windows user, shared across demo login names. File/JSON errors are reported without silently discarding saved votes. To recover corrupt data, close the app and restore a backup or rename the affected JSON file. Demo votes can also be reset with confirmation inside Elections.

## Project structure and OOP

- `DreamOS.WinForms/`: native forms, reusable controls, theme, MDI shell, session lifecycle, local data wiring. Forms are composed in C# rather than generated Designer files; there is no web UI or third-party UI framework.
- `DreamOS.Core/`: independently testable models, calculations, catalog, auth simulation, JSON storage and election logic.
- `DreamOS.Tests/`: dependency-free executable regression suite. **Run it with `dotnet run`, not `dotnet test`**; process exit code is nonzero on failure.
- `DreamOS/`: supplied artwork, screenshots and design briefs.

Encapsulation: cart items and election state mutate through validated operations. Inheritance: `RetroForm` → `ExerciseForm` → concrete exercise forms, plus custom UserControls. Abstraction: `IPaymentRule` and `IStorage<T>`. Polymorphism: cash/charge rules implement distinct calculations behind the same interface; JSON and test storage implement the storage contract.

## Cloud development

Linux supports editing, restoring, compiling the Windows target and running the cross-platform calculation tests. **WinForms itself requires Windows**; Visual Studio, actual desktop interaction, DPI behavior and audio playback cannot be validated on this Linux host. `EnableWindowsTargeting` permits cross-compilation without changing the Windows framework target.

The cloud SDK is installed outside the checkout. Activate it in each shell:

```bash
export DOTNET_ROOT=/workspace/.tools/dotnet10
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME=/workspace/.tools/dotnet-home
export NUGET_PACKAGES=/workspace/.tools/nuget/packages
export DOTNET_CLI_TELEMETRY_OPTOUT=1
cd /workspace/DreamOS
dotnet build DreamOS.sln -c Release -m:1 --disable-build-servers
dotnet run --project DreamOS.Tests -c Release --no-build
```

Use the existing isolated checkout; no Git worktree is needed. The reusable environment installer downloads the official SDK with SHA-512 verification, restores reference packs, builds and runs the regression suite. It does not start a web service.

## Validation

Current Linux validation: .NET 10 Release solution build succeeded with **0 warnings and 0 errors**; **89 regression checks passed, 0 failed, 0 skipped**. Coverage includes screenshot examples, every tuition/room table entry, deduction boundaries, invalid inputs, draw invariants, payment change, persisted votes, duplicate prevention, ties, JSON corruption and disk-write failure.

On Windows, verify this acceptance pass before presentation:

1. Wrong/right login, logout and re-login; closing login exits.
2. Launch all nine exercises from shortcuts and Start. Minimize/restore, maximize, focus, close/reopen, cascade and tile.
3. Compute reference examples, try invalid fields, clear/reset each exercise; exercise password retry/cancel and linked transaction.
4. Cast a vote, reopen/restart to confirm persistence, reject a duplicate ID, inspect tie/winner and reset.
5. Save/reopen notes; cancel an unsaved-notes close/logout. Schedule a reminder and inspect notifications; play/loop/stop a local WAV.
6. Check keyboard navigation, guide hover/focus, resizing and 100%/150%/200% DPI. Confirm original art remains proportional.

Windows UI acceptance is **pending**, not claimed as passed by the Linux build or core tests.
