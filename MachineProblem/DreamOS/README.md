# DREAM OS — C# WINDOWS FORMS PROJECT (Claude handoff)

## Project identity
**Dream OS** is a **C# .NET Windows Forms (WinForms)** desktop application for Visual Studio on Windows. It is a *simulation of a cute Windows 95/98-style mini operating system*, NOT a real bootable OS and NOT a website. It houses the classroom Visual Basic machine-problem exercises as functional C# Windows Forms.

**Do not use React, TypeScript, HTML, CSS, JavaScript, Electron, WPF, MAUI, or webviews.** Implement with `System.Windows.Forms`, `System.Drawing`, and ordinary C# classes. Target **.NET 10 (Windows)** or the version required by the instructor; avoid third-party UI frameworks. Solution must open in Visual Studio on Windows.

## What the assets are
- `01_Design_Board`: full Dream OS pastel/Y2K concept sheet.
- `02_App_Window_Concept_Crops`: illustrated *mockup crops* for app forms, not editable controls.
- `03_Desktop_Login_Widgets`: login, desktop and widgets visual mockup crops.
- `04_Guide_Character`: original pixel girl reference and visual mockup poses.
- `05_Original_References`: original 10 exercise/reference screenshots, pink login, desktop inspirations, widget inspiration and original guide girl; refer to these for actual exercise instructions.
- `06_Documentation`: Claude instructions, architecture, this README.

**The image board is illustrative; it may have incorrect labels, figures or spelled names.** The original exercise screenshot takes precedence for rates, formulas, UI controls, and messages. Convert mockup appearance into real WinForms controls; do not overlay fake input screenshots.

## User flow
1. App launches to `LoginForm` (pink concept design); ask for a demo username/password without implying serious security.
2. Valid login displays `DesktopForm`: pastel wallpaper, shortcut icons, taskbar, Start menu, clock, and mascot guide (the pixel girl).
3. Double-click a shortcut / select in Start menu to launch one of the machine problem forms. Keep apps contained within the desktop experience using an MDI parent/child approach (or borderless `Form` hosted inside a desktop workspace `Panel`; select ONE approach and implement consistently).
4. A help character remains docked at a corner and changes its speech bubble when user moves the mouse over or focuses an icon, app window, action, or widget; basic fade/timer is optional.
5. Minimize, restore/focus, close app windows; taskbar shows running apps. Logout returns to login.

## Required machine-problem apps

| Form class | Application | Behavior |
|---|---|---|
| `AboutForm` | About Program | Application information, AUTHOR dialog, OK closes |
| `PizzaForm` | Special Pizza Builder | Pizza size, toppings, crust, drinks, dine-in/takeout, compute and clear |
| `LottoForm` | 3 Digit Lotto | Three unique digits, minimum demo bet PHP 10, random draw, jackpot/consolation results; educational simulation ONLY |
| `TuitionForm` | Tuition Fee Computation | Course, year, tuition units, lab units, fees and total |
| `PayrollForm` | Employee Payroll | Position, rate/day, days worked, civil status, SSS/tax and net pay |
| `HotelForm` | Hotel Billing System | Customer, check-in/out, room capacity/type, payment, days/charge |
| `ElectionForm` | Presidential Elections 2010 | Voter ID check, 1 vote per voter, tally and winner; classroom simulation |
| `PasswordForm` | Password Entry | Exercise-specific username/password gate to transaction; main OS login is separate |
| `ApplianceStoreForm` | Appliance Store Transaction | Product selection, quantity, unit price, subtotal, cash/charge, tendered and change |

The last two are a linked exercise: Password Entry opens the Appliance Store Transaction form, but both must remain accessible within the shell as an exercise flow.

