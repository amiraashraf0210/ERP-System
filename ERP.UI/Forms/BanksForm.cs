using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class BanksForm : Form
    {
        private DataGridView gridBanks = null!, gridLoans = null!;
        private List<Bank> _banks = new();
        private Label lblBankSummary = null!, lblLoanSummary = null!;

        public BanksForm()
        {
            this.Text = "البنوك والقروض";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            BuildLayout();
        }

        private void BuildLayout()
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 220,
                BackColor = AppTheme.Light,
                Panel1MinSize = 150,
                Panel2MinSize = 150
            };
            split.Panel1.Controls.Add(BuildBanksPanel());
            split.Panel2.Controls.Add(BuildLoansPanel());
            this.Controls.Add(split);
            LoadBanks();
        }

        private Panel BuildBanksPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Light };
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(6, 5, 6, 0) };
            var lblTitle  = new Label { Text = "🏦 البنوك", AutoSize = true, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, Margin = new Padding(0, 8, 0, 0) };
            var btnAdd    = UIHelper.MakeButton("➕ إضافة",  AppTheme.Accent,  new Size(100, 36), Point.Empty); btnAdd.Margin    = new Padding(0, 0, 8, 0);
            var btnEdit   = UIHelper.MakeButton("✏ تعديل",  AppTheme.Primary, new Size(100, 36), Point.Empty); btnEdit.Margin   = new Padding(0, 0, 8, 0);
            var btnDelete = UIHelper.MakeButton("🗑 حذف",   AppTheme.Danger,  new Size(100, 36), Point.Empty); btnDelete.Margin = new Padding(0, 0, 8, 0);
            btnAdd.Click    += (s, e) => AddBank();
            btnEdit.Click   += (s, e) => EditBank();
            btnDelete.Click += (s, e) => DeleteBank();
            flow.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, lblTitle });
            toolbar.Controls.Add(flow);
            lblBankSummary = new Label { Dock = DockStyle.Top, Height = 28, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0), BackColor = Color.FromArgb(239, 246, 255) };
            gridBanks = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(gridBanks);
            gridBanks.DataBindingComplete += (s, e) => { if (gridBanks.Columns["Id"] is DataGridViewColumn col) col.Visible = false; };
            gridBanks.SelectionChanged += (s, e) => LoadLoans();
            pnl.Controls.Add(UIHelper.WrapGrid(gridBanks));
            pnl.Controls.Add(lblBankSummary);
            pnl.Controls.Add(toolbar);
            return pnl;
        }

        private Panel BuildLoansPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Light };
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(6, 5, 6, 0) };
            var lblTitle  = new Label { Text = "💳 القروض والالتزامات", AutoSize = true, Font = AppTheme.FontBold, ForeColor = AppTheme.Warning, Margin = new Padding(0, 8, 0, 0) };
            var btnAdd    = UIHelper.MakeButton("➕ قرض جديد",  AppTheme.Warning, new Size(120, 36), Point.Empty); btnAdd.Margin    = new Padding(0, 0, 8, 0);
            var btnPay    = UIHelper.MakeButton("💵 سداد دفعة", AppTheme.Accent,  new Size(130, 36), Point.Empty); btnPay.Margin    = new Padding(0, 0, 8, 0);
            var btnDetail = UIHelper.MakeButton("📋 تفاصيل",    AppTheme.Primary, new Size(100, 36), Point.Empty); btnDetail.Margin = new Padding(0, 0, 8, 0);
            var btnDelete = UIHelper.MakeButton("🗑 حذف",       AppTheme.Danger,  new Size(100, 36), Point.Empty); btnDelete.Margin = new Padding(0, 0, 8, 0);
            btnAdd.Click    += (s, e) => AddLoan();
            btnPay.Click    += (s, e) => PayLoan();
            btnDetail.Click += (s, e) => LoanDetails();
            btnDelete.Click += (s, e) => DeleteLoan();
            flow.Controls.AddRange(new Control[] { btnAdd, btnPay, btnDetail, btnDelete, lblTitle });
            toolbar.Controls.Add(flow);
            lblLoanSummary = new Label { Dock = DockStyle.Top, Height = 28, Font = AppTheme.FontBold, ForeColor = AppTheme.Danger, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0), BackColor = Color.FromArgb(255, 240, 240) };
            gridLoans = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(gridLoans);
            gridLoans.DataBindingComplete += (s, e) => { if (gridLoans.Columns["Id"] is DataGridViewColumn col) col.Visible = false; };
            pnl.Controls.Add(UIHelper.WrapGrid(gridLoans));
            pnl.Controls.Add(lblLoanSummary);
            pnl.Controls.Add(toolbar);
            return pnl;
        }

        private void LoadBanks()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _banks = db.Banks.ToList();
            gridBanks.DataSource = _banks.Select(b => new { Id = b.Id, الكود = b.Code, البنك = b.BankName, رقم_الحساب = b.AccountNo, ملاحظات = b.Notes }).ToList();
            lblBankSummary.Text = $"عدد البنوك: {_banks.Count}";
            LoadLoans();
        }

        private void LoadLoans()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            int? bankId = GetSelectedBankId();
            var query = db.BankLoans.Include(l => l.Bank).Include(l => l.Payments).AsQueryable();
            if (bankId.HasValue) query = query.Where(l => l.BankId == bankId.Value);
            var loans = query.OrderByDescending(l => l.LoanDate).ToList();
            gridLoans.DataSource = loans.Select(l => new
            {
                Id      = l.Id,
                الكود   = l.LoanCode,
                البنك   = l.Bank?.BankName ?? "",
                النوع   = l.LoanType == "Loan" ? "قرض 🔴" : "إيداع 🟢",
                المبلغ  = l.Amount.ToString("N2"),
                المسدد  = l.TotalPaid.ToString("N2"),
                المتبقي = l.Remaining.ToString("N2"),
                الحالة  = l.Status == "Active" ? "✅ نشط" : "🔒 مسدد",
                التاريخ = l.LoanDate.ToString("yyyy/MM/dd"),
                ملاحظات = l.Notes
            }).ToList();
            double totalLoans = loans.Where(l => l.LoanType == "Loan").Sum(l => l.Amount);
            double totalPaid  = loans.Where(l => l.LoanType == "Loan").Sum(l => l.TotalPaid);
            lblLoanSummary.Text = $"إجمالي القروض: {totalLoans:N2}  |  المسدد: {totalPaid:N2}  |  المتبقي: {(totalLoans - totalPaid):N2}";
        }

        private int? GetSelectedBankId()
        {
            if (gridBanks.SelectedRows.Count == 0) return null;
            var row = gridBanks.SelectedRows[0];
            return (gridBanks.Columns.Contains("Id") && row.Cells["Id"].Value is int id && id > 0) ? id : null;
        }

        private int? GetSelectedLoanId()
        {
            if (gridLoans.SelectedRows.Count == 0) { UIHelper.ShowError("اختر قرضاً أولاً"); return null; }
            var row = gridLoans.SelectedRows[0];
            return (gridLoans.Columns.Contains("Id") && row.Cells["Id"].Value is int id) ? id : null;
        }

        private void AddBank()
        {
            using var f = new SimpleEditForm("إضافة بنك", new[] { "الكود", "اسم البنك *", "رقم الحساب" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(f.Values[1])) { UIHelper.ShowError("اسم البنك مطلوب"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Banks.Add(new Bank { Code = f.Values[0], BankName = f.Values[1], AccountNo = f.Values[2] });
            db.SaveChanges(); LoadBanks();
        }

        private void EditBank()
        {
            var id = GetSelectedBankId(); if (id == null) { UIHelper.ShowError("اختر بنكاً أولاً"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var b = db.Banks.Find(id)!;
            using var f = new SimpleEditForm("تعديل بنك", new[] { "الكود", "اسم البنك *", "رقم الحساب" }, new[] { b.Code, b.BankName, b.AccountNo ?? "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            b.Code = f.Values[0]; b.BankName = f.Values[1]; b.AccountNo = f.Values[2];
            db.SaveChanges(); LoadBanks();
        }

        private void DeleteBank()
        {
            var id = GetSelectedBankId(); if (id == null) { UIHelper.ShowError("اختر بنكاً أولاً"); return; }
            if (!UIHelper.Confirm("حذف هذا البنك وكل قروضه؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var loans = db.BankLoans.Where(l => l.BankId == id).ToList();
            foreach (var l in loans) db.LoanPayments.RemoveRange(db.LoanPayments.Where(p => p.LoanId == l.Id).ToList());
            db.BankLoans.RemoveRange(loans);
            var b = db.Banks.Find(id); if (b != null) db.Banks.Remove(b);
            db.SaveChanges(); LoadBanks();
        }

        private void AddLoan()
        {
            var bankId = GetSelectedBankId(); if (bankId == null) { UIHelper.ShowError("اختر بنكاً أولاً"); return; }
            using var f = new LoanEditForm(bankId.Value);
            if (f.ShowDialog(this) == DialogResult.OK) LoadLoans();
        }

        private void PayLoan()
        {
            var id = GetSelectedLoanId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var loan = db.BankLoans.Include(l => l.Payments).FirstOrDefault(l => l.Id == id);
            if (loan == null) return;
            if (loan.Status == "Settled") { UIHelper.ShowError("هذا القرض مسدد بالكامل"); return; }
            using var f = new LoanPaymentForm(loan);
            if (f.ShowDialog(this) == DialogResult.OK) LoadLoans();
        }

        private void LoanDetails()
        {
            var id = GetSelectedLoanId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var loan = db.BankLoans.Include(l => l.Bank).Include(l => l.Payments).FirstOrDefault(l => l.Id == id);
            if (loan == null) return;
            using var f = new LoanDetailsForm(loan);
            f.ShowDialog(this);
        }

        private void DeleteLoan()
        {
            var id = GetSelectedLoanId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا القرض وكل دفعاته؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.LoanPayments.RemoveRange(db.LoanPayments.Where(p => p.LoanId == id).ToList());
            var l = db.BankLoans.Find(id); if (l != null) db.BankLoans.Remove(l);
            db.SaveChanges(); LoadLoans();
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); LoadBanks(); }
    }

    internal class LoanEditForm : Form
    {
        private readonly int _bankId;
        public LoanEditForm(int bankId)
        {
            _bankId = bankId;
            this.Text = "➕ قرض / إيداع جديد";
            this.Size = new Size(440, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(16, 12, 16, 8), BackColor = Color.White };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var txtCode   = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6), Text = $"LN{DateTime.Now:yyMMddHHmm}" };
            var cboType   = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 6, 0, 6) };
            cboType.Items.AddRange(new object[] { "قرض (علينا)", "إيداع (لنا)" });
            cboType.SelectedIndex = 0;
            var txtAmount = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6), Text = "0" };
            var dtpDate   = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 6, 0, 6) };
            var txtNotes  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };

            void Row(int r, string lbl, Control ctrl)
            { tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, r); tbl.Controls.Add(ctrl, 1, r); }
            Row(0, "الكود:",      txtCode);
            Row(1, "النوع:",      cboType);
            Row(2, "المبلغ *:",   txtAmount);
            Row(3, "التاريخ:",    dtpDate);
            Row(4, "ملاحظات:",    txtNotes);

            var btnFlow   = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White };
            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(120, 36), Point.Empty); btnSave.Margin   = new Padding(0, 4, 0, 0);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger,  new Size(110, 36), Point.Empty); btnCancel.Margin = new Padding(0, 4, 8, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                if (!double.TryParse(txtAmount.Text, out double amt) || amt <= 0) { UIHelper.ShowError("أدخل مبلغاً صحيحاً"); this.DialogResult = DialogResult.None; return; }
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                db.BankLoans.Add(new BankLoan { BankId = _bankId, LoanCode = txtCode.Text.Trim(), LoanType = cboType.SelectedIndex == 0 ? "Loan" : "Deposit", Amount = amt, LoanDate = dtpDate.Value.Date, Notes = txtNotes.Text.Trim(), Status = "Active" });
                db.SaveChanges();
                UIHelper.ShowSuccess("✅ تم إضافة القرض");
            };
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(new Label(), 0, 5); tbl.Controls.Add(btnFlow, 1, 5);
            this.Controls.Add(tbl);
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }
    }

    internal class LoanPaymentForm : Form
    {
        private readonly BankLoan _loan;
        public LoanPaymentForm(BankLoan loan)
        {
            _loan = loan;
            this.Text = $"💵 سداد — {loan.LoanCode}";
            this.Size = new Size(420, 270);
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

            var lblRemain = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.Danger, TextAlign = ContentAlignment.MiddleRight, Text = $"{loan.Remaining:N2}" };
            var txtAmount = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6), Text = loan.Remaining.ToString("N2") };
            var dtpDate   = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 6, 0, 6) };
            var txtNotes  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };

            void Row(int r, string lbl, Control ctrl)
            { tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, r); tbl.Controls.Add(ctrl, 1, r); }
            Row(0, "المتبقي:",       lblRemain);
            Row(1, "مبلغ الدفعة *:", txtAmount);
            Row(2, "التاريخ:",        dtpDate);
            Row(3, "ملاحظات:",        txtNotes);

            var btnFlow   = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White };
            var btnSave   = UIHelper.MakeButton("💵 سداد",  AppTheme.Accent, new Size(120, 36), Point.Empty); btnSave.Margin   = new Padding(0, 4, 0, 0);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger,  new Size(110, 36), Point.Empty); btnCancel.Margin = new Padding(0, 4, 8, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                if (!double.TryParse(txtAmount.Text, out double amt) || amt <= 0) { UIHelper.ShowError("أدخل مبلغاً صحيحاً"); this.DialogResult = DialogResult.None; return; }
                if (amt > loan.Remaining + 0.01) { UIHelper.ShowError($"المبلغ أكبر من المتبقي ({loan.Remaining:N2})"); this.DialogResult = DialogResult.None; return; }
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                int fiscalYearId = MainForm.CurrentFiscalYearId
                    ?? db.FiscalYears.Where(f => !f.IsClosed).OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();
                if (fiscalYearId == 0) fiscalYearId = db.FiscalYears.OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();

                var box = new BoxTransaction { Out = true, Value = amt, Date = dtpDate.Value.Date, Time = DateTime.Now, Notes = $"سداد قرض {loan.LoanCode}", No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1, FiscalYearId = fiscalYearId };
                db.BoxTransactions.Add(box);
                db.SaveChanges();
                db.LoanPayments.Add(new LoanPayment { LoanId = loan.Id, Amount = amt, PayDate = dtpDate.Value.Date, Notes = txtNotes.Text.Trim(), BoxTransactionId = box.Id });
                var loanDb = db.BankLoans.Include(l => l.Payments).First(l => l.Id == loan.Id);
                if (loanDb.TotalPaid + amt >= loanDb.Amount - 0.01) loanDb.Status = "Settled";
                db.SaveChanges();
                UIHelper.ShowSuccess($"✅ تم السداد: {amt:N2}\nتم خصمه من الخزينة تلقائياً");
            };
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(new Label(), 0, 4); tbl.Controls.Add(btnFlow, 1, 4);
            this.Controls.Add(tbl);
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }
    }

    internal class LoanDetailsForm : Form
    {
        public LoanDetailsForm(BankLoan loan)
        {
            this.Text = $"📋 تفاصيل — {loan.LoanCode}";
            this.Size = new Size(600, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            var pnlInfo = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White, Padding = new Padding(12, 8, 12, 8) };
            pnlInfo.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlInfo.Height - 1, pnlInfo.Width, pnlInfo.Height - 1);
            var lbl = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight };
            lbl.Text = $"البنك: {loan.Bank?.BankName}  |  المبلغ: {loan.Amount:N2}  |  المسدد: {loan.TotalPaid:N2}  |  المتبقي: {loan.Remaining:N2}  |  {(loan.Status == "Active" ? "✅ نشط" : "🔒 مسدد")}";
            pnlInfo.Controls.Add(lbl);
            var grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.DataSource = loan.Payments.OrderBy(p => p.PayDate).Select(p => new { التاريخ = p.PayDate.ToString("yyyy/MM/dd"), المبلغ = p.Amount.ToString("N2"), ملاحظات = p.Notes }).ToList();
            this.Controls.Add(UIHelper.WrapGrid(grid));
            this.Controls.Add(pnlInfo);
        }
    }
}
