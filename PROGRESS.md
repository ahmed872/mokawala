# سجل حالة المشروع — نظام إدارة الحركة (جوميكس)

> **الغرض من الملف:** البيئة السحابية بتتصفّر بين الجلسات، وأي جلسة جديدة كانت بتبدأ من الصفر وتحرق وقت وكريديتس في إعادة الاستكشاف. الملف ده هو الذاكرة: اقرأه أول الجلسة، وحدّثه مع كل push.

---

## Git

- **البرانش الحالي للشغل:** `claude/awesome-pasteur-66v84o`
- **الأساس:** المشروع كله تحت `FleetManagementSystem_CSharp_WPF/` (سوليوشن .NET 8: Core / Data / Services / WPF / Tests).
- ادفع دايمًا بـ `git push -u origin claude/awesome-pasteur-66v84o`.

## الحالة الحالية (آخر تحديث: 2026-07-06)

### مُنجَز ومرفوع

1. **حسابات أصحاب العهدة:** أي عهدة جديدة بتنشئ حساب دخول تلقائي للمستلم (username = الاسم، الباسورد الافتراضي = username، مع إجبار تنبيه لتغييره أول دخول). الدور الجديد: `UserRole.CustodyHolder` — يشوف تبويب العهد فقط وعهدته هو فقط، ويقدر يعمل تصفية بس (لا إضافة/تعديل/حذف).
   - `CustodyService.EnsureCustodianUserAsync` في `OperationalServices.Part2.cs`