### Rates and behavior seen in original screenshots
- Pizza: small **PHP 100**, medium **PHP 150**, large **PHP 200**; thick crust adds **50% of chosen size**; soft drink **PHP 20**, fruit juice **PHP 15**, coffee/chocolate **PHP 25**; toppings **PHP 10 each**.
- Lotto: minimum bet **PHP 10**; three digits cannot repeat within ticket; exact ordered match triggers sample **1 million pesos jackpot**; same digits different order triggers a consolation prize **bet × 1,000**, based on text in screenshot. This is a local fictional classroom activity; no real stakes.
- Tuition: **PHP 100 per unit**, **PHP 1,000 per lab unit**. Misc fee comes from course/year table in original screenshot; do not invent it.
- Payroll: position A **PHP 500/day**, B **PHP 400/day**, C **PHP 300/day**; civil-status tax Single **10%**, Married **5%**, Widowed **5%**. Use original SSS thresholds: >=10,000 7%, 5,000–9,999 5%, 1,000–4,999 3%, below 1,000 1% (clarify precise boundaries in code comments).
- Hotel: room type/capacity price matrix from screenshot; payment cash 0%, check +5%, credit card +10%; bill at least 1 day if check-in and check-out are same day.
- Appliances: unit price list in original screenshot; cash pays **subtotal less 5%**, charge pays **subtotal plus 5%**. If cash, amount tendered must cover total; show change.
- Electon/tally and auth are demonstrations only; local test data must not be presented as real public results/security.

## Architecture and OOP expectations
- WinForms project `DreamOS.WinForms` targeting `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`.
- Plain C# model/service classes in `Models/`, `Services/`, `Data/` (separated from Forms and user controls), so calculations are **not** hardcoded into click event handlers.
- Encapsulation: private fields/validated properties; abstraction: service interfaces where useful; inheritance: `RetroWindowForm : Form` and shared control types; polymorphism: `IPricingRule` or `IPaymentCalculation` implemented by cash/charge payment rule classes. **Do not force inheritance where composition is clearer.**
- Data can be in-memory plus JSON files via `System.Text.Json` under user AppData for settings, notes and demo tallies. **Database optional**; no need for PostgreSQL or SQL Server for offline coursework unless required by teacher. No plaintext real passwords; demo authentication only.
- Use `decimal` for PHP amounts and explicit rounding `MidpointRounding.AwayFromZero` when money must round; use a single reusable PHP currency formatter.

## Design implementation specifics
- Use real `Label`, `TextBox`, `Button`, `ComboBox`, `ListBox`, `RadioButton`, `CheckBox`, `NumericUpDown`, `DateTimePicker`, `PictureBox`, `Panel`, `MenuStrip`, `Timer` etc.
- Use designer-compatible `.Designer.cs` and `.resx` files as appropriate; `TableLayoutPanel`/`FlowLayoutPanel` reduce clipping.
- Pastel theme suggestions: blush `#F7C3D6`, cream `#FFF5F0`, blue title bar `#9CC9EE`, accent `#CE5E7F`, mint `#BDE3D1`, dark text `#29283D`.
- Set bitmap assets to `PictureBoxSizeMode.Zoom`, preserve aspect ratio and **nearest-neighbor interpolation** if custom painting pixel art. Preserve original guide girl's long dark hair, red headband/dress, white socks and red shoes; use original guide image when in doubt.
- Design for Windows desktop 1366×768 and support resizing and DPI scaling. Provide clear keyboard focus and tooltips. Hover handling: `MouseEnter`/`MouseLeave`, `Enter`/`Leave` and guide text map.
- App form controls must perform real business logic. Window close/minimize/taskbar controls must work; don't draw dead buttons as artwork.

## Development phases for Claude
1. Create `.sln`, WinForms `.csproj`, Program.cs and base theme, shared dialog/windows, folders, and `LoginForm`.
2. Implement DesktopForm, app launcher, Start menu, taskbar, clock, window manager and mascot guide.
3. Implement each machine-problem form and C# calculation classes.
4. Implement calendar, sticky notes, music player *UI* (play only user-supplied/local audio), reminder, system popup and settings widgets.
5. Add `System.Text.Json` persistence, input validation and automated unit tests (optional separate xUnit project) for formulas and boundaries.
6. Build/debug under Windows and Visual Studio, compare sample outputs against manual screenshots, verify keyboard/mouse interactions.

## Acceptance criteria
- `DreamOS.sln` opens in Visual Studio; `F5` starts native C# WinForms login, then desktop.
- All nine exercises launch and have working forms; other windows can be reopened, focused, minimized/closed.
- An understandable hover guide appears for every app shortcut and handles keyboard focus.
- Formula examples and invalid-input cases are tested; user sees clear `MessageBox` feedback.
- Visual styling draws on the ZIP references, but controls remain editable and accessible.
- No web stack and no fake executable behavior.

**Start here:** `06_Documentation/CLAUDE_START_HERE.md` and `06_Documentation/CSHARP_WINFORMS_ARCHITECTURE.md`.
