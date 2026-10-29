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

Click a shortcut or choose a Start menu entry. Each app has one child form hosted in the desktop workspace; opening it again focuses it. Custom blue title-bar buttons close, minimize and maximize; drag the title to move a window and the bottom-right grip to resize it. Taskbar buttons restore/focus apps. Start also offers cascade, tile, notifications, logout and exit. The supplied original girl-and-teddy image stays beside guidance that responds to mouse hover and keyboard focus.

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

**Desktop Widgets** provides a live clock, calendar, saved notes, reminders, notification history and a local audio player with play, pause, loop and stop. Supply your own permitted audio. Reminder timers and notification history last for the current session; keep the widget window open for reminders. Notes prompt to save on close/logout/exit.

Keyboard: Tab/Shift+Tab move focus, Enter activates the login button, Space activates focused buttons; Enter and Space activate focused controls. Both focus and hover update the guide.

## Reference rules and demo data

Calculations follow the original screenshots in `DreamOS/05_Original_References`, rather than the generated design board. PHP amounts use `decimal` and two-decimal rounding away from zero. Payroll uses continuous SSS intervals: below 1,000 = 1%; [1,000, 5,000) = 3%; [5,000, 10,000) = 5%; 10,000+ = 7%. Payroll days are limited to a 31-day pay period.

The supplied screenshots do not specify a usable password or full employee/voter roster. Both login flows therefore use the documented demo password `dream10`; employee names are sample records. Demo voter IDs are `12345`, `10001`, `10002`, `10003`. Classroom candidate labels follow the reference: Noli, Val, Lester, Gwapo. Tallies start at zero. Authentication, voting and lotto are local educational demonstrations; no real accounts, public elections, stakes or prizes are involved.

Notes and votes are stored atomically under `%LOCALAPPDATA%\DreamOS` as `notes.json` and `classroom-votes.json`. This is per Windows user, shared across demo login names. File/JSON errors are reported without silently discarding saved votes. To recover corrupt data, close the app and restore a backup or rename the affected JSON file. Demo votes can also be reset with confirmation inside Elections.

## Project structure and OOP

- `DreamOS.WinForms/`: native forms, reusable controls, theme, desktop window host, session lifecycle, local data wiring. Forms are composed in C# rather than generated Designer files; there is no web UI or third-party UI framework.
- `DreamOS.Core/`: independently testable models, calculations, catalog, auth simulation, JSON storage and election logic.
- `DreamOS.Tests/`: dependency-free executable regression suite. **Run it with `dotnet run`, not `dotnet test`**; process exit code is nonzero on failure.
- `DreamOS/`: supplied artwork, screenshots and design briefs.

Encapsulation: cart items and election state mutate through validated operations. Inheritance: `RetroForm` → `WorkflowForm` → concrete exercise forms, plus custom UserControls. Abstraction: `IPaymentRule` and `IStorage<T>`. Polymorphism: cash/charge rules implement distinct calculations behind the same interface; JSON and test storage implement the storage contract.

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

Current Linux validation: .NET 10 Release solution build succeeded with **0 warnings and 0 errors**; **106 regression checks passed, 0 failed, 0 skipped**. Coverage includes screenshot examples, every tuition/room table entry, deduction boundaries, invalid inputs, draw invariants, payment change, persisted votes, duplicate prevention, ties, JSON corruption and disk-write failure.

On Windows, verify this acceptance pass before presentation:

1. Wrong/right login, logout and re-login; closing login exits.
2. Launch all nine exercises from shortcuts and Start. Minimize/restore, maximize, focus, close/reopen, cascade and tile.
3. Compute reference examples, try invalid fields, clear/reset each exercise; exercise password retry/cancel and linked transaction.
4. Cast a vote, reopen/restart to confirm persistence, reject a duplicate ID, inspect tie/winner and reset.
5. Save/reopen notes; cancel an unsaved-notes close/logout. Schedule a reminder and inspect notifications; play/pause/loop/stop a local MP3 and WAV.
6. Check keyboard navigation, guide hover/focus, resizing and 100%/150%/200% DPI. Confirm original art remains proportional.

