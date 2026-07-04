using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    // ══════════════════════ EMPLOYEES LIST ══════════════════════
    public class EmployeesForm : BaseListForm
    {
        private List<Employee> _all = new();
        public EmployeesForm() : base("الموظفين") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Employees.OrderBy(e => e.Code).ToList();
            var now = DateTime.Today;

            var att = db.EmployeeAttendances
                .Where(a => a.Year == now.Year && a.Month == now.Month)
                .ToDictionary(a => a.EmployeeId);

            grid.DataSource = _all.Select(e =>
            {
                var st = att.TryGetValue(e.Id, out var a) ? a.Status : "Active";
                return new
                {
                    Id       = e.Id,
                    الكود    = e.Code,
                    الاسم    = e.Name,
                    الوظيفة  = e.Job,
                    الراتب   = e.Salary,
                    الحالة   = st switch { "Left" => "مشي 🚪", "Rejoined" => "رجع ↩", _ => "شغّال ✅" },
                    الموبايل = e.Mobile
                };
            }).ToList();
        }

        protected override void AddExtraButtons(FlowLayoutPanel toolbar)
        {
            var btnPay = UIHelper.MakeButton("💵 صرف راتب",   AppTheme.Warning, new Size(130, 36), Point.Empty); btnPay.Margin = new Padding(4, 0, 0, 0); btnPay.Click += (s, e) => OpenPayroll();
            var btnAtt = UIHelper.MakeButton("📋 كشف الرواتب", AppTheme.Primary, new Size(140, 36), Point.Empty); btnAtt.Margin = new Padding(4, 0, 0, 0); btnAtt.Click += (s, e) => OpenPayrollSheet();
            var btnSt  = UIHelper.MakeButton("🔄 تغيير الحالة", AppTheme.Info,  new Size(140, 36), Point.Empty); btnSt.Margin  = new Padding(4, 0, 0, 0); btnSt.Click  += (s, e) => ChangeStatus();
            toolbar.Controls.AddRange(new Control[] { btnPay, btnAtt, btnSt });
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("إضافة موظف",
                new[] { "الكود", "الاسم *", "الوظيفة", "الراتب الأساسي", "التليفون", "الموبايل", "ملاحظات" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(f.Values[1])) { UIHelper.ShowError("الاسم مطلوب"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Employees.Add(new Employee { Code = int.TryParse(f.Values[0], out int c) ? c : 0, Name = f.Values[1], Job = f.Values[2], Salary = f.Values[3], Tel = f.Values[4], Mobile = f.Values[5], Notes = f.Values[6] });
            db.SaveChanges(); LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var e = db.Employees.Find(id)!;
            using var f = new SimpleEditForm("تعديل موظف",
                new[] { "الكود", "الاسم *", "الوظيفة", "الراتب الأساسي", "التليفون", "الموبايل", "ملاحظات" },
                new[] { e.Code.ToString(), e.Name, e.Job ?? "", e.Salary ?? "", e.Tel ?? "", e.Mobile ?? "", e.Notes ?? "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            e.Code = int.TryParse(f.Values[0], out int c) ? c : 0;
            e.Name = f.Values[1]; e.Job = f.Values[2]; e.Salary = f.Values[3];
            e.Tel = f.Values[4]; e.Mobile = f.Values[5]; e.Notes = f.Values[6];
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا الموظف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var e = db.Employees.Find(id);
            if (e != null) { db.Employees.Remove(e); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(e => e.Name.Contains(k, StringComparison.OrdinalIgnoreCase) || (e.Job ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(e => new { Id = e.Id, الكود = e.Code, الاسم = e.Name, الوظيفة = e.Job, الموبايل = e.Mobile }).ToList();

        private void OpenPayroll()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var emp = db.Employees.Find(id)!;
            using var f = new PayrollSingleForm(emp);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void OpenPayrollSheet()
        {
            using var f = new PayrollSheetForm();
            f.ShowDialog(this);
            LoadData();
        }

        private void ChangeStatus()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var emp = db.Employees.Find(id)!;
            var now = DateTime.Today;

            var att = db.EmployeeAttendances.FirstOrDefault(a => a.EmployeeId == id && a.Year == now.Year && a.Month == now.Month);
            string curStatus = att?.Status ?? "Active";

            using var dlg = new Form
            {
                Text = $"تغيير حالة: {emp.Name}", Size = new Size(360, 230),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false,
                BackColor = Color.White, RightToLeft = RightToLeft.Yes, RightToLeftLayout = true
            };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(14, 10, 14, 8), BackColor = Color.White };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var cboStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontNormal, Margin = new Padding(0, 3, 0, 3) };
            cboStatus.Items.AddRange(new[] { "Active - شغّال", "Left - مشي", "Rejoined - رجع" });
            cboStatus.SelectedIndex = curStatus switch { "Left" => 1, "Rejoined" => 2, _ => 0 };

            var dtpDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = now, Font = AppTheme.FontNormal, Margin = new Padding(0, 3, 0, 3) };
            var txtNotes = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };

            tbl.Controls.Add(new Label { Text = "الحالة:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            tbl.Controls.Add(cboStatus, 1, 0);
            tbl.Controls.Add(new Label { Text = "تاريخ التغيير:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            tbl.Controls.Add(dtpDate, 1, 1);
            tbl.Controls.Add(new Label { Text = "ملاحظة:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            tbl.Controls.Add(txtNotes, 1, 2);

            var bflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            var btnOk = UIHelper.MakeButton("✔ حفظ", AppTheme.Accent, new Size(110, 34), Point.Empty); btnOk.Margin = new Padding(0, 4, 0, 0); btnOk.DialogResult = DialogResult.OK;
            var btnNo = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(90, 34), Point.Empty); btnNo.Margin = new Padding(6, 4, 0, 0); btnNo.DialogResult = DialogResult.Cancel;
            bflow.Controls.AddRange(new Control[] { btnOk, btnNo });
            tbl.Controls.Add(new Label(), 0, 3); tbl.Controls.Add(bflow, 1, 3);
            dlg.Controls.Add(tbl); dlg.AcceptButton = btnOk; dlg.CancelButton = btnNo;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string newStatus = cboStatus.SelectedIndex switch { 1 => "Left", 2 => "Rejoined", _ => "Active" };
            if (att == null)
            {
                att = new EmployeeAttendance { EmployeeId = emp.Id, Month = now.Month, Year = now.Year };
                db.EmployeeAttendances.Add(att);
            }
            att.Status = newStatus;
            att.StatusDate = dtpDate.Value.Date;
            att.Notes = txtNotes.Text.Trim();
            db.SaveChanges();

            string msg = newStatus switch { "Left" => "🚪 تم تسجيل مغادرة", "Rejoined" => "↩ تم تسجيل الرجوع", _ => "✅ الحالة: شغّال" };
            UIHelper.ShowSuccess($"{msg}\n{emp.Name}  |  {dtpDate.Value:dd/MM/yyyy}");
            LoadData();
        }
    }

    // ══════════════════════ PAYROLL SINGLE ══════════════════════
    internal class PayrollSingleForm : Form
    {
        private readonly Employee _emp;
        private NumericUpDown numMonth = null!, numYear = null!;
        private TextBox txtBase = null!, txtDays = null!, txtNet = null!, txtNotes = null!;
        private DateTimePicker dtpDate = null!;
        private Label lblStatus = null!;

        public PayrollSingleForm(Employee emp)
        {
            _emp = emp;
            Text = $"💵 صرف راتب — {emp.Name}";
            Size = new Size(420, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; BackColor = Color.White;
            RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
            BuildUI();
        }

        private void BuildUI()
        {
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(14, 10, 14, 8), BackColor = Color.White };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            numMonth = new NumericUpDown { Minimum = 1, Maximum = 12, Value = DateTime.Today.Month, Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Margin = new Padding(0, 3, 0, 3) };
            numYear  = new NumericUpDown { Minimum = 2020, Maximum = 2100, Value = DateTime.Today.Year, Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Margin = new Padding(0, 3, 0, 3) };
            txtBase  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = _emp.Salary ?? "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };
            txtDays  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = "30", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };
            txtNet   = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontBold, BackColor = Color.FromArgb(220, 252, 231), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };
            txtNotes = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = $"راتب {_emp.Name}", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };
            dtpDate  = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = AppTheme.FontNormal, Margin = new Padding(0, 3, 0, 3) };
            lblStatus= new Label { Dock = DockStyle.Fill, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };

            void CalcNet(object? s = null, EventArgs? e = null)
            {
                if (double.TryParse(txtBase.Text, out double b) && double.TryParse(txtDays.Text, out double d))
                    txtNet.Text = (_emp.ByDay ? b * d : b).ToString("N2");
                CheckExisting();
            }

            txtBase.TextChanged += CalcNet; txtDays.TextChanged += CalcNet;
            numMonth.ValueChanged += CalcNet; numYear.ValueChanged += CalcNet;

            void CheckExisting(object? s = null, EventArgs? e = null)
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                bool paid = db.PayrollRecords.Any(p => p.EmployeeId == _emp.Id && p.Month == (int)numMonth.Value && p.Year == (int)numYear.Value && p.Status == "Paid");
                lblStatus.Text = paid ? "⚠ تم صرف راتب هذا الشهر مسبقاً" : "";
                lblStatus.ForeColor = paid ? AppTheme.Danger : AppTheme.TextGray;
            }

            tbl.Controls.Add(L("السنة:"),           0, 0); tbl.Controls.Add(numYear,  1, 0);
            tbl.Controls.Add(L("الشهر:"),          0, 1); tbl.Controls.Add(numMonth, 1, 1);
            tbl.Controls.Add(L("الراتب الأساسي:"), 0, 2); tbl.Controls.Add(txtBase,  1, 2);
            tbl.Controls.Add(L(_emp.ByDay ? "عدد الأيام:" : "الراتب الشهري:"), 0, 3); tbl.Controls.Add(txtDays, 1, 3);
            tbl.Controls.Add(L("الصافي للصرف:"),   0, 4); tbl.Controls.Add(txtNet,   1, 4);
            tbl.Controls.Add(L("تاريخ الصرف:"),    0, 5); tbl.Controls.Add(dtpDate,  1, 5);
            tbl.Controls.Add(L("ملاحظات:"),         0, 6); tbl.Controls.Add(txtNotes, 1, 6);
            tbl.Controls.Add(lblStatus,             0, 7); tbl.SetColumnSpan(lblStatus, 2);

            if (!_emp.ByDay) { txtDays.Visible = false; tbl.Controls.Add(new Label(), 0, 3); }

            var bflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            var btnOk = UIHelper.MakeButton("💵 صرف", AppTheme.Accent, new Size(110, 34), Point.Empty); btnOk.Margin = new Padding(0, 4, 0, 0); btnOk.Click += Save;
            var btnNo = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(90, 34), Point.Empty); btnNo.Margin = new Padding(6, 4, 0, 0); btnNo.DialogResult = DialogResult.Cancel;
            bflow.Controls.AddRange(new Control[] { btnOk, btnNo });
            tbl.Controls.Add(bflow, 1, 7);

            Controls.Add(tbl); AcceptButton = btnOk; CancelButton = btnNo;
            CalcNet();
        }

        private void Save(object? sender, EventArgs e)
        {
            if (!double.TryParse(txtNet.Text, out double net) || net <= 0) { UIHelper.ShowError("أدخل مبلغاً صحيحاً"); return; }
            int month = (int)numMonth.Value, year = (int)numYear.Value;

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            int fyId = MainForm.CurrentFiscalYearId ?? db.FiscalYears.Where(f => !f.IsClosed).OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();
            if (fyId == 0) fyId = db.FiscalYears.OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();

            var box = new BoxTransaction
            {
                Out = true, Value = net, Date = dtpDate.Value.Date, Time = DateTime.Now,
                Notes = txtNotes.Text.Trim(), CustName = _emp.Name,
                No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1,
                FiscalYearId = fyId
            };
            db.BoxTransactions.Add(box); db.SaveChanges();

            var salaryCost = db.Costs.FirstOrDefault(c => c.CostName.Contains("راتب") || c.CostName.Contains("رواتب") || c.CostName.Contains("أجور"));
            db.Expenses.Add(new Expense { Value = (float)net, Date = dtpDate.Value.Date, Detail = txtNotes.Text.Trim(), Notes = _emp.Name, CostId = salaryCost?.Id, FiscalYearId = fyId });

            var rec = db.PayrollRecords.FirstOrDefault(p => p.EmployeeId == _emp.Id && p.Month == month && p.Year == year)
                      ?? new PayrollRecord { EmployeeId = _emp.Id, Month = month, Year = year };
            bool isNew = rec.Id == 0;
            rec.BaseSalary = double.TryParse(_emp.Salary, out double bs) ? bs : 0;
            rec.WorkedDays = double.TryParse(txtDays.Text, out double wd) ? wd : 30;
            rec.NetSalary  = net;
            rec.PaidDate   = dtpDate.Value.Date;
            rec.BoxTransactionId = box.Id;
            rec.Notes      = txtNotes.Text.Trim();
            rec.Status     = "Paid";
            if (isNew) db.PayrollRecords.Add(rec);
            db.SaveChanges();

            UIHelper.ShowSuccess($"✅ تم صرف راتب {_emp.Name}\nالمبلغ: {net:N2}\nتم خصمه من الخزينة");
            DialogResult = DialogResult.OK;
        }

        private static Label L(string t) => new Label { Text = t, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
    }

    internal class PayrollSheetForm : Form
    {
        private NumericUpDown numMonth = null!, numYear = null!;
        private DataGridView grid = null!;
        private Label lblSummary = null!;

        public PayrollSheetForm()
        {
            Text = "📋 كشف رواتب الشهر";
            Size = new Size(900, 560);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.Light;
            RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
            BuildUI(); LoadSheet();
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White, Padding = new Padding(10, 8, 10, 8) };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);

            numMonth = new NumericUpDown { Minimum = 1, Maximum = 12, Value = DateTime.Today.Month, Width = 70, Font = AppTheme.FontNormal, Margin = new Padding(0, 0, 8, 0) };
            numYear  = new NumericUpDown { Minimum = 2020, Maximum = 2100, Value = DateTime.Today.Year, Width = 85, Font = AppTheme.FontNormal, Margin = new Padding(0, 0, 8, 0) };

            var btnLoad = UIHelper.MakeButton("🔄 تحميل", AppTheme.Primary, new Size(100, 36), Point.Empty);
            btnLoad.Click += (s, e) => LoadSheet();

            var btnPayAll = UIHelper.MakeButton("💵 صرف الكل", AppTheme.Accent, new Size(120, 36), Point.Empty);
            btnPayAll.Margin = new Padding(8, 0, 0, 0);
            btnPayAll.Click += (s, e) => PayAll();

            lblSummary = new Label { AutoSize = true, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, Margin = new Padding(16, 8, 0, 0) };

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            flow.Controls.AddRange(new Control[]
            {
                lblSummary, btnPayAll, btnLoad,
                numYear,
                new Label { Text = "السنة:", AutoSize = true, Font = AppTheme.FontBold, Margin = new Padding(0, 8, 4, 0) },
                numMonth,
                new Label { Text = "الشهر:", AutoSize = true, Font = AppTheme.FontBold, Margin = new Padding(0, 8, 4, 0) },
            });
            pnlTop.Controls.Add(flow);

            grid = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(grid);
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "الحالة" || e.Value == null) return;
                if (e.Value.ToString()!.Contains("تم"))     { e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61); }
                else if (e.Value.ToString()!.Contains("مشي")){ e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28); }
                else                                         { e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14); }
            };

            Controls.Add(UIHelper.WrapGrid(grid));
            Controls.Add(pnlTop);
        }

        private void LoadSheet()
        {
            int month = (int)numMonth.Value, year = (int)numYear.Value;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var emps = db.Employees.OrderBy(e => e.Code).ToList();
            var recs = db.PayrollRecords.Where(p => p.Month == month && p.Year == year).ToDictionary(p => p.EmployeeId);
            var atts = db.EmployeeAttendances.Where(a => a.Month == month && a.Year == year).ToDictionary(a => a.EmployeeId);

            double totPaid = 0, totPending = 0;
            grid.DataSource = emps.Select(e =>
            {
                bool paid = recs.TryGetValue(e.Id, out var r) && r.Status == "Paid";
                double net = paid ? r!.NetSalary : (double.TryParse(e.Salary, out double s) ? s : 0);
                string attSt = atts.TryGetValue(e.Id, out var a) ? a.Status : "Active";
                if (paid) totPaid += net; else totPending += net;
                return new
                {
                    الكود      = e.Code,
                    الاسم      = e.Name,
                    الوظيفة    = e.Job,
                    الراتب     = net.ToString("N2"),
                    الحالة_الوظيفية = attSt switch { "Left" => "مشي 🚪", "Rejoined" => "رجع ↩", _ => "شغّال" },
                    الحالة     = paid ? $"تم الصرف ✅ ({r!.PaidDate:dd/MM})" : "لم يُصرف ⏳",
                    ملاحظات   = paid ? (r!.Notes ?? "") : ""
                };
            }).ToList();

            lblSummary.Text = $"تم الصرف: {totPaid:N2}  |  لم يُصرف بعد: {totPending:N2}";
        }

        private void PayAll()
        {
            if (!UIHelper.Confirm($"صرف رواتب كل الموظفين لشهر {numMonth.Value}/{numYear.Value}؟")) return;
            int month = (int)numMonth.Value, year = (int)numYear.Value;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            int fyId = MainForm.CurrentFiscalYearId ?? db.FiscalYears.Where(f => !f.IsClosed).OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();
            if (fyId == 0) fyId = db.FiscalYears.OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();

            var emps = db.Employees.ToList();
            var paid = db.PayrollRecords.Where(p => p.Month == month && p.Year == year && p.Status == "Paid").Select(p => p.EmployeeId).ToHashSet();
            var atts = db.EmployeeAttendances.Where(a => a.Month == month && a.Year == year).ToDictionary(a => a.EmployeeId);
            var salaryCost = db.Costs.FirstOrDefault(c => c.CostName.Contains("راتب") || c.CostName.Contains("رواتب") || c.CostName.Contains("أجور"));
            int count = 0; double total = 0;

            foreach (var emp in emps)
            {
                if (paid.Contains(emp.Id)) continue;
                if (atts.TryGetValue(emp.Id, out var a) && a.Status == "Left") continue;
                if (!double.TryParse(emp.Salary, out double net) || net <= 0) continue;

                var box = new BoxTransaction { Out = true, Value = net, Date = DateTime.Today, Time = DateTime.Now, Notes = $"راتب {emp.Name} {month}/{year}", CustName = emp.Name, No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1, FiscalYearId = fyId };
                db.BoxTransactions.Add(box); db.SaveChanges();

                db.Expenses.Add(new Expense { Value = (float)net, Date = DateTime.Today, Detail = $"راتب {emp.Name} {month}/{year}", Notes = emp.Name, CostId = salaryCost?.Id, FiscalYearId = fyId });
                db.PayrollRecords.Add(new PayrollRecord { EmployeeId = emp.Id, Month = month, Year = year, BaseSalary = net, WorkedDays = 30, NetSalary = net, PaidDate = DateTime.Today, BoxTransactionId = box.Id, Notes = $"راتب {month}/{year}", Status = "Paid" });
                db.SaveChanges(); count++; total += net;
            }

            UIHelper.ShowSuccess($"✅ تم صرف رواتب {count} موظف\nإجمالي: {total:N2} ج");
            LoadSheet();
        }
    }
}
