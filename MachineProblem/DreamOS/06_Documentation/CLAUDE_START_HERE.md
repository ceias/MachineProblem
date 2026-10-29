# COPY THIS PROMPT TO CLAUDE — DREAM OS C# WINFORMS

You are an experienced **C# .NET 10 Windows Forms developer and desktop UI designer**. Build my school project **DREAM OS**, a *native Windows desktop mini-OS simulation* inspired by Windows 95/98 and the attached pink/blue Y2K pixel UI assets. This is **NOT A WEB PROJECT**. Do not use React, TypeScript, HTML, CSS, JavaScript, Electron, WPF, or MAUI. Use **C# Windows Forms in Visual Studio**, `System.Windows.Forms`, `System.Drawing` and plain C# OOP.

FIRST read `README.md`, `06_Documentation/CSHARP_WINFORMS_ARCHITECTURE.md`, the `05_Original_References/` screenshots for exact exercise requirements, and the image concepts. The generated design board is ART REFERENCE ONLY; original manual screenshots determine the correct machine-problem rules. Preserve uploaded chibi girl's recognizable design: long black hair, red headband, red dress, white socks/red shoes and small teddy-bear companion.

Build a real `.sln` / `.csproj` solution for `net10.0-windows` (`UseWindowsForms=true`), with files organized in `Forms/`, `Controls/`, `Models/`, `Services/`, `Data/`, `Assets/` and `Themes/`. Build a pink **LoginForm**, a **DesktopForm** with wallpaper, desktop shortcuts, taskbar, Start menu, clock, opening/minimizing/restoring/focusing forms, and a mascot walkthrough who displays context-aware text on `MouseEnter` and keyboard `Enter` / focus. Use an MDI shell or a consistent child-form hosting approach; don't show unrelated loose windows all over the screen.

Implement NINE functional machine-problem windows:
1. About Program + Author popup
2. Special Pizza Builder
3. 3 Digit Lotto (fictional educational simulation)
4. Tuition Fee Computation
5. Employee Payroll Computation
6. Hotel Billing System
7. Presidential Elections 2010 (fictional classroom simulation)
8. Password Entry (opens appliance store as the original exercise)
9. Appliance Store Transaction

Separate calculations into C# services/models, use `decimal` for prices, reusable input validation and clear/reset handling, and demonstrate all four OOP pillars naturally. Add themed widgets: calendar, notes, clock, notification, music-player UI (local or permitted audio only), reminders, and popups. Store notes/settings/local demo votes with `System.Text.Json` when helpful; do not require a server or database. Login is for local demo and not robust authentication.

**Deliver actual compilable source code, not just an illustration or pseudocode**: a complete Visual Studio solution, all Forms (including Designer files if used), necessary asset paths and resource configuration, and step-by-step instructions to build/run. Build in incremental stages but keep the project compilable. Verify exact rate tables from the reference screenshots; when text cannot be read, state that clearly instead of fabricating requirements. Do not replace the guide character with an unrelated avatar or distort it. Do not turn screenshots into clickable backgrounds; use actual WinForms controls.

Start by outputting the proposed folder/file tree, architecture and working Visual Studio scaffold, then implement the desktop and app forms with real event handlers, before adding polish.