Windows UI acceptance is **pending**, not claimed as passed by the Linux build or core tests.

## Supplied artwork and interface

The UI uses the original provided PNG artwork rather than a flat placeholder screen. Both entry projects copy the login, desktop, widgets, pizza, lotto, tuition and hotel PNGs plus the original guide JPEG into `Assets`. The supplied mockups are flattened images containing text and fake fields; `Controls/Artwork.cs` selects only decorative regions (heart badge, computer and app icons). The continuous sky and cloud banks are drawn in GDI+ instead of stretching strips of the flattened desktop screenshot. Input fields, menus, title-bar actions and calculations remain real WinForms controls. The original image files are unchanged.

The login follows the centered heart/title/input layout. The desktop has cloud wallpaper, pixel shortcuts, a welcome window, a bottom Start/taskbar/clock and the girl with a speech bubble. Shared exercise forms use blue title bars, beveled pink buttons, cream fields, an app illustration and a separate results panel. The guide background is masked during rendering; it no longer appears in an opaque white rectangle.

All app windows consistently use borderless Forms hosted in one workspace Panel, replacing the earlier MDI implementation so their title bars can match the reference artwork. Minimize hides a window until its taskbar button is selected; maximize fills the workspace; restore returns its prior bounds.

Windows visual validation is still required: check the supplied reference crops alongside the running login/desktop, hover and focus guidance, title-bar close/minimize/maximize/restore, window dragging and resizing, logout cancellation with unsaved notes, and layout at 100%, 150% and 200% DPI. Compilation and the core tests do not validate rendered appearance on Linux.

### Desktop rendering refinement

The shortcut buttons and their shelf now paint the cached workspace backdrop at their own coordinates, instead of relying on nested transparent WinForms controls. This addresses the repeated miniature-window backgrounds visible in the earlier Windows screenshot. Labels have separate pale caption strips, predictable icon sizes and keyboard focus outlines.

Caption buttons draw their minimize/maximize/close marks as lines and rectangles, avoiding font clipping and ellipsis. The welcome card has a two-column illustration/content layout, and the right-hand date/time card opens the existing widgets. Taskbar item heights leave room for a horizontal scrollbar when many apps are open. The guide bubble and character paint together over the cached backdrop.

Windows visual acceptance remains pending: specifically inspect shortcut backgrounds after opening/moving/minimizing windows, scrolling the shelf, tabbing through shortcuts, resizing the workspace and changing DPI. Validate caption actions and the calendar-card launch. The Linux build and calculation tests cannot establish that Windows painting matches the design.

## Redesigned exercise windows

The nine exercise popups now use `WorkflowForm` with grouped input areas and fixed action buttons. The previous generic scrolling form and decorative results sidebar are no longer used by the exercise windows.

- **Pizza:** illustrated size, crust, topping and drink cards; multiple topping selections; dining mode; live price hint and itemized order slip.
- **Lotto:** three large digit steppers, quick demo-bet buttons, separate winning-number balls and a clearly labeled fictional ticket.
- **Tuition:** enrollment and study-load groups, an aligned miscellaneous-fee table and an assessment slip.
- **Payroll:** employee profile, pay period, days worked and a payslip separating gross earnings, SSS, tax and net pay.
- **Hotel:** guest/contact details, stay dates, room category/capacity, payment, live duration/rate estimate and a guest folio. This remains a classroom billing system; it does not track room inventory or confirm reservations.
- **Appliances:** searchable illustrated product cards replace the product dropdown. Quantity applies per card click. The cashier shows a selectable item grid, payment method, tendered cash, total, remaining cash needed and change. Complete sale validates tendered cash before generating a receipt. Changing the basket/payment invalidates the previous receipt.
- **Election:** ballot-entry and live-tally panels plus a vote confirmation slip; the one-vote-per-ID rules remain intact.
- **Password / About:** compact layouts without unnecessary invoice sidebars.
- **Widgets:** side-by-side calendar/reminder tools, an enlarged note editor and a music transport panel. Notes, Music and Remind launchers plus a saved-note preview are visible next to the desktop guide; saving notes updates that preview.

