# Codex instructions — Dream OS (C# Windows Forms)

You are implementing a **Windows desktop application in C#**, not a web application. Do not create React, TypeScript, HTML, CSS, Electron, or web-based UI.

## Project goal
Build Dream OS, a cute Y2K / Windows 95-inspired mini desktop in **C# Windows Forms using .NET 10** (Windows target). A login screen leads to a desktop shell with launchable machine-problem programs and retro widgets. The girl from `04_Guide_Character/Guide_Original_Reference.jpg` is an on-screen guide that displays context-sensitive speech when users hover app shortcuts or windows.

## Required programs
- Special Pizza Builder (size, crust, toppings, drinks, totals)
- 3 Digits Lotto (bet, validation, win rules; reference is an educational simulation, no real-money gambling)
- Tuition Fee Computation
- Employee Payroll Computation
- Hotel Billing System
- Presidential Elections 2010 (mock classroom voting only)
- Password Entry/Login
- Appliance Store Transaction/POS
- About Program and Author popup

Read `README.md`, `06_Documentation/CSHARP_WINFORMS_ARCHITECTURE.md`, and the original task screenshots in `05_Original_References/` before implementing calculations. Treat generated UI crops as visual *references*, not source code; when UI mockup figures contradict lab screenshots, the lab screenshots govern behavior. Never hardcode sample results as business logic.

## Engineering
- Solution: `DreamOS.sln`; app project: `DreamOS.WinForms/DreamOS.WinForms.csproj`.
- Target `net10.0-windows`, `UseWindowsForms=true`.
- Favor composable UserControls, models, and services; follow the four OOP pillars in meaningful code, not cosmetic class names.
- Use built-in System.Drawing + WinForms for retro window chrome and desktop icons.
- Include validation, recoverable errors, proper currency/date formatting, and app state reset.
- Keep desktop navigation usable with mouse and keyboard; hover descriptions must also be available through focus for accessibility.
- Include a `dotnet build` validation step and document anything that could not be tested on the current host.
- Create a clear launch flow (login -> desktop -> apps), with no dead buttons or nonfunctional menu items.
- No copyrighted media playback; use original or user-provided assets only.
- The root solution now contains the implementation, core services, and executable regression suite; the DreamOS subdirectory retains the original reference assets and briefs.

## Implementation sequence
1. Make the solution and bootable WinForms app.
2. Build retro theme system, login, desktop window management, shortcuts, taskbar, icons, and guide speech.
3. Implement each machine problem with isolated calculation/business services.
4. Implement widget suite (clock, calendar, notepad, alerts, media UI using user-provided files only).
5. Add tests for computational services and finish build / usage instructions.
