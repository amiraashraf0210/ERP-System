using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class StoresForm : BaseListForm
    {
        private List<Store> _all = new();
        public StoresForm() : base("المخازن") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Stores.OrderBy(s => s.Code).ToList();
            grid.DataSource = _all.Select(s => new { Id = s.Id, الكود = s.Code, الاسم = s.StoreName, المسؤول = s.Person, تفاصيل = s.Details, ملاحظات = s.Notes }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("إضافة مخزن", new[] { "الكود", "الاسم *", "المسؤول", "تفاصيل", "ملاحظات" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(f.Values[1])) { UIHelper.ShowError("اسم المخزن مطلوب"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Stores.Add(new Store { Code = int.TryParse(f.Values[0], out int c) ? c : 0, StoreName = f.Values[1], Person = f.Values[2], Details = f.Values[3], Notes = f.Values[4] });
            db.SaveChanges(); LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var s = db.Stores.Find(id)!;
            using var f = new SimpleEditForm("تعديل مخزن", new[] { "الكود", "الاسم *", "المسؤول", "تفاصيل", "ملاحظات" },
                new[] { s.Code.ToString(), s.StoreName, s.Person ?? "", s.Details ?? "", s.Notes ?? "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            s.Code = int.TryParse(f.Values[0], out int c) ? c : 0;
            s.StoreName = f.Values[1]; s.Person = f.Values[2]; s.Details = f.Values[3]; s.Notes = f.Values[4];
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا المخزن؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var s = db.Stores.Find(id);
            if (s != null) { db.Stores.Remove(s); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(s => s.StoreName.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(s => new { Id = s.Id, الكود = s.Code, الاسم = s.StoreName, المسؤول = s.Person }).ToList();
    }

    public class StoreTransferForm : Form
    {
        private ComboBox cmbFrom = null!, cmbTo = null!, cmbGood = null!;
        private TextBox txtQty = null!, txtNotes = null!;
        private DataGridView grid = null!;

        public StoreTransferForm()
        {
            this.Text = "تحويل بين المخازن";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            BuildLayout();
            LoadLookups();
            LoadHistory();
        }

        private void BuildLayout()
        {
            var pnlForm = new Panel { Dock = DockStyle.Top, Height = 175, BackColor = AppTheme.Surface, Padding = new Padding(16, 12, 16, 8) };
            pnlForm.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlForm.Height - 1, pnlForm.Width, pnlForm.Height - 1);

            var lblTitle = new Label { Text = "تحويل صنف بين مخزنين", Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleRight };

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 3, BackColor = Color.Transparent };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            for (int i = 0; i < 3; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            Label Lb(string t) => new Label { Text = t, Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark, TextAlign = ContentAlignment.MiddleRight };
            ComboBox Cb() => new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 8, 4) };
            TextBox Tb() => new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 8, 4) };

            cmbFrom  = Cb(); cmbTo = Cb(); cmbGood = Cb();
            txtQty   = Tb(); txtNotes = Tb();

            tbl.Controls.Add(Lb("من مخزن:"),  0, 0); tbl.Controls.Add(cmbFrom,  1, 0);
            tbl.Controls.Add(Lb("إلى مخزن:"), 2, 0); tbl.Controls.Add(cmbTo,    3, 0);
            tbl.Controls.Add(Lb("الصنف:"),    0, 1); tbl.Controls.Add(cmbGood,  1, 1);
            tbl.Controls.Add(Lb("الكمية:"),   2, 1); tbl.Controls.Add(txtQty,   3, 1);
            tbl.Controls.Add(Lb("ملاحظات:"),  0, 2); tbl.Controls.Add(txtNotes, 1, 2); tbl.SetColumnSpan(txtNotes, 3);

            var btnSave = UIHelper.MakeButton("تنفيذ التحويل", AppTheme.Accent, new Size(150, 34), Point.Empty);
            btnSave.Dock = DockStyle.Fill; btnSave.Margin = new Padding(0, 4, 0, 4);
            btnSave.Click += (s, e) => SaveTransfer();
            tbl.Controls.Add(Lb(""), 4, 2); tbl.Controls.Add(btnSave, 5, 2);

            pnlForm.Controls.Add(tbl);
            pnlForm.Controls.Add(lblTitle);

            grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);

            this.Controls.Add(grid);
            this.Controls.Add(pnlForm);
        }

        private void LoadLookups()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var stores = db.Stores.OrderBy(s => s.StoreName).ToList();
            cmbFrom.DataSource = stores.ToList();
            cmbFrom.DisplayMember = "StoreName";
            cmbFrom.ValueMember = "Id";

            cmbTo.DataSource = stores.ToList();
            cmbTo.DisplayMember = "StoreName";
            cmbTo.ValueMember = "Id";

            var goods = db.Goods.OrderBy(g => g.Name).ToList();
            cmbGood.DataSource = goods;
            cmbGood.DisplayMember = "Name";
            cmbGood.ValueMember = "Id";
        }

        private void LoadHistory()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var list = db.StoreMovements
                .Include(sm => sm.StoreFromNav)
                .Include(sm => sm.StoreToNav)
                .OrderByDescending(sm => sm.Date)
                .Take(200)
                .ToList()
                .Select(sm => new
                {
                    sm.Id,
                    التاريخ = sm.Date.ToString("yyyy/MM/dd"),
                    من = sm.StoreFromNav?.StoreName ?? sm.StoreFrom.ToString(),
                    إلى = sm.StoreToNav?.StoreName ?? sm.StoreTo.ToString(),
                    الكمية = sm.Value.ToString("N2"),
                    ملاحظات = sm.Notes ?? ""
                }).ToList();
            grid.DataSource = list;
            if (grid.Columns["Id"] is DataGridViewColumn col) col.Visible = false;
        }

        private void SaveTransfer()
        {
            if (cmbFrom.SelectedValue is not int fromId || cmbTo.SelectedValue is not int toId)
            { UIHelper.ShowError("اختر المخازن"); return; }
            if (fromId == toId) { UIHelper.ShowError("المخزنان يجب أن يكونا مختلفين"); return; }
            if (cmbGood.SelectedValue is not int goodId) { UIHelper.ShowError("اختر الصنف"); return; }
            if (!double.TryParse(txtQty.Text, out double qty) || qty <= 0)
            { UIHelper.ShowError("أدخل كمية صحيحة"); return; }

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var good = db.Goods.Find(goodId);
            if (good == null) { UIHelper.ShowError("الصنف غير موجود"); return; }

            var fromStore = db.Stores.Find(fromId)!;
            var toStore   = db.Stores.Find(toId)!;

            // حساب المخزون المتاح في مخزن المصدر تحديداً
            var stockIn  = db.Movements.Where(m => m.GoodId == goodId && !m.Out && m.StoreNo == fromStore.Code).Sum(m => (double?)m.Quantity) ?? 0;
            var stockOut = db.Movements.Where(m => m.GoodId == goodId && m.Out  && m.StoreNo == fromStore.Code).Sum(m => (double?)m.Quantity) ?? 0;
            var available = stockIn - stockOut;
            if (qty > available)
            {
                UIHelper.ShowError($"الكمية المتاحة في مخزن {fromStore.StoreName}: {available:N2}");
                return;
            }

            var nextCode = (db.StoreMovements.Max(sm => (int?)sm.Code) ?? 0) + 1;
            var now = DateTime.Now;

            db.StoreMovements.Add(new StoreMovement
            {
                Code = nextCode,
                Date = now,
                StoreFrom = fromStore.Code,
                StoreTo = toStore.Code,
                Value = (float)qty,
                Notes = txtNotes.Text.Trim(),
                Reference = goodId
            });

            // Get current fiscal year
            int fiscalYearId = MainForm.CurrentFiscalYearId ?? 
                db.FiscalYears.FirstOrDefault(f => f.Year == now.Year && !f.IsClosed)?.Id ?? 
                db.FiscalYears.OrderByDescending(f => f.Year).FirstOrDefault()?.Id ?? 1;

            db.Movements.Add(new Movement
            {
                GoodId = goodId,
                Quantity = qty,
                Date = now,
                Out = true,
                Move = true,
                StoreNo = fromStore.Code,
                StoreNo2 = toStore.Code,
                Notes = $"تحويل إلى {toStore.StoreName}",
                BillNo = nextCode.ToString(),
                FiscalYearId = fiscalYearId
            });
            db.Movements.Add(new Movement
            {
                GoodId = goodId,
                Quantity = qty,
                Date = now,
                Out = false,
                Move = true,
                StoreNo = toStore.Code,
                StoreNo2 = fromStore.Code,
                Notes = $"تحويل من {fromStore.StoreName}",
                BillNo = nextCode.ToString(),
                FiscalYearId = fiscalYearId
            });

            if (good.StoreId == fromId)
                good.StoreId = toId;

            db.SaveChanges();
            UIHelper.ShowSuccess($"تم التحويل بنجاح — رقم {nextCode}");
            txtQty.Text = "";
            txtNotes.Text = "";
            LoadHistory();
        }
    }
}