The new 30-sprite atlas is documented in `DreamOS.WinForms/Assets/README.md`. Both application projects copy it to the build/publish output. The original supplied artwork is preserved.

Receipt panels support **Save slip** (text) and **Print** (Windows print dialog; use Microsoft Print to PDF if installed). They wrap long names/items, scroll independently, and remain disabled until a current result exists. Payroll and tuition documents are statements, hotel folios are estimates, and lotto/vote slips are explicitly classroom simulations. There is no claim that a real payment or booking occurred.

Run the regression suite with `dotnet run --project DreamOS.Tests -c Release`: it now includes receipt data/export checks in addition to the original formula, validation and persistence tests. Windows visual acceptance and real print/audio device validation remain necessary; a Linux cross-build does not verify actual WinForms rendering.

## Branded receipts, icon cards and interactive guide

All receipt previews, PNG exports and printed pages now share `ReceiptDesign`: a white Dream OS paper slip, pink wordmark, mini pixel-art strip, aligned item/value columns, dashed total divider, decorative barcode-style footer and serrated paper edge. Save slip defaults to a high-resolution PNG; text remains an optional format. Print sends this same styled layout to the Windows printer, centered at up to 3.15 inches wide. Long receipts continue across pages with repeated headers; all rows and the final total are retained. Printer margins/paper selection still depend on the Windows driver. The barcode graphic is decorative and is not a scannable payment code.

`WorkflowForm` banners and input sections use bordered icon cards. Login, welcome, notes/music panels and custom message dialogs use the same card styling. Native file and printer selection dialogs remain the operating system's dialogs.

The guide can be dragged within the desktop workspace. Its dragged position survives window opens and workspace resizing for the current session. Right-click to pause/resume animations or return home; keyboard users can focus the guide, use Control+arrow keys to move, Control+Home to return, and Space to pause/resume. Hovering or focusing an app triggers a short, distinct movement and matching icon prop: pizza toss, lotto hop, study sway, payslip tilt, hotel greeting, ballot wave, password peek, shopping bounce, heart sway or widget dance. Alerts trigger a short notification bounce. These animate the original supplied girl rather than replacing her design.

Notifications, validation errors, vote/reset confirmations, password retry/cancel, author information and unsaved-note prompts use `DreamDialog`. Enter and Escape have explicit button behavior; saving notes remains the default in its Save/No/Cancel prompt. The dialog close button never silently confirms a destructive action.

Validation: both solutions build on .NET 10 with zero warnings/errors; 97 cross-platform checks pass, including bounded/paused guide motion and complete, terminating receipt pagination. Windows visual rendering, guide input interactions, actual printer/PDF-driver output and dialog keyboard behavior still require Windows acceptance testing.

## Audio playback and invalid WAV files

Music now uses pinned NAudio 2.2.1 with Windows decoding/output rather than `SoundPlayer`. The picker accepts MP3, WAV, M4A/AAC, WMA and AIFF; exact codec availability depends on Windows. Play resumes, Pause retains position, Stop resets to the beginning, and Loop reads continuously across the end of the recording. Closing the widget releases the audio device and file.

File contents are checked independently of the extension, and decoder/output initialization must succeed before the track name or playback buttons change. A failed selection leaves the previous working track intact. Late playback/device failures are reported through the custom dialog on the UI thread. Renamed HTML, WebM/Ogg downloads and empty/truncated headers are recognized as unsupported rather than treated as valid WAV audio.

If a recording still cannot load, export an actual MP3 or PCM WAV (16-bit, 44.1 kHz) with an audio converter/editor; renaming the extension is insufficient. No music or external audio is bundled.

Current cross-platform validation: 106 checks pass, including audio-container signature and mislabeled-file cases. Both .NET 10 entry projects compile. Actual Windows codec/device playback and the user's specific recording cannot be validated on this Linux host. On Windows check MP3, PCM WAV, a converted/compressed recording, pause/resume, end-of-track looping, Stop, failed-file selection with an existing track, and closing the widget while playing.
