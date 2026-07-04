using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    // ══════════════════════ PAYMENTS ══════════════════════
    public class PaymentsForm : BaseListForm
    {
        private readonly bool _isReceipt;
        private List<Payment> _all = new();

        public PaymentsForm(bool isReceipt) : base(isReceipt ? "سندات القبض" : "سندات الصرف")
        {
            _isReceipt = isReceipt;
        }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Payments.Where(p => p.Type == _isReceipt).OrderByDescending(p => p.Date).Take(200).ToList();
            grid.DataSource = _all.Select(p => new
            {
                Id = p.Id, التاريخ = p.Date.ToString("yyyy/MM/dd"),
                المبلغ = p.Total.ToString("N2"),
                من_إلى = _isReceipt ? $"من: {p.From}" : $"إلى: {p.To}",
                ملاحظات = p.Notes
            }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm(
                _isReceipt ? "إضافة سند قبض" : "إضافة سند صرف",
                new[] { "التاريخ", "المبلغ", "ملاحظات" },
                new[] { DateTime.Today.ToString("yyyy/MM/dd"), "0", "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Payments.Add(new Payment
            {
                Type = _isReceipt,
                Date = DateTime.TryParse(f.Values[0], out var d) ? d : DateTime.Today,
                Total = float.TryParse(f.Values[1], out float t) ? t : 0,
                Notes = f.Values[2]
            });
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا السند؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = db.Payments.Find(id);
            if (p != null) { db.Payments.Remove(p); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) => LoadData();
    }

    // ══════════════════════ BOX (TREASURY) ══════════════════════
    public class BoxForm : BaseListForm
    {
        private List<BoxTransaction> _all = new();
        public BoxForm() : base("الخزينة") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var query = db.BoxTransactions.AsQueryable();
            
            // Filter by selected fiscal year
            if (MainForm.CurrentFiscalYearId.HasValue)
                query = query.Where(b => b.FiscalYearId == MainForm.CurrentFiscalYearId.Value);
            
            _all = query.OrderByDescending(b => b.Date).Take(300).ToList();
            double balance = query.Sum(b => b.Out ? -b.Value : b.Value);

            // Add balance row at top
            var display = _all.Select(b => new
            {
                Id = b.Id, التاريخ = b.Date.ToString("yyyy/MM/dd"),
                النوع = b.Out ? "مصروف 🔴" : "وارد 🟢",
                المبلغ = b.Value.ToString("N2"),
                ملاحظات = b.Notes,
                اسم_العميل = b.CustName,
                رقم_الخزينة = b.BoxNo
            }).ToList();
            grid.DataSource = display;

            // Show balance in title
            this.Text = $"الخزينة  |  الرصيد الحالي: {balance:N2}";
        }

        protected override void AddExtraButtons(FlowLayoutPanel toolbar)
        {
            var btnAdjust = Btn("⚖ ضبط الرصيد", AppTheme.Warning);
            btnAdjust.Click += (s, e) => AdjustBalance();
            toolbar.Controls.Add(btnAdjust);
        }

        private void AdjustBalance()
        {
            // احسب الرصيد الحالي أولاً
            using var dbCheck = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var queryCheck = dbCheck.BoxTransactions.AsQueryable();
            if (MainForm.CurrentFiscalYearId.HasValue)
                queryCheck = queryCheck.Where(b => b.FiscalYearId == MainForm.CurrentFiscalYearId.Value);
            double currentBalance = queryCheck.Sum(b => b.Out ? -(double)b.Value : (double)b.Value);

            // نافذة ضبط الرصيد مع عرض الرصيد الحالي
            using var dlg = new Form
            {
                Text = "⚖ ضبط رصيد الخزينة يدوياً",
                Size = new Size(460, 290),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5,
                Padding = new Padding(16, 14, 16, 12), BackColor = Color.White
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblCurrTitle = new Label { Text = "الرصيد الحالي:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var lblCurrVal   = new Label
            {
                Text = currentBalance.ToString("N2") + " ج",
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = currentBalance >= 0 ? AppTheme.Accent : AppTheme.Danger,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.FromArgb(240, 255, 245)
            };

            var lblActTitle  = new Label { Text = "الرصيد الفعلي:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var txtActual    = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = currentBalance.ToString("N2"), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 5, 0, 5) };

            var lblDateTitle = new Label { Text = "التاريخ:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var dtpDate      = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 5, 0, 5) };

            var lblNoteTitle = new Label { Text = "ملاحظات:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var txtNotes     = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = "ضبط رصيد يدوي", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 5, 0, 5) };

            var btnOk     = UIHelper.MakeButton("✔ تطبيق الضبط", AppTheme.Accent, new Size(150, 36), Point.Empty);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء",        AppTheme.Danger, new Size(100, 36), Point.Empty);
            btnCancel.DialogResult = DialogResult.Cancel;
            var btnPnl = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(0, 4, 0, 0) };
            btnPnl.Controls.AddRange(new Control[] { btnOk, btnCancel });

            tbl.Controls.Add(lblCurrTitle, 0, 0); tbl.Controls.Add(lblCurrVal,  1, 0);
            tbl.Controls.Add(lblActTitle,  0, 1); tbl.Controls.Add(txtActual,   1, 1);
            tbl.Controls.Add(lblDateTitle, 0, 2); tbl.Controls.Add(dtpDate,     1, 2);
            tbl.Controls.Add(lblNoteTitle, 0, 3); tbl.Controls.Add(txtNotes,    1, 3);
            tbl.Controls.Add(new Label(),  0, 4); tbl.Controls.Add(btnPnl,      1, 4);
            dlg.Controls.Add(tbl);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            btnOk.Click += (s, e) =>
            {
                if (!double.TryParse(txtActual.Text, out double actualBalance) || actualBalance < 0)
                {
                    UIHelper.ShowError("أدخل رصيداً صحيحاً (لا يقل عن صفر)");
                    return;
                }
                double diff = actualBalance - currentBalance;
                if (Math.Abs(diff) < 0.01)
                {
                    MessageBox.Show($"الرصيد الحالي ({currentBalance:N2}) مطابق للرصيد الفعلي المُدخل.",
                        "لا يوجد فرق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string direction = diff > 0 ? "إيراد" : "مصروف";
                if (!UIHelper.Confirm($"سيتم إضافة قيد ضبط ({direction}) بمقدار {Math.Abs(diff):N2}\nالرصيد الجديد سيصبح: {actualBalance:N2}\nهل تريد المتابعة؟"))
                    return;

                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                DateTime txDate = dtpDate.Value.Date;
                int fiscalYearId = MainForm.CurrentFiscalYearId ??
                    db.FiscalYears.FirstOrDefault(fy => fy.Year == txDate.Year && !fy.IsClosed)?.Id ??
                    db.FiscalYears.OrderByDescending(fy => fy.Year).FirstOrDefault()?.Id ?? 1;

                db.BoxTransactions.Add(new BoxTransaction
                {
                    Out = diff < 0,
                    Value = Math.Abs(diff),
                    Date = txDate,
                    Time = DateTime.Now,
                    Notes = $"ضبط رصيد يدوي: {txtNotes.Text.Trim()} | الرصيد قبل: {currentBalance:N2} | الرصيد بعد: {actualBalance:N2}",
                    No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1,
                    FiscalYearId = fiscalYearId
                });
                db.SaveChanges();
                LoadData();
                UIHelper.ShowSuccess($"✅ تم ضبط الرصيد بنجاح\nالرصيد القديم: {currentBalance:N2}\nالرصيد الجديد: {actualBalance:N2}\nالفرق: {diff:+N2;-N2;0}");
                dlg.DialogResult = DialogResult.OK;
            };

            dlg.ShowDialog(this);
        }

        protected override void OnAdd()
        {
            using var f = new BoxTransactionEditForm();
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه الحركة؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var b = db.BoxTransactions.Find(id);
            if (b != null) { db.BoxTransactions.Remove(b); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) => LoadData();
    }

    internal class BoxTransactionEditForm : Form
    {
        public BoxTransactionEditForm()
        {
            this.Text = "حركة خزينة جديدة";
            this.Size = new Size(380, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;

            var rbIn  = new RadioButton { Text = "وارد (إيراد)", Location = new Point(20, 20), AutoSize = true, Font = AppTheme.FontBold, ForeColor = AppTheme.Accent, Checked = true };
            var rbOut = new RadioButton { Text = "مصروف",       Location = new Point(150, 20), AutoSize = true, Font = AppTheme.FontBold, ForeColor = AppTheme.Danger };

            var lblVal  = new Label { Text = "المبلغ:", Location = new Point(20, 60), AutoSize = true, Font = AppTheme.FontBold };
            var txtVal  = new TextBox { Location = new Point(100, 57), Size = new Size(240, 28), Font = AppTheme.FontNormal, Text = "0", BorderStyle = BorderStyle.FixedSingle };

            var lblDate = new Label { Text = "التاريخ:", Location = new Point(20, 100), AutoSize = true, Font = AppTheme.FontBold };
            var txtDate = new TextBox { Location = new Point(100, 97), Size = new Size(240, 28), Font = AppTheme.FontNormal, Text = DateTime.Today.ToString("yyyy/MM/dd"), BorderStyle = BorderStyle.FixedSingle };

            var lblNote = new Label { Text = "ملاحظات:", Location = new Point(20, 140), AutoSize = true, Font = AppTheme.FontBold };
            var txtNote = new TextBox { Location = new Point(100, 137), Size = new Size(240, 28), Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle };

            var lblCust = new Label { Text = "اسم العميل:", Location = new Point(20, 180), AutoSize = true, Font = AppTheme.FontBold };
            var txtCust = new TextBox { Location = new Point(100, 177), Size = new Size(240, 28), Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle };

            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(120, 36), new Point(100, 220));
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(110, 36), new Point(230, 220));
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                if (!double.TryParse(txtVal.Text, out double val) || val <= 0)
                { UIHelper.ShowError("أدخل مبلغاً صحيحاً"); this.DialogResult = DialogResult.None; return; }
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                
                // Get current fiscal year
                DateTime txDate = DateTime.TryParse(txtDate.Text, out var d) ? d : DateTime.Today;
                int fiscalYearId = MainForm.CurrentFiscalYearId ?? 
                    db.FiscalYears.FirstOrDefault(f => f.Year == txDate.Year && !f.IsClosed)?.Id ?? 
                    db.FiscalYears.OrderByDescending(f => f.Year).FirstOrDefault()?.Id ?? 1;
                
                db.BoxTransactions.Add(new BoxTransaction
                {
                    Out = rbOut.Checked,
                    Value = val,
                    Date = txDate,
                    Time = DateTime.Now,
                    Notes = txtNote.Text.Trim(),
                    CustName = txtCust.Text.Trim(),
                    No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1,
                    FiscalYearId = fiscalYearId
                });
                db.SaveChanges();
            };
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { rbIn, rbOut, lblVal, txtVal, lblDate, txtDate, lblNote, txtNote, lblCust, txtCust, btnSave, btnCancel });
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }
    }

    // ══════════════════════ COST TYPES (أنواع المصروفات/الإيرادات) ══════════════════════
    public class CostTypesForm : BaseListForm
    {
        private List<Cost> _all = new();
        public CostTypesForm() : base("أنواع المصروفات والإيرادات") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Costs.OrderBy(c => c.Code).ToList();
            grid.DataSource = _all.Select(c => new { Id = c.Id, الكود = c.Code, الاسم = c.CostName, ملاحظات = c.Notes }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("إضافة نوع", new[] { "الكود", "الاسم *" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(f.Values[1])) { UIHelper.ShowError("الاسم مطلوب"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Costs.Add(new Cost { Code = f.Values[0], CostName = f.Values[1] });
            db.SaveChanges(); LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var c = db.Costs.Find(id)!;
            using var f = new SimpleEditForm("تعديل نوع", new[] { "الكود", "الاسم *" }, new[] { c.Code, c.CostName });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            c.Code = f.Values[0]; c.CostName = f.Values[1];
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا النوع؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var c = db.Costs.Find(id);
            if (c != null) { db.Costs.Remove(c); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all
                : _all.Where(c => c.CostName.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(c => new { Id = c.Id, الكود = c.Code, الاسم = c.CostName }).ToList();
    }

    // ══════════════════════ EXPENSES ══════════════════════
    public class ExpensesForm : BaseListForm
    {
        private List<Expense> _all = new();
        public ExpensesForm() : base("المصروفات") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var query = db.Expenses.Include(e => e.Cost).AsQueryable();
            
            // Filter by selected fiscal year
            if (MainForm.CurrentFiscalYearId.HasValue)
                query = query.Where(e => e.FiscalYearId == MainForm.CurrentFiscalYearId.Value);
            
            _all = query.OrderByDescending(e => e.Date).ToList();
            grid.DataSource = _all.Select(e => new
            {
                Id = e.Id,
                التاريخ = e.Date.ToString("yyyy/MM/dd"),
                النوع = e.Cost?.CostName ?? "—",
                المبلغ = e.Value.ToString("N2"),
                التفاصيل = e.Detail,
                ملاحظات = e.Notes
            }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new ExpenseIncomeEditForm("إضافة مصروف", isExpense: true);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            // Get current fiscal year
            int fiscalYearId = MainForm.CurrentFiscalYearId ?? 
                db.FiscalYears.FirstOrDefault(fy => fy.Year == f.EnteredDate.Year && !fy.IsClosed)?.Id ?? 
                db.FiscalYears.OrderByDescending(fy => fy.Year).FirstOrDefault()?.Id ?? 1;

            // سجل في الخزينة أولاً
            var box = new BoxTransaction
            {
                Out = true, Value = f.EnteredValue,
                Date = f.EnteredDate, Time = DateTime.Now,
                Notes = $"مصروف: {f.EnteredCostName} - {f.EnteredDetail}",
                No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1,
                FiscalYearId = fiscalYearId
            };
            db.BoxTransactions.Add(box);
            db.SaveChanges();

            // سجل المصروف مربوط بحركة الخزينة
            db.Expenses.Add(new Expense
            {
                Value = f.EnteredValue, Detail = f.EnteredDetail,
                Notes = f.EnteredNotes, Date = f.EnteredDate,
                CostId = f.EnteredCostId, BoxTransactionId = box.Id,
                FiscalYearId = fiscalYearId, MainActive = true
            });
            db.SaveChanges();
            LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var exp = db.Expenses.Include(e => e.Cost).FirstOrDefault(e => e.Id == id);
            if (exp == null) return;
            using var f = new ExpenseIncomeEditForm("تعديل مصروف", isExpense: true, existing: exp);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            exp.Value = f.EnteredValue; exp.Detail = f.EnteredDetail;
            exp.Notes = f.EnteredNotes; exp.Date = f.EnteredDate;
            exp.CostId = f.EnteredCostId;
            if (exp.BoxTransactionId.HasValue)
            {
                var box = db.BoxTransactions.Find(exp.BoxTransactionId.Value);
                if (box != null) { box.Value = f.EnteredValue; box.Date = f.EnteredDate; }
            }
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا المصروف؟ سيتم حذف حركته من الخزينة أيضاً.")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var e = db.Expenses.Find(id);
            if (e != null)
            {
                // احذف من الخزينة لو موجود
                if (e.BoxTransactionId.HasValue)
                {
                    var box = db.BoxTransactions.Find(e.BoxTransactionId.Value);
                    if (box != null) db.BoxTransactions.Remove(box);
                }
                db.Expenses.Remove(e);
                db.SaveChanges();
            }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all
                : _all.Where(e => (e.Detail ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)
                               || (e.Cost?.CostName ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(e => new { Id = e.Id, التاريخ = e.Date.ToString("yyyy/MM/dd"), النوع = e.Cost?.CostName ?? "—", المبلغ = e.Value.ToString("N2"), التفاصيل = e.Detail }).ToList();
    }

    // ══════════════════════ INCOMES ══════════════════════
    public class IncomesForm : BaseListForm
    {
        private List<Income> _all = new();
        public IncomesForm() : base("الإيرادات") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var query = db.Incomes.Include(i => i.Cost).AsQueryable();
            
            // Filter by selected fiscal year
            if (MainForm.CurrentFiscalYearId.HasValue)
                query = query.Where(i => i.FiscalYearId == MainForm.CurrentFiscalYearId.Value);
            
            _all = query.OrderByDescending(i => i.Date).ToList();
            grid.DataSource = _all.Select(i => new
            {
                Id = i.Id,
                التاريخ = i.Date.ToString("yyyy/MM/dd"),
                النوع = i.Cost?.CostName ?? "—",
                المبلغ = i.Value.ToString("N2"),
                التفاصيل = i.Detail,
                ملاحظات = i.Notes
            }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new ExpenseIncomeEditForm("إضافة إيراد", isExpense: false);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            // Get current fiscal year
            int fiscalYearId = MainForm.CurrentFiscalYearId ?? 
                db.FiscalYears.FirstOrDefault(fy => fy.Year == f.EnteredDate.Year && !fy.IsClosed)?.Id ?? 
                db.FiscalYears.OrderByDescending(fy => fy.Year).FirstOrDefault()?.Id ?? 1;

            // سجل في الخزينة أولاً
            var box = new BoxTransaction
            {
                Out = false, Value = f.EnteredValue,
                Date = f.EnteredDate, Time = DateTime.Now,
                Notes = $"إيراد: {f.EnteredCostName} - {f.EnteredDetail}",
                No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1,
                FiscalYearId = fiscalYearId
            };
            db.BoxTransactions.Add(box);
            db.SaveChanges();

            // سجل الإيراد مربوط بحركة الخزينة
            db.Incomes.Add(new Income
            {
                Value = f.EnteredValue, Detail = f.EnteredDetail,
                Notes = f.EnteredNotes, Date = f.EnteredDate,
                CostId = f.EnteredCostId, BoxTransactionId = box.Id,
                FiscalYearId = fiscalYearId, MainActive = true
            });
            db.SaveChanges();
            LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var inc = db.Incomes.Include(i => i.Cost).FirstOrDefault(i => i.Id == id);
            if (inc == null) return;
            using var f = new ExpenseIncomeEditForm("تعديل إيراد", isExpense: false, existing: inc);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            inc.Value = f.EnteredValue; inc.Detail = f.EnteredDetail;
            inc.Notes = f.EnteredNotes; inc.Date = f.EnteredDate;
            inc.CostId = f.EnteredCostId;
            if (inc.BoxTransactionId.HasValue)
            {
                var box = db.BoxTransactions.Find(inc.BoxTransactionId.Value);
                if (box != null) { box.Value = f.EnteredValue; box.Date = f.EnteredDate; }
            }
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا الإيراد؟ سيتم حذف حركته من الخزينة أيضاً.")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var i = db.Incomes.Find(id);
            if (i != null)
            {
                if (i.BoxTransactionId.HasValue)
                {
                    var box = db.BoxTransactions.Find(i.BoxTransactionId.Value);
                    if (box != null) db.BoxTransactions.Remove(box);
                }
                db.Incomes.Remove(i);
                db.SaveChanges();
            }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all
                : _all.Where(i => (i.Detail ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)
                               || (i.Cost?.CostName ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(i => new { Id = i.Id, التاريخ = i.Date.ToString("yyyy/MM/dd"), النوع = i.Cost?.CostName ?? "—", المبلغ = i.Value.ToString("N2"), التفاصيل = i.Detail }).ToList();
    }

    // ══════════════════════ EXPENSE/INCOME EDIT FORM ══════════════════════
    internal class ExpenseIncomeEditForm : Form
    {
        public float EnteredValue { get; private set; }
        public string EnteredDetail { get; private set; } = "";
        public string EnteredNotes  { get; private set; } = "";
        public DateTime EnteredDate { get; private set; } = DateTime.Today;
        public int? EnteredCostId   { get; private set; }
        public string EnteredCostName { get; private set; } = "";

        public ExpenseIncomeEditForm(string title, bool isExpense, object? existing = null)
        {
            this.Text = title;
            this.Size = new Size(420, 310);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;

            // قيم افتراضية من الـ existing record
            float defVal   = existing is Expense ex ? ex.Value  : existing is Income inc ? inc.Value  : 0;
            string defDet  = existing is Expense ex2? ex2.Detail??"": existing is Income inc2? inc2.Detail??"": "";
            string defNotes= existing is Expense ex3? ex3.Notes??"": existing is Income inc3? inc3.Notes??"": "";
            DateTime defDate = existing is Expense ex4? ex4.Date: existing is Income inc4? inc4.Date: DateTime.Today;
            int? defCostId = existing is Expense ex5? ex5.CostId: existing is Income inc5? inc5.CostId: null;

            // الحقول
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5,
                Padding = new Padding(12), BackColor = Color.White
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            // النوع
            var cboType = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 6, 0, 6) };
            using (var db = Program.ServiceProvider.GetRequiredService<AppDbContext>())
            {
                var costs = db.Costs.OrderBy(c => c.Code).ToList();
                costs.Insert(0, new Cost { Id = 0, CostName = "-- بدون تصنيف --" });
                cboType.DataSource = costs;
                cboType.DisplayMember = "CostName";
                cboType.ValueMember  = "Id";
                if (defCostId.HasValue) cboType.SelectedValue = defCostId.Value;
            }

            var txtValue  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = defVal > 0 ? defVal.ToString("N2") : "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };
            var dtpDate   = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = defDate, Margin = new Padding(0, 6, 0, 6) };
            var txtDetail = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = defDet,   BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };
            var txtNotes  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = defNotes, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };

            void AddRow(int row, string lbl, Control ctrl)
            {
                table.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, row);
                table.Controls.Add(ctrl, 1, row);
            }
            AddRow(0, "النوع:", cboType);
            AddRow(1, "المبلغ *:", txtValue);
            AddRow(2, "التاريخ:", dtpDate);
            AddRow(3, "التفاصيل:", txtDetail);
            AddRow(4, "ملاحظات:", txtNotes);

            // أزرار
            var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
            var btnSave = UIHelper.MakeButton("💾 حفظ", isExpense ? AppTheme.Danger : AppTheme.Accent, new Size(130, 38), Point.Empty);
            btnSave.Margin = new Padding(8, 7, 0, 0);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.TextGray, new Size(110, 38), Point.Empty);
            btnCancel.Margin = new Padding(8, 7, 0, 0);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(0) };
            btnFlow.Controls.Add(btnSave);
            btnFlow.Controls.Add(btnCancel);
            pnlBtns.Controls.Add(btnFlow);

            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                if (!float.TryParse(txtValue.Text, out float val) || val <= 0)
                { UIHelper.ShowError("أدخل مبلغاً صحيحاً أكبر من صفر"); this.DialogResult = DialogResult.None; return; }

                EnteredValue  = val;
                EnteredDetail = txtDetail.Text.Trim();
                EnteredNotes  = txtNotes.Text.Trim();
                EnteredDate   = dtpDate.Value.Date;

                var selected = cboType.SelectedItem as Cost;
                EnteredCostId   = (selected?.Id ?? 0) == 0 ? null : selected?.Id;
                EnteredCostName = selected?.CostName ?? "";
            };
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(table);
            this.Controls.Add(pnlBtns);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }
    }

    // ══════════════════════ JOURNAL ══════════════════════
    public class JournalForm : BaseListForm
    {
        private List<Journal> _all = new();
        public JournalForm() : base("القيد اليومي") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Journal.OrderByDescending(j => j.Date).Take(200).ToList();
            grid.DataSource = _all.Select(j => new { Id = j.Id, التاريخ = j.Date.ToString("yyyy/MM/dd"), حساب1 = j.T1, حساب2 = j.T2, القيمة = j.Value.ToString("N2"), ملاحظات = j.Notes }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("إضافة قيد", new[] { "حساب مدين", "حساب دائن", "القيمة", "ملاحظات", "التاريخ" }, new[] { "", "", "0", "", DateTime.Today.ToString("yyyy/MM/dd") });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Journal.Add(new Journal { T1 = f.Values[0], T2 = f.Values[1], Value = float.TryParse(f.Values[2], out float v) ? v : 0, Notes = f.Values[3], Date = DateTime.TryParse(f.Values[4], out var d) ? d : DateTime.Today, Time = DateTime.Now });
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا القيد؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var j = db.Journal.Find(id);
            if (j != null) { db.Journal.Remove(j); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) => LoadData();
    }

    // ══════════════════════ ACCOUNTS TREE ══════════════════════
    public class AccountsTreeForm : Form
    {
        private TreeView tree = null!;
        private Label lblBalance = null!;

        private static readonly Color[] RootColors =
        {
            AppTheme.Primary,  // أصول - أزرق
            Color.FromArgb(192, 57, 43),   // التزامات - أحمر
            Color.FromArgb(39, 174, 96),   // ملكية - أخضر
            Color.FromArgb(243, 156, 18),  // إيرادات - برتقالي
            AppTheme.Primary,  // مصروفات - بنفسجي
        };

        public AccountsTreeForm()
        {
            this.Text    = "شجرة الحسابات";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(500, 400);

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(8, 7, 8, 0) };
            var btnAdd     = UIHelper.MakeButton("➕ إضافة حساب",  AppTheme.Accent,   new Size(150, 36), Point.Empty); btnAdd.Margin     = new Padding(0, 0, 8, 0);
            var btnRefresh = UIHelper.MakeButton("🔄 تحديث",      AppTheme.TextGray, new Size(110, 36), Point.Empty); btnRefresh.Margin = new Padding(0, 0, 8, 0);
            var btnExpand  = UIHelper.MakeButton("📂 توسيع الكل", AppTheme.Primary,  new Size(130, 36), Point.Empty); btnExpand.Margin  = new Padding(0, 0, 8, 0);
            btnAdd.Click     += (s, e) => AddAccount();
            btnRefresh.Click += (s, e) => LoadTree();
            btnExpand.Click  += (s, e) => { tree.ExpandAll(); };
            flow.Controls.AddRange(new Control[] { btnAdd, btnRefresh, btnExpand });
            toolbar.Controls.Add(flow);

            // Summary bar
            var pnlInfo = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(239, 246, 255) };
            lblBalance = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };
            pnlInfo.Controls.Add(lblBalance);

            // TreeView
            tree = new TreeView
            {
                Dock          = DockStyle.Fill,
                Font          = new Font("Segoe UI", 10),
                BackColor     = Color.White,
                Scrollable    = true,
                HideSelection = false,
                ItemHeight    = 28,
                Indent        = 24,
                RightToLeft   = RightToLeft.Yes,
                DrawMode      = TreeViewDrawMode.OwnerDrawAll,
            };
            tree.DrawNode += TreeDrawNode;
            tree.NodeMouseDoubleClick += (s, e) => EditAccount(e.Node);

            var treeWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            treeWrap.Controls.Add(tree);
            treeWrap.Paint += (s, e) =>
            {
                using var path = UIHelper.RoundedRect(new Rectangle(0, 0, treeWrap.Width - 1, treeWrap.Height - 1), AppTheme.Radius);
                e.Graphics.DrawPath(new Pen(AppTheme.Border), path);
            };
            this.Controls.Add(treeWrap);
            this.Controls.Add(pnlInfo);
            this.Controls.Add(toolbar);
            LoadTree();
        }

        private void TreeDrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            bool isRoot   = e.Node.Parent == null;
            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            bool focused  = (e.State & TreeNodeStates.Focused)  != 0;

            var bounds = e.Bounds;
            // OwnerDrawAll: bounds can be empty on first paint pass — skip
            if (bounds.Height == 0) return;

            // ── Full-row background ──
            var rowRect = new Rectangle(0, bounds.Y, tree.ClientSize.Width, bounds.Height);
            using (var bgBrush = new SolidBrush(selected ? AppTheme.PrimaryLight : Color.White))
                g.FillRectangle(bgBrush, rowRect);

            // ── Expand/collapse button (drawn manually so it shows in RTL) ──
            if (e.Node.Nodes.Count > 0)
            {
                var btnRect = tree.RectangleToClient(tree.RectangleToScreen(
                    new Rectangle(bounds.X - 20, bounds.Y + (bounds.Height - 9) / 2, 9, 9)));
                btnRect = new Rectangle(bounds.X - 16, bounds.Y + (bounds.Height - 9) / 2, 9, 9);
                g.DrawRectangle(Pens.Gray, btnRect);
                g.DrawLine(Pens.Black, btnRect.X + 2, btnRect.Y + 4, btnRect.Right - 2, btnRect.Y + 4);
                if (!e.Node.IsExpanded)
                    g.DrawLine(Pens.Black, btnRect.X + 4, btnRect.Y + 2, btnRect.X + 4, btnRect.Bottom - 2);
            }

            // ── Colored side bar for root nodes ──
            int colorIdx = isRoot ? e.Node.Index : (e.Node.Parent?.Index ?? 0);
            var nodeColor = RootColors[colorIdx % RootColors.Length];

            if (isRoot)
                g.FillRectangle(new SolidBrush(nodeColor),
                    new Rectangle(tree.ClientSize.Width - 4, bounds.Y + 3, 4, bounds.Height - 6));
            else
                g.FillEllipse(new SolidBrush(Color.FromArgb(80, nodeColor)),
                    new Rectangle(tree.ClientSize.Width - 20, bounds.Y + (bounds.Height - 8) / 2, 8, 8));

            // ── Text ──
            using var font  = isRoot ? new Font("Segoe UI", 10, FontStyle.Bold) : new Font("Segoe UI", 10);
            using var brush = new SolidBrush(selected ? Color.White : (isRoot ? nodeColor : AppTheme.TextDark));
            var tf = new StringFormat(StringFormatFlags.DirectionRightToLeft | StringFormatFlags.NoWrap)
            {
                Alignment     = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                Trimming      = StringTrimming.EllipsisCharacter
            };
            // Leave room for color bar on the right and expand button on the left
            var textRect = new RectangleF(bounds.X + 20, bounds.Y, tree.ClientSize.Width - 30, bounds.Height);
            g.DrawString(e.Node.Text, font, brush, textRect, tf);

            // ── Focus rectangle ──
            if (focused)
                ControlPaint.DrawFocusRectangle(g, bounds);
        }

        private void LoadTree()
        {
            tree.Nodes.Clear();
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var roots    = db.Roots.OrderBy(r => r.Code).ToList();
            var accounts = db.RootAccounts.OrderBy(a => a.Code).ToList();

            foreach (var root in roots)
            {
                var node = new TreeNode($"{root.Code}  {root.Name}");
                var subs = accounts.Where(a => a.TypeAccount == root.Code).ToList();
                foreach (var acc in subs)
                    node.Nodes.Add(new TreeNode($"{acc.Code}  {acc.Name}"));
                tree.Nodes.Add(node);
            }
            tree.ExpandAll();
            lblBalance.Text = $"الحسابات الرئيسية: {roots.Count}    |    الحسابات الفرعية: {accounts.Count}";
        }

        private void EditAccount(TreeNode? node)
        {
            if (node == null || node.Parent == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var code = node.Text.Split(' ')[0];
            var acc  = db.RootAccounts.FirstOrDefault(a => a.Code == code);
            if (acc == null) return;
            var roots = db.Roots.OrderBy(r => r.Code).ToList();
            using var dlg = new AccountEditForm(roots, acc);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            acc.Code = dlg.AccountCode; acc.Name = dlg.AccountName; acc.TypeAccount = dlg.RootCode; acc.Notes = dlg.Notes;
            db.SaveChanges();
            LoadTree();
        }

        private void AddAccount()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var roots = db.Roots.OrderBy(r => r.Code).ToList();
            if (roots.Count == 0) { UIHelper.ShowError("لا توجد حسابات رئيسية"); return; }
            using var dlg = new AccountEditForm(roots, null);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            db.RootAccounts.Add(new RootAccount { Code = dlg.AccountCode, Name = dlg.AccountName, TypeAccount = dlg.RootCode, Notes = dlg.Notes });
            db.SaveChanges();
            LoadTree();
            UIHelper.ShowSuccess("تمت إضافة الحساب ✅");
        }
    }

    // ══════════════════════ ACCOUNT SELECTOR DIALOG ══════════════════════
    public class AccountSelectorDialog : Form
    {
        public RootAccount? SelectedAccount { get; private set; }
        
        private TreeView tree = null!;
        private TextBox txtSearch = null!;
        private List<RootAccount> _allAccounts = new();

        public AccountSelectorDialog()
        {
            this.Text = "اختر حساب من شجرة الحسابات";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(450, 400);
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.ShowInTaskbar = false;

            BuildUI();
            LoadTree();
        }

        private void BuildUI()
        {
            // Search bar
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
            pnlSearch.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlSearch.Height - 1, pnlSearch.Width, pnlSearch.Height - 1);
            
            var tblSearch = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent, Padding = new Padding(8, 6, 8, 6)
            };
            tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tblSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblSearch = new Label { Text = "🔍 بحث:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, Width = 70 };
            txtSearch = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(8, 0, 0, 0) };
            txtSearch.TextChanged += (s, e) => FilterTree();

            tblSearch.Controls.Add(lblSearch, 0, 0);
            tblSearch.Controls.Add(txtSearch, 1, 0);
            pnlSearch.Controls.Add(tblSearch);

            // TreeView
            tree = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                Scrollable = true,
                HideSelection = false,
                ItemHeight = 26,
                Indent = 20,
                RightToLeft = RightToLeft.Yes,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true
            };
            tree.NodeMouseDoubleClick += (s, e) => { if (e.Node != null && e.Node.Parent != null) SelectAndClose(e.Node); };
            tree.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && tree.SelectedNode != null && tree.SelectedNode.Parent != null) SelectAndClose(tree.SelectedNode); };

            var treeWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8) };
            treeWrap.Controls.Add(tree);

            // Buttons
            var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
            pnlBtns.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlBtns.Width, 0);
            
            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(8, 8, 8, 0) };
            var btnSelect = UIHelper.MakeButton("✔ اختيار", AppTheme.Accent, new Size(120, 36), Point.Empty);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.TextGray, new Size(110, 36), Point.Empty);
            btnSelect.Margin = btnCancel.Margin = new Padding(0, 0, 8, 0);
            
            btnSelect.Click += (s, e) =>
            {
                if (tree.SelectedNode != null && tree.SelectedNode.Parent != null)
                    SelectAndClose(tree.SelectedNode);
                else
                    UIHelper.ShowError("اختار حساباً فرعياً");
            };
            btnCancel.DialogResult = DialogResult.Cancel;
            
            btnFlow.Controls.AddRange(new Control[] { btnSelect, btnCancel });
            pnlBtns.Controls.Add(btnFlow);

            this.Controls.Add(treeWrap);
            this.Controls.Add(pnlBtns);
            this.Controls.Add(pnlSearch);
        }

        private void LoadTree()
        {
            tree.Nodes.Clear();
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var roots = db.Roots.OrderBy(r => r.Code).ToList();
            _allAccounts = db.RootAccounts.OrderBy(a => a.Code).ToList();

            foreach (var root in roots)
            {
                var node = new TreeNode($"{root.Code} - {root.Name}")
                {
                    Tag = root,
                    ForeColor = GetRootColor(root.Code)
                };
                node.NodeFont = new Font(tree.Font, FontStyle.Bold);
                
                var subs = _allAccounts.Where(a => a.TypeAccount == root.Code).ToList();
                foreach (var acc in subs)
                {
                    var childNode = new TreeNode($"{acc.Code} - {acc.Name}")
                    {
                        Tag = acc
                    };
                    node.Nodes.Add(childNode);
                }
                tree.Nodes.Add(node);
            }
            tree.ExpandAll();
        }

        private Color GetRootColor(string code)
        {
            return code switch
            {
                "1" => AppTheme.Primary,   // أصول - أزرق
                "2" => Color.FromArgb(192, 57, 43),    // التزامات - أحمر
                "3" => Color.FromArgb(39, 174, 96),    // ملكية - أخضر
                "4" => Color.FromArgb(243, 156, 18),   // إيرادات - برتقالي
                "5" => AppTheme.Primary,  // مصروفات - بنفسجي
                _ => AppTheme.TextDark
            };
        }

        private void FilterTree()
        {
            var search = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(search))
            {
                LoadTree();
                return;
            }

            tree.BeginUpdate();
            tree.CollapseAll();
            
            foreach (TreeNode rootNode in tree.Nodes)
            {
                bool rootMatch = rootNode.Text.ToLower().Contains(search);
                bool hasMatchingChild = false;

                foreach (TreeNode childNode in rootNode.Nodes)
                {
                    bool childMatch = childNode.Text.ToLower().Contains(search);
                    if (childMatch) hasMatchingChild = true;
                }

                // TreeNode doesn't have Visible property, so we just expand if there are matching children
                if (hasMatchingChild) rootNode.Expand();
            }
            
            tree.EndUpdate();
        }

        private void SelectAndClose(TreeNode node)
        {
            if (node.Tag is RootAccount acc)
            {
                SelectedAccount = acc;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    internal class AccountEditForm : Form
    {
        public string  AccountCode { get; private set; } = "";
        public string  AccountName { get; private set; } = "";
        public string  RootCode    { get; private set; } = "";
        public string? Notes       { get; private set; }

        public AccountEditForm(List<Root> roots, RootAccount? existing)
        {
            this.Text = existing == null ? "➕ إضافة حساب" : "✏ تعديل حساب";
            this.Size = new Size(440, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(16, 12, 16, 8), BackColor = Color.White };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var cmbRoot  = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontNormal, Margin = new Padding(0, 6, 0, 6) };
            var txtCode  = new TextBox  { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };
            var txtName  = new TextBox  { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };
            var txtNotes = new TextBox  { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };

            cmbRoot.DataSource = roots; cmbRoot.DisplayMember = "Name"; cmbRoot.ValueMember = "Code";
            if (existing != null) { txtCode.Text = existing.Code; txtName.Text = existing.Name; txtNotes.Text = existing.Notes; cmbRoot.SelectedValue = existing.TypeAccount ?? roots[0].Code; }

            void Row(int r, string lbl, Control ctrl)
            { tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, r); tbl.Controls.Add(ctrl, 1, r); }
            Row(0, "الحساب الرئيسي:", cmbRoot);
            Row(1, "كود الحساب:",     txtCode);
            Row(2, "اسم الحساب:",     txtName);
            Row(3, "ملاحظات:",         txtNotes);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White };
            var btnSave = UIHelper.MakeButton("💾 حفظ", AppTheme.Accent, new Size(120, 36), Point.Empty); btnSave.Margin = new Padding(0, 4, 0, 0);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(110, 36), Point.Empty); btnCancel.Margin = new Padding(0, 4, 8, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
                { UIHelper.ShowError("الكود والاسم مطلوبان"); this.DialogResult = DialogResult.None; return; }
                AccountCode = txtCode.Text.Trim(); AccountName = txtName.Text.Trim();
                RootCode = cmbRoot.SelectedValue?.ToString() ?? ""; Notes = txtNotes.Text.Trim();
            };
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(new Label(), 0, 4);
            tbl.Controls.Add(btnFlow, 1, 4);

            this.Controls.Add(tbl);
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }
    }
}