2. **تصفية العهدة:** `CustodySettlementWindow` (تصفية جزئية/كاملة). المبلغ المصفَّى يرجع الخزينة "إيراد" تلقائيًا (داخل `CustodyService.SettleAsync`)، والعهدة تتعلم "مرتجعة" عند اكتمال التصفية. جدول العهد فيه أعمدة: قيمة العهدة / تم تسديده / المتبقي / حساب الدخول + زر "تصفية".
3. **العهدة أصبحت باسم المسئول والعربية اختيارية:** `Custody.VehicleId` بقى nullable؛ `CustodyEntryWindow` معاد تصميمها: "المستلم" ComboBox قابل للكتابة من قائمة الموظفين (مع ملء الوظيفة تلقائيًا)، والعربية حقل اختياري بزر "بدون عربية". الترحيل للداتابيز القديمة في `EnsureCustodyVehicleOptionalAsync` (MySQL: ALTER؛ SQLite: إعادة بناء الجدول).
4. **صفحة المستخدمين Modal:** `UserEntryWindow` بدل التعديل داخل الجدول؛ الأدوار بأسماء عربية؛ باسورد فاضي عند الإنشاء = اسم المستخدم.
5. **تغيير كلمة المرور من الداخل:** `ChangePasswordWindow` + زر في كارت المستخدم بالصفحة الرئيسية + سؤال تلقائي بعد الدخول لمن معه الباسورد الافتراضي (`MustChangePassword`).
6. **سجل غيابات السائق:** رجع يظهر — ScrollViewer حوالين تبويب "حضور وغياب" (`AttendanceTabScrollViewer` في `MainWindow.xaml`) + BringIntoView + رسالة لو مفيش نتائج.
7. **تعديل قيمة العهدة يسجل الفرق فقط في الخزينة** (مش خصم كامل المبلغ في كل حفظ).
8. **اللوجو:** fallback دائم للوجو المدمج `pack://application:,,,/Resources/gomix-logo.png` لو مسار الإعدادات فاضي/بايظ (`UpdateCompanyLogo` + تصحيح تلقائي في `DataBootstrapService`).
9. **ترقية مخطط قاعدة البيانات تلقائيًا** أول تشغيل عبر `EnsureOperationalSchemaAsync` (أعمدة: Custody.Amount/SettledAmount/SettlementDate/SettlementNotes/UserId، Users.MustChangePassword + جعل Custody.VehicleId اختياري) — تعمل مع MySQL وSQLite.
10. **إصلاح قواعد البيانات القديمة (2026-07-06):** جهاز أحمد فيه SQLite بجدول عهد قديم ناقص أعمدة أساسية (كان بيدي `no such column: c.CustodianName` عند الدخول و`error while saving` عند الحفظ). الحل: `EnsureCustodyTableAsync` ينشئ الجدول لو مش موجود + فحص كل أعمدة العهد عمودًا عمودًا + إعادة بناء جدول SQLite لو VehicleId كان NOT NULL. اتعملت محاكاة كاملة للسيناريو ونجحت.
11. **تحميل الأقسام أصبح مرنًا:** `RefreshAllAsync` في `MainWindow.xaml.cs` بيكمل باقي الأقسام لو قسم فشل ويعرض رسالة واحدة بأسماء الأقسام الفاشلة — ده كان سبب اختفاء اللوجو (فشل العهد كان بيوقف تحميل الإعدادات واللوجو اللي بعده).
12. **تبويب "عهدتي" (2026-07-06):** تبويب Tag="MyCustody" ظاهر لكل مستخدم مسجل أيًا كان دوره (Admin/خزينة/صاحب عهدة...) — يعرض العهد المسجلة باسمه (ملخص: إجمالي/مسدد/متبقي + جدول + زر تصفية). ده حل مشكلة "اديت لمحمود (Admin) عهدة ومالاقاهاش لما دخل بحسابه". `MyCustodyTab` في MainWindow.xaml + `LoadMyCustodyAsync` (بيتنده أيضًا من داخل LoadCustodyAsync). دور CustodyHolder بقت وحدته "MyCustody" بدل "Custody".
13. **UI: جدول المستخدمين** اتشال من التوسيع التلقائي للأعمدة (`GetMainDataGrids`) عشان عمود الحالة كان بيتقص بره الشاشة؛ عمود البريد أصبح Width="*".
    - **تنبيه مهم للنوافذ الجديدة:** أي Window مستقلة تستخدم StaticResource لازم يكون المورد معرفًا في **App.xaml** (المتاح: PageBackgroundBrush/Surface*/Accent*/Text/Muted/Border + AppTextBoxStyle/AppPasswordBoxStyle/AppComboBoxStyle + Primary/Secondary/Danger/SoftButtonStyle). ستايلات MainWindow (مثل FilterDatePickerStyle وTripPageCardStyle) **غير متاحة** خارجها — استخدامها في نافذة مستقلة يعمل **كراش فوري** عند فتحها. ده كان سبب قفل البرنامج عند "تصفية العهدة" (اتصلح 2026-07-06) + اتضافت DispatcherUnhandledException في App.xaml.cs كشبكة أمان.
15. **اعتماد مرتجع العهدة (2026-07-06):** حركة الخزينة لها `Status` ("معتمد" افتراضي / "معلق"). مرتجع تصفية العهدة بيتسجل "معلق" ولا يدخل الرصيد ولا الإجماليات حتى يعتمده مسؤول الخزينة أو المدير من زر "اعتماد" في تبويب الخزينة (بيظهر فقط للحركات المعلقة). `TreasuryService.ApproveAsync` + عمود Status في TreasuryTransactions (EnsureColumn) + استثناء المعلق من `GetCurrentBalanceAsync` وداشبورد `GetDashboardMetricsAsync` و`UpdateTreasurySummary`. تعديل حركة معلقة لا يعتمدها ضمنيًا.
14. **مستلم العهدة بقى من مستخدمي النظام:** قائمة "المستلم" في نافذة العهدة بتعرض المستخدمين المفعّلين (مش الموظفين) — `BuildCustodianOptions` في MainWindow + `CustodyFormDto.UserId`. اختيار مستخدم موجود يربط العهدة بحسابه مباشرة؛ كتابة اسم جديد تنشئ حساب تلقائي. حفظ الحساب والعهدة في SaveChanges واحدة.

