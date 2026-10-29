# C# WINFORMS IMPLEMENTATION BLUEPRINT

## Native technology
- Visual Studio 2026 with .NET 10 support, Windows, C# and Windows Forms App targeting `net10.0-windows`.
- Avoid web frameworks and third-party UI kits unless instructor permits.
- Project is a visual simulation of an OS inside one native Windows app, not a kernel or virtual machine.

## Suggested solution tree
```text
DreamOS.sln
DreamOS.WinForms/
  DreamOS.WinForms.csproj
  Program.cs
  Forms/
    LoginForm.cs
    DesktopForm.cs
    AboutForm.cs
    PizzaForm.cs
    LottoForm.cs
    TuitionForm.cs
    PayrollForm.cs
    HotelForm.cs
    ElectionForm.cs
    PasswordForm.cs
    ApplianceStoreForm.cs
  Controls/
    RetroTitleBar.cs
    RetroButton.cs
    DesktopShortcut.cs
    TaskbarControl.cs
    GuideCharacterControl.cs
    GuideSpeechBubble.cs
    StickyNoteControl.cs
    CalendarWidget.cs
    MediaPlayerWidget.cs
  Models/
    PizzaOrder.cs
    LottoTicket.cs
    Enrollment.cs
    Employee.cs
    HotelReservation.cs
    Voter.cs
    Candidate.cs
    ApplianceProduct.cs
    CartItem.cs
  Services/
    WindowManager.cs
    PizzaPricingService.cs
    LottoService.cs
    TuitionService.cs
    PayrollService.cs
    HotelBillingService.cs
    ElectionService.cs
    ApplianceCheckoutService.cs
    GuideHelpService.cs
    JsonStorageService.cs
  Data/
    SampleCatalogs.cs
    RateTables.cs
  Themes/
    RetroPalette.cs
    RetroTheme.cs
  Assets/
    GirlOriginal.png
    Wallpaper.png
    AppIcons/
  Properties/
    Resources.resx
DreamOS.Tests/   (optional calculation unit tests)
```

## Form hosting strategies
**Recommended:** `DesktopForm.IsMdiContainer = true` and machine-problem forms with `MdiParent = desktopForm` for built-in focus/close/minimize. Customize the desktop using panels, an overlay guide, taskbar and a Start menu. MDI backgrounds can have rendering limits; if desktop visuals must be behind floating windows, consider manually hosting borderless child Forms inside a `Panel` instead. Be consistent; do not mix modes.

## Hover guide
`GuideHelpService` maps IDs such as `app.pizza`, `app.lotto`, `app.tuition`, `app.payroll`, `app.hotel`, `app.election`, `app.appliance`, `app.password`, `app.about` and `widget.calendar` to brief descriptions. `GuideCharacterControl` holds a `PictureBox` showing the original pixel girl and a `GuideSpeechBubble`. Register both mouse hover and keyboard focus via `MouseEnter`/`MouseLeave`, `Enter`/`Leave`. Keep image aspect ratio, guide position non-obstructive, and avoid a message box on every hover.

## OOP mapping
- Encapsulation: validated properties in `PizzaOrder`, `LottoTicket`, `Employee`, `HotelReservation`.
- Abstraction: `IPriceCalculator`, `IPaymentRule`, `IStorageService` where advantageous.
- Inheritance: `RetroForm : Form`, reusable `UserControl` subclasses for cards, shortcuts, widgets.
- Polymorphism: `CashPaymentRule` and `ChargePaymentRule` implement `IPaymentRule`; `Calculate(decimal subtotal)` varies based on payment selection.

## Concrete UI design
Forms should be real Windows Forms designer objects. Use `Label`, `TextBox`, `NumericUpDown`, `ComboBox`, `ListBox`, `RadioButton`, `CheckBox`, `Button`, `DateTimePicker`, `PictureBox` and `Panel`; `Timer` for clock and transitions. Each is styled with blush/cream backgrounds, blue title bars, pixel-inspired borders and simple custom drawing when necessary. Favor standard readable fonts for form content. Do not try to use the full design board as one giant bitmap UI.

## Testing checklist
- Login demo success / failure feedback; valid desktop launch; logout.
- All nine app icons and Start menu entries launch; window close and reopen; focus/taskbar.
- Pizza thick crust +50% of base, topping cost PHP 10 each, correct drinks.
- Lotto bet < PHP 10 rejected; repeated digits rejected; equal order / different order / no-match conditions.
- Tuition missing course/year rejected, fees table read from original.
- Payroll computation brackets and deduction signs, `NetPay = BasicPay - (SSS + Tax)`.
- Hotel date validation, same-day = 1 day, room rate + payment surcharge.
- Election one simulated vote per ID, tally and winner, no real elections claim.
- Appliance quantity >0, cash discount vs charge surcharge, adequate cash tendered, correct change.
- Hover/focus guide tooltip mapped and visible; clock and calendar work.
- Visual Studio compiles on Windows; high-DPI layout is usable.

## Notes on source screenshots
The concept-art crop may contain invented or incorrectly rendered labels, amounts and names. Always rely on original captured PDF pages when assigning business rules. If there is no actual PDF in the ZIP, request it or state which rates are uncertain. Use new author/project credit rather than falsely claiming the original example's name is the project's author.
