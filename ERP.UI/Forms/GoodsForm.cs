using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    public class GoodsForm : BaseListForm
    {
        private List<Good> _all = new();
        public GoodsForm() : base("الأصناف") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Goods.Include(g => g.Group).Include(g => g.Unit)
                .Include(g => g.Importer).OrderBy(g => g.Code).ToList();
            BindGrid(_all);
        }

        private void BindGrid(List<Good> list) =>
            grid.DataSource = list.Select(g => new
            {
                Id = g.Id, الكود = g.Code, الاسم = g.Name,
                المجموعة = g.Group?.GroupName, الوحدة = g.Unit?.UnitName,
                المورد = g.Importer?.Name,
                سعر_البيع = g.SellPrice.ToString("N2"),
                سعر_الشراء = g.BuyPrice.ToString("N2"),
                الحد_الأدنى = g.MinStock, الحد_الأقصى = g.MaxStock
            }).ToList();

        protected override void OnAdd()
        {
            using var f = new GoodEditForm(null);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new GoodEditForm(id);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا الصنف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.Goods.Find(id);
            if (g != null) { db.Goods.Remove(g); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            BindGrid(string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(g => g.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || g.Code.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    // ─────────────────────────── GOOD EDIT ───────────────────────────
    public class GoodEditForm : Form
    {
        private readonly int? _goodId;
        private TextBox txtCode = null!, txtName = null!, txtSize = null!, txtNotes = null!;
        private TextBox txtSellPrice = null!, txtBuyPrice = null!, txtMinStock = null!, txtMaxStock = null!;
        private TextBox txtSellPriceSP = null!, txtHalfPrice = null!, txtCustPrice = null!;
        private ComboBox cboGroup = null!, cboUnit = null!, cboImporter = null!,
                         cboModel = null!, cboColor = null!, cboMarket = null!, cboStore = null!;

        public GoodEditForm(int? goodId)
        {
            _goodId = goodId;
            BuildUI();
            LoadCombos();
            if (goodId != null)
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var g = db.Goods.Find(goodId);
                if (g != null) FillFields(g);
            }
        }

        private void BuildUI()
        {
            this.Text = _goodId == null ? "➕ صنف جديد" : "✏ تعديل صنف";
            this.Size = new Size(540, 620);
            this.MinimumSize = new Size(520, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Glass effect panel
            var pnlGlass = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(200, 255, 255, 255),
                Padding = new Padding(2)
            };
            pnlGlass.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(pnlGlass.ClientRectangle, 
                    Color.FromArgb(220, 240, 255), 
                    Color.FromArgb(255, 255, 255), 
                    LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, pnlGlass.ClientRectangle);
                using var pen = new Pen(Color.FromArgb(100, 150, 200), 2);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlGlass.Width - 1, pnlGlass.Height - 1);
            };

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BackColor = Color.White };

            var tabBasic = new TabPage { Text = "البيانات الأساسية", BackColor = Color.White, Padding = new Padding(8, 6, 8, 6), AutoScroll = true };
            var rows1 = new List<Control>();
            rows1.Add(MakeRow("الكود *",    txtCode     = MakeTxt(), Color.FromArgb(0, 80, 160), Color.FromArgb(230, 240, 255)));
            rows1.Add(MakeRow("الاسم *",    txtName     = MakeTxt(), Color.FromArgb(0, 80, 160), Color.FromArgb(230, 240, 255)));
            rows1.Add(MakeRowPlus("المجموعة",  cboGroup    = MakeCombo(), () => QuickAdd("اسم المجموعة", v => { using var db = Db(); db.GoodGroups.Add(new GoodGroup { GroupName = v }); db.SaveChanges(); }, cboGroup, () => Db().GoodGroups.OrderBy(x => x.GroupName).Select(x => new IdName(x.Id, x.GroupName)).ToList()), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRowPlus("الوحدة",    cboUnit     = MakeCombo(), () => QuickAdd("اسم الوحدة",   v => { using var db = Db(); db.Units.Add(new Unit { UnitName = v }); db.SaveChanges(); },              cboUnit,  () => Db().Units.OrderBy(x => x.UnitName).Select(x => new IdName(x.Id, x.UnitName)).ToList()), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRow("المورد",     cboImporter = MakeCombo(), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRowPlus("الموديل",   cboModel    = MakeCombo(), () => QuickAdd("اسم الموديل",  v => { using var db = Db(); db.GoodModels.Add(new GoodModel { ModelName = v }); db.SaveChanges(); },   cboModel, () => Db().GoodModels.OrderBy(x => x.ModelName).Select(x => new IdName(x.Id, x.ModelName)).ToList()), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRowPlus("اللون",     cboColor    = MakeCombo(), () => QuickAdd("اسم اللون",    v => { using var db = Db(); db.Colors.Add(new GoodColor { ColorName = v }); db.SaveChanges(); },       cboColor, () => Db().Colors.OrderBy(x => x.ColorName).Select(x => new IdName(x.Id, x.ColorName)).ToList()), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRowPlus("السوق",     cboMarket   = MakeCombo(), () => QuickAdd("اسم السوق",    v => { using var db = Db(); db.Markets.Add(new Market { MarketName = v }); db.SaveChanges(); },       cboMarket,() => Db().Markets.OrderBy(x => x.MarketName).Select(x => new IdName(x.Id, x.MarketName)).ToList()), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRow("المقاس",     txtSize     = MakeTxt(), Color.FromArgb(80, 80, 80), Color.FromArgb(245, 245, 245)));
            rows1.Add(MakeRow("المخزن",     cboStore    = MakeCombo(), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)));
            rows1.Add(MakeRow("ملاحظات",    txtNotes    = MakeTxt(), Color.FromArgb(80, 80, 80), Color.FromArgb(245, 245, 245)));
            int y1 = 6;
            foreach (var row in rows1) { row.Top = y1; tabBasic.Controls.Add(row); y1 += 40; }

            var tabPrices = new TabPage { Text = "الأسعار", BackColor = Color.White, Padding = new Padding(8, 6, 8, 6) };
            var rows2 = new List<Control>
            {
                MakeRow("سعر البيع *",     txtSellPrice   = MakeTxt("0"), Color.FromArgb(0, 150, 0), Color.FromArgb(220, 255, 220)),
                MakeRow("سعر الشراء",      txtBuyPrice    = MakeTxt("0"), Color.FromArgb(200, 120, 0), Color.FromArgb(255, 240, 200)),
                MakeRow("سعر الجملة",      txtSellPriceSP = MakeTxt("0"), Color.FromArgb(0, 150, 0), Color.FromArgb(220, 255, 220)),
                MakeRow("نصف جملة",        txtHalfPrice   = MakeTxt("0"), Color.FromArgb(0, 150, 0), Color.FromArgb(220, 255, 220)),
                MakeRow("سعر خاص للعميل", txtCustPrice   = MakeTxt("0"), Color.FromArgb(0, 150, 0), Color.FromArgb(220, 255, 220)),
            };
            int y2 = 6;
            foreach (var row in rows2) { row.Top = y2; tabPrices.Controls.Add(row); y2 += 40; }

            var tabStock = new TabPage { Text = "المخزون", BackColor = Color.White, Padding = new Padding(8, 6, 8, 6) };
            var rows3 = new List<Control>
            {
                MakeRow("الحد الأدنى",  txtMinStock = MakeTxt("0"), Color.FromArgb(200, 120, 0), Color.FromArgb(255, 240, 200)),
                MakeRow("الحد الأقصى",  txtMaxStock = MakeTxt("0"), Color.FromArgb(200, 120, 0), Color.FromArgb(255, 240, 200)),
            };
            int y3 = 6;
            foreach (var row in rows3) { row.Top = y3; tabStock.Controls.Add(row); y3 += 40; }

            tabs.TabPages.AddRange(new[] { tabBasic, tabPrices, tabStock });

            var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
            pnlBtns.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(100, 150, 200)), 0, 0, pnlBtns.Width, 0);
            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White, Padding = new Padding(8, 6, 8, 0) };
            var btnSave   = UIHelper.MakeButton("💾 حفظ",   Color.FromArgb(0, 150, 0), new Size(130, 38), Point.Empty);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", Color.FromArgb(200, 50, 50),  new Size(110, 38), Point.Empty);
            btnSave.Margin = btnCancel.Margin = new Padding(0, 0, 8, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += BtnSave_Click;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            pnlBtns.Controls.Add(btnFlow);

            pnlGlass.Controls.Add(pnlBtns);
            pnlGlass.Controls.Add(tabs);
            this.Controls.Add(pnlGlass);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private static AppDbContext Db() => Program.ServiceProvider.GetRequiredService<AppDbContext>();

        private static Panel MakeRow(string label, Control ctrl, Color labelColor, Color backColor)
        {
            var p = new Panel { Height = 36, Dock = DockStyle.None, Left = 0, Width = 480, BackColor = Color.White, RightToLeft = RightToLeft.Yes };
            var lbl = new Label
            {
                Text = label, Width = 110, Height = 34, Top = 0, Left = 360,
                Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes,
                ForeColor = labelColor, BackColor = Color.White
            };
            ctrl.Left = 4; ctrl.Top = 4; ctrl.Width = 350; ctrl.Height = 28;
            ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            ctrl.RightToLeft = RightToLeft.Yes;
            ctrl.BackColor = backColor;
            if (ctrl is TextBox tb) 
            { 
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.BackColor = backColor;
            }
            if (ctrl is ComboBox cb)
            {
                cb.BackColor = backColor;
                cb.FlatStyle = FlatStyle.Flat;
            }
            p.Controls.Add(lbl);
            p.Controls.Add(ctrl);
            p.Resize += (s, e) =>
            {
                p.Width = p.Parent?.ClientSize.Width - 16 ?? 480;
                lbl.Left = p.Width - 114;
                ctrl.Width = lbl.Left - 8;
            };
            return p;
        }

        private Panel MakeRowPlus(string label, ComboBox cbo, Action onPlus, Color labelColor, Color backColor)
        {
            var p = new Panel { Height = 36, Dock = DockStyle.None, Left = 0, Width = 480, BackColor = Color.White, RightToLeft = RightToLeft.Yes };
            var lbl = new Label
            {
                Text = label, Width = 110, Height = 34, Top = 0, Left = 360,
                Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes,
                ForeColor = labelColor, BackColor = Color.White
            };
            var btnPlus = new Button
            {
                Text = "+", Width = 30, Height = 28, Top = 4, Left = 4,
                BackColor = Color.FromArgb(0, 150, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = AppTheme.FontBold, Cursor = Cursors.Hand
            };
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.Click += (s, e) => onPlus();
            cbo.Left = 38; cbo.Top = 4; cbo.Width = 316; cbo.Height = 28;
            cbo.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            cbo.RightToLeft = RightToLeft.Yes;
            cbo.BackColor = backColor;
            cbo.FlatStyle = FlatStyle.Flat;
            p.Controls.Add(lbl);
            p.Controls.Add(btnPlus);
            p.Controls.Add(cbo);
            p.Resize += (s, e) =>
            {
                p.Width = p.Parent?.ClientSize.Width - 16 ?? 480;
                lbl.Left = p.Width - 114;
                cbo.Width = lbl.Left - btnPlus.Right - 4;
            };
            return p;
        }

        private void QuickAdd(string prompt, Action<string> save, ComboBox cbo, Func<List<IdName>> reload)
        {
            using var dlg = new Form
            {
                Text = "إضافة جديدة", Size = new Size(360, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.White,
                RightToLeft = RightToLeft.Yes, RightToLeftLayout = true
            };

            // Glass effect panel
            var pnlGlass = new Panel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(200, 255, 255, 255),
                Padding = new Padding(2)
            };
            pnlGlass.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(pnlGlass.ClientRectangle, 
                    Color.FromArgb(220, 240, 255), 
                    Color.FromArgb(255, 255, 255), 
                    LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, pnlGlass.ClientRectangle);
                using var pen = new Pen(Color.FromArgb(100, 150, 200), 2);
                e.Graphics.DrawRectangle(pen, 0, 0, pnlGlass.Width - 1, pnlGlass.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(16, 12, 16, 8), BackColor = Color.White
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lbl = new Label
            {
                Text = prompt + ":", Dock = DockStyle.Fill, Font = AppTheme.FontBold,
                TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes,
                ForeColor = Color.FromArgb(0, 80, 160), BackColor = Color.White
            };
            var txt = new TextBox
            {
                Dock = DockStyle.Fill, Font = AppTheme.FontNormal,
                BorderStyle = BorderStyle.FixedSingle, RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 2, 0, 2),
                BackColor = Color.FromArgb(230, 240, 255)
            };

            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.White, WrapContents = false
            };
            var btnOk  = UIHelper.MakeButton("✔ إضافة", Color.FromArgb(0, 150, 0), new Size(110, 34), Point.Empty);
            var btnNo  = UIHelper.MakeButton("✖ إلغاء", Color.FromArgb(200, 50, 50), new Size(100, 34), Point.Empty);
            btnOk.Margin = new Padding(0, 0, 8, 0);
            btnOk.DialogResult = DialogResult.OK;
            btnNo.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.Add(btnOk);
            btnFlow.Controls.Add(btnNo);

            tbl.Controls.Add(lbl,     0, 0);
            tbl.Controls.Add(txt,     0, 1);
            tbl.Controls.Add(btnFlow, 0, 2);
            pnlGlass.Controls.Add(tbl);
            dlg.Controls.Add(pnlGlass);
            dlg.AcceptButton = btnOk; dlg.CancelButton = btnNo;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            string val = txt.Text.Trim();
            if (string.IsNullOrEmpty(val)) return;

            save(val);
            var items = new List<IdName> { new IdName(0, "-- اختر --") };
            items.AddRange(reload());
            cbo.DataSource = null;
            cbo.DataSource = items;
            cbo.DisplayMember = "Name"; cbo.ValueMember = "Id";
            var added = items.FirstOrDefault(x => x.Name == val);
            if (added != null) cbo.SelectedItem = added;
        }

        private static ComboBox MakeCombo() => new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = AppTheme.FontNormal };
        private static TextBox   MakeTxt(string val = "") => new TextBox { Font = AppTheme.FontNormal, Text = val, BorderStyle = BorderStyle.FixedSingle };

        private void LoadCombos()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            BindCombo(cboGroup,    db.GoodGroups.OrderBy(g => g.GroupName).Select(g => new IdName(g.Id, g.GroupName)).ToList(), "-- مجموعة --");
            BindCombo(cboUnit,     db.Units.OrderBy(u => u.UnitName).Select(u => new IdName(u.Id, u.UnitName)).ToList(),       "-- وحدة --");
            BindCombo(cboImporter, db.Importers.OrderBy(i => i.Name).Select(i => new IdName(i.Id, i.Name)).ToList(),           "-- مورد --");
            BindCombo(cboModel,    db.GoodModels.OrderBy(m => m.ModelName).Select(m => new IdName(m.Id, m.ModelName)).ToList(),"-- موديل --");
            BindCombo(cboColor,    db.Colors.OrderBy(c => c.ColorName).Select(c => new IdName(c.Id, c.ColorName)).ToList(),    "-- لون --");
            BindCombo(cboMarket,   db.Markets.OrderBy(m => m.MarketName).Select(m => new IdName(m.Id, m.MarketName)).ToList(), "-- سوق --");
            BindCombo(cboStore,    db.Stores.OrderBy(s => s.StoreName).Select(s => new IdName(s.Id, s.StoreName)).ToList(),    "-- مخزن --");
        }

        private static void BindCombo(ComboBox cb, List<IdName> items, string placeholder)
        {
            var all = new List<IdName> { new IdName(0, placeholder) };
            all.AddRange(items);
            cb.DisplayMember = "Name";
            cb.ValueMember   = "Id";
            cb.DataSource    = all;
            cb.SelectedIndex = 0;
        }

        private void FillFields(Good g)
        {
            txtCode.Text = g.Code; txtName.Text = g.Name; txtSize.Text = g.Size; txtNotes.Text = g.Notes;
            txtSellPrice.Text   = g.SellPrice.ToString("N2");
            txtBuyPrice.Text    = g.BuyPrice.ToString("N2");
            txtSellPriceSP.Text = g.SellPriceSP.ToString("N2");
            txtHalfPrice.Text   = g.HalfPrice.ToString("N2");
            txtCustPrice.Text   = g.CustPrice.ToString("N2");
            txtMinStock.Text    = g.MinStock.ToString();
            txtMaxStock.Text    = g.MaxStock.ToString();

            SetCombo(cboGroup,    g.GroupId);
            SetCombo(cboUnit,     g.UnitId);
            SetCombo(cboImporter, g.ImporterId);
            SetCombo(cboModel,    g.ModelId);
            SetCombo(cboColor,    g.ColorId);
            SetCombo(cboMarket,   g.MarketId);
            SetCombo(cboStore,    g.StoreId);
        }

        private static void SetCombo(ComboBox cb, int? id)
        {
            if (id == null || id == 0) return;
            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (cb.Items[i] is IdName item && item.Id == id)
                { cb.SelectedIndex = i; return; }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { UIHelper.ShowError("اسم الصنف مطلوب"); this.DialogResult = DialogResult.None; return; }

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = _goodId == null ? new Good() : db.Goods.Find(_goodId)!;
            g.Code = txtCode.Text.Trim(); g.Name = txtName.Text.Trim();
            g.Size = txtSize.Text.Trim(); g.Notes = txtNotes.Text.Trim();
            g.SellPrice   = double.TryParse(txtSellPrice.Text, out double sp) ? sp : 0;
            g.BuyPrice    = float.TryParse(txtBuyPrice.Text, out float bp) ? bp : 0;
            g.SellPriceSP = double.TryParse(txtSellPriceSP.Text, out double spsp) ? spsp : 0;
            g.HalfPrice   = double.TryParse(txtHalfPrice.Text, out double hp) ? hp : 0;
            g.CustPrice   = double.TryParse(txtCustPrice.Text, out double cp) ? cp : 0;
            g.MinStock    = int.TryParse(txtMinStock.Text, out int mn) ? mn : 0;
            g.MaxStock    = int.TryParse(txtMaxStock.Text, out int mx) ? mx : 0;

            g.GroupId    = GetComboId(cboGroup);
            g.UnitId     = GetComboId(cboUnit);
            g.ImporterId = GetComboId(cboImporter);
            g.ModelId    = GetComboId(cboModel);
            g.ColorId    = GetComboId(cboColor);
            g.MarketId   = GetComboId(cboMarket);
            g.StoreId    = GetComboId(cboStore);

            if (_goodId == null) db.Goods.Add(g);
            db.SaveChanges();
            UIHelper.ShowSuccess("تم حفظ الصنف ✅");
        }

        private static int? GetComboId(ComboBox cb)
        {
            if (cb.SelectedItem is IdName item && item.Id > 0) return item.Id;
            return null;
        }

        private record IdName(int Id, string Name);
    }
}