### معلَّق / أفكار لم تُطلب بعد

- (فاضي حاليًا — أضِف هنا أي طلب جديد من أحمد قبل تنفيذه)

### مشاكل معروفة (لا تحاول إصلاحها ضمن شغل آخر)

- التست `SmokeTests.DriverAttendanceService_SavesWeeklyAttendance_ReportsRestBalance_AndAlertsExpiringLicense` **فاشل من قبل أي تعديلات** (فشل موجود على HEAD الأصلي). النتيجة المتوقعة: 19/20 ناجح.
- `AllServices.cs` و`AppStartup.cs` وملفات `Views/` و`ViewModels/` **غير مُجمَّعة** (csproj بـ `EnableDefaultItems=false`) — متضيعش وقت فيها، هي كود قديم مرجعي.

## تجهيز بيئة العمل (لينكس، أول الجلسة)

```bash
# 1) SDK (مصادر مايكروسوفت محجوبة عبر البروكسي — استخدم أوبونتو)
apt-get update; apt-get install -y dotnet-sdk-8.0

# 2) بناء الباك-إند + التستات (19/20 هو النجاح المتوقع)
cd FleetManagementSystem_CSharp_WPF
dotnet build FleetManagementSystem.Tests/FleetManagementSystem.Tests.csproj
dotnet test  FleetManagementSystem.Tests/FleetManagementSystem.Tests.csproj

# 3) التحقق من مشروع الـ WPF (لا يُبنى على لينكس — الأداة دي بديل موثوق)
python3 tools/wpf_check.py     # لازم تطبع STUB COMPILE PASSED
# + فحص XAML: xmllint --noout على أي XAML اتعدل
```

ملاحظات بيئة مهمة:
- حزمة أوبونتو للـ SDK **ناقصة WindowsDesktop targets** — بناء `FleetManagementSystem.WPF.csproj` مباشرة هيفشل حتى مع `EnableWindowsTargeting`. استخدم `tools/wpf_check.py` (بيولّد stubs لـ InitializeComponent/x:Name/الأحداث ويجمّع كل كود الواجهة ضد ref packs من nuget.org).
- **قيد مهم في `wpf_check.py`:** الأداة بتفحص كود C# والأحداث و x:Name، لكنها **لا تكمبايل XAML markup** فمابتمسكش أخطاء XAML زي `MC3024` (خاصية اتعرّفت مرتين، مثلاً `Style="..."` مع `<Button.Style>` جوا نفس العنصر). قبل أي push فيه تعديل XAML شغّل الفحص اليدوي المضاف في `tools/xaml_lint.py` (بيمسك الخصائص المكررة + well-formedness). حصل غلط زي ده اتصلح 2026-07-06 في زرار اعتماد الخزينة.
- `builds.dotnet.microsoft.com` و`dotnetcli.azureedge.net` محجوبين؛ `nuget.org` و`archive.ubuntu.com` شغالين.
- عند إضافة نافذة WPF جديدة: **لازم تسجلها يدويًا في csproj** (Page + Compile) لأن `EnableDefaultItems=false`.

## قواعد العمل مع أحمد (صاحب المشروع)

- التواصل بالعربية المصرية، ونصوص الواجهة كلها عربية (RTL).
- **push بعد كل جزء مكتمل** — الجلسات بتقف فجأة (حد الاستخدام كل 5 ساعات) وأي شغل غير مرفوع بيضيع.
- حدّث هذا الملف (قسم "الحالة الحالية" والتاريخ) في نفس الكوميت مع أي شغل جديد.
- بعد الـ push قول لأحمد خطوات السحب والاختبار:
  ```
  git fetch origin
  git checkout claude/awesome-pasteur-66v84o
  git pull origin claude/awesome-pasteur-66v84o
  ```
  ثم يبني ويشغل من Visual Studio على ويندوز. قاعدة البيانات بتترقى لوحدها أول تشغيل.
