using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class FiscalYearForm : BaseListForm
    {
        private List<FiscalYear> _all = new();
        public FiscalYearForm() : base("السنوات المالية") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.FiscalYears.OrderByDescending(f => f.Year).ToList();
            BindGrid(_all);
        }

        private void BindGrid(List<FiscalYear> list) =>
            grid.DataSource = list.Select(f => new
            {
                Id       = f.Id,
                السنة    = f.Year,
                من       = f.StartDate.ToString("yyyy/MM/dd"),
                إلى      = f.EndDate.ToString("yyyy/MM/dd"),
                الحالة   = f.IsClosed ? "🔒 مغلقة" : "✅ نشطة",
                ملاحظات  = f.Notes
            }).ToList();

        protected override void OnAdd()
        {
            using var f = new FiscalYearEditForm(null);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new FiscalYearEditForm(id);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var fy = db.FiscalYears.Find(id);
            if (fy == null) return;
            if (fy.IsClosed) { UIHelper.ShowError("لا يمكن حذف سنة مالية مغلقة"); return; }
            if (!UIHelper.Confirm($"حذف السنة المالية {fy.Year}؟")) return;
            db.FiscalYears.Remove(fy);
            db.SaveChanges();
            LoadData();
        }

        protected override void AddExtraButtons(FlowLayoutPanel toolbar)
        {
            var btnClose = UIHelper.MakeButton("🔒 إغلاق السنة", AppTheme.Warning, new Size(140, 36), Point.Empty);
            btnClose.Margin = new Padding(0, 0, 8, 0);
            btnClose.Click += (s, e) => CloseFiscalYear();
            toolbar.Controls.Add(btnClose);
        }

        private void CloseFiscalYear()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var fy = db.FiscalYears.Find(id);
            if (fy == null) return;
            if (fy.IsClosed) { UIHelper.ShowError("هذه السنة مغلقة مسبقاً"); return; }
            if (!UIHelper.Confirm($"إغلاق السنة المالية {fy.Year}؟\nلن تستطيع إضافة قيود عليها بعد الإغلاق.")) return;
            fy.IsClosed = true;
            db.SaveChanges();
            UIHelper.ShowSuccess($"✅ تم إغلاق السنة المالية {fy.Year}");
            LoadData();
        }

        protected override void OnSearch(string k) =>
            BindGrid(string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(f => f.Year.ToString().Contains(k) || (f.Notes ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    internal class FiscalYearEditForm : Form
    {
        private readonly int? _id;

        public FiscalYearEditForm(int? id)
        {
            _id = id;
            BuildUI();
            if (id != null)
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var fy = db.FiscalYears.Find(id);
                if (fy != null) FillFields(fy);
            }
        }

        private NumericUpDown nudYear = null!;
        private DateTimePicker dtpFrom = null!, dtpTo = null!;
        private TextBox txtNotes = null!;
        private CheckBox chkClosed = null!;

        private void BuildUI()
        {
            this.Text = _id == null ? "➕ سنة مالية جديدة" : "✏ تعديل سنة مالية";
            this.Size = new Size(440, 310);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6,
                Padding = new Padding(16, 12, 16, 8), BackColor = Color.White
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            nudYear  = new NumericUpDown { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year, Margin = new Padding(0, 6, 0, 6) };
            dtpFrom  = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Today.Year, 1, 1), Margin = new Padding(0, 6, 0, 6) };
            dtpTo    = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Today.Year, 12, 31), Margin = new Padding(0, 6, 0, 6) };
            txtNotes = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 0, 6) };
            chkClosed= new CheckBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = "مغلقة", Margin = new Padding(0, 8, 0, 0) };

            nudYear.ValueChanged += (s, e) =>
            {
                int y = (int)nudYear.Value;
                dtpFrom.Value = new DateTime(y, 1, 1);
                dtpTo.Value   = new DateTime(y, 12, 31);
            };

            void Row(int r, string lbl, Control ctrl)
            {
                tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, r);
                tbl.Controls.Add(ctrl, 1, r);
            }
            Row(0, "السنة:",       nudYear);
            Row(1, "تاريخ البدء:", dtpFrom);
            Row(2, "تاريخ الانتهاء:", dtpTo);
            Row(3, "ملاحظات:",     txtNotes);
            Row(4, "الحالة:",      chkClosed);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White };
            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(120, 36), Point.Empty); btnSave.Margin   = new Padding(0, 4, 0, 0);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger,  new Size(110, 36), Point.Empty); btnCancel.Margin = new Padding(0, 4, 8, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += BtnSave_Click;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(new Label(), 0, 5);
            tbl.Controls.Add(btnFlow, 1, 5);

            this.Controls.Add(tbl);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void FillFields(FiscalYear fy)
        {
            nudYear.Value  = fy.Year;
            dtpFrom.Value  = fy.StartDate;
            dtpTo.Value    = fy.EndDate;
            txtNotes.Text  = fy.Notes ?? "";
            chkClosed.Checked = fy.IsClosed;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var fy = _id == null ? new FiscalYear() : db.FiscalYears.Find(_id)!;
            fy.Year      = (int)nudYear.Value;
            fy.StartDate = dtpFrom.Value.Date;
            fy.EndDate   = dtpTo.Value.Date;
            fy.Notes     = txtNotes.Text.Trim();
            fy.IsClosed  = chkClosed.Checked;
            if (_id == null) db.FiscalYears.Add(fy);
            db.SaveChanges();
            UIHelper.ShowSuccess("تم الحفظ ✅");
        }
    }
}
