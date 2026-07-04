using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    // ═══════════════════════ GENERIC LOOKUP EDIT ═══════════════════════
    internal class LookupEditForm : Form
    {
        public string? EnteredName { get; private set; }
        public int EnteredCode { get; private set; }

        public LookupEditForm(string title, string? currentName = null, int currentCode = 0)
        {
            this.Text = title; this.Size = new Size(380, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.RightToLeft = RightToLeft.Yes; this.RightToLeftLayout = true;

            var lblCode = new Label { Text = "الكود:", Location = new Point(20, 20), AutoSize = true, Font = AppTheme.FontBold };
            var txtCode = new TextBox { Location = new Point(80, 18), Size = new Size(260, 28), Font = AppTheme.FontNormal, Text = currentCode.ToString() };
            var lblName = new Label { Text = "الاسم:", Location = new Point(20, 58), AutoSize = true, Font = AppTheme.FontBold };
            var txtName = new TextBox { Location = new Point(80, 56), Size = new Size(260, 28), Font = AppTheme.FontNormal, Text = currentName ?? "" };

            var btnSave = UIHelper.MakeButton("حفظ", AppTheme.Accent, new Size(120, 36), new Point(80, 100));
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) { UIHelper.ShowError("الاسم مطلوب"); this.DialogResult = DialogResult.None; return; }
                EnteredName = txtName.Text.Trim();
                EnteredCode = int.TryParse(txtCode.Text, out int c) ? c : 0;
            };
            var btnCancel = UIHelper.MakeButton("إلغاء", AppTheme.Danger, new Size(100, 36), new Point(210, 100));
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { lblCode, txtCode, lblName, txtName, btnSave, btnCancel });
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }
    }

    // ═══════════════════════ CUSTOMER GROUPS ═══════════════════════
    public class CustomerGroupsForm : BaseListForm
    {
        private List<CustomerGroup> _all = new();
        public CustomerGroupsForm() : base("مجموعات العملاء") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.CustomerGroups.OrderBy(g => g.Code).ToList();
            BindGrid(_all);
        }
        private void BindGrid(List<CustomerGroup> list) =>
            grid.DataSource = list.Select(g => new { Id = g.Id, الكود = g.Code, المجموعة = g.GroupName, ملاحظات = g.Notes }).ToList();
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة مجموعة");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.CustomerGroups.Add(new CustomerGroup { Code = f.EnteredCode, GroupName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.CustomerGroups.Find(id)!;
            using var f = new LookupEditForm("تعديل مجموعة", g.GroupName, g.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            g.Code = f.EnteredCode; g.GroupName = f.EnteredName!;
            db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه المجموعة؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.CustomerGroups.Find(id); if (g != null) { db.CustomerGroups.Remove(g); db.SaveChanges(); }
            LoadData();
        }
        protected override void OnSearch(string k) => BindGrid(string.IsNullOrWhiteSpace(k) ? _all : _all.Where(g => g.GroupName.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    // ═══════════════════════ IMPORTER GROUPS ═══════════════════════
    public class ImporterGroupsForm : BaseListForm
    {
        private List<ImporterGroup> _all = new();
        public ImporterGroupsForm() : base("مجموعات الموردين") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.ImporterGroups.OrderBy(g => g.Code).ToList();
            grid.DataSource = _all.Select(g => new { Id = g.Id, الكود = g.Code, المجموعة = g.GroupName }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة مجموعة مورد");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ImporterGroups.Add(new ImporterGroup { Code = f.EnteredCode, GroupName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.ImporterGroups.Find(id)!;
            using var f = new LookupEditForm("تعديل", g.GroupName, g.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            g.Code = f.EnteredCode; g.GroupName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.ImporterGroups.Find(id); if (g != null) { db.ImporterGroups.Remove(g); db.SaveChanges(); }
            LoadData();
        }
        protected override void OnSearch(string k) { LoadData(); }
    }

    // ═══════════════════════ GOOD GROUPS ═══════════════════════
    public class GoodGroupsForm : BaseListForm
    {
        private List<GoodGroup> _all = new();
        public GoodGroupsForm() : base("مجموعات الأصناف") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.GoodGroups.OrderBy(g => g.Code).ToList();
            grid.DataSource = _all.Select(g => new { Id = g.Id, الكود = g.Code, المجموعة = g.GroupName }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة مجموعة صنف");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GoodGroups.Add(new GoodGroup { Code = f.EnteredCode, GroupName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.GoodGroups.Find(id)!;
            using var f = new LookupEditForm("تعديل", g.GroupName, g.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            g.Code = f.EnteredCode; g.GroupName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var g = db.GoodGroups.Find(id); if (g != null) { db.GoodGroups.Remove(g); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) { LoadData(); }
    }

    // ═══════════════════════ UNITS ═══════════════════════
    public class UnitsForm : BaseListForm
    {
        private List<Unit> _all = new();
        public UnitsForm() : base("الوحدات") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Units.OrderBy(u => u.Code).ToList();
            grid.DataSource = _all.Select(u => new { Id = u.Id, الكود = u.Code, الوحدة = u.UnitName, ملاحظات = u.Notes }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة وحدة");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Units.Add(new Unit { Code = f.EnteredCode, UnitName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var u = db.Units.Find(id)!;
            using var f = new LookupEditForm("تعديل وحدة", u.UnitName, u.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            u.Code = f.EnteredCode; u.UnitName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه الوحدة؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var u = db.Units.Find(id); if (u != null) { db.Units.Remove(u); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all : _all.Where(u => u.UnitName.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(u => new { Id = u.Id, الكود = u.Code, الوحدة = u.UnitName }).ToList();
    }

    // ═══════════════════════ MODELS ═══════════════════════
    public class ModelsForm : BaseListForm
    {
        private List<GoodModel> _all = new();
        public ModelsForm() : base("الموديلات") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.GoodModels.OrderBy(m => m.Code).ToList();
            grid.DataSource = _all.Select(m => new { Id = m.Id, الكود = m.Code, الموديل = m.ModelName }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة موديل");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GoodModels.Add(new GoodModel { Code = f.EnteredCode, ModelName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var m = db.GoodModels.Find(id)!;
            using var f = new LookupEditForm("تعديل موديل", m.ModelName, m.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            m.Code = f.EnteredCode; m.ModelName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var m = db.GoodModels.Find(id); if (m != null) { db.GoodModels.Remove(m); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) { LoadData(); }
    }

    // ═══════════════════════ COLORS ═══════════════════════
    public class ColorsForm : BaseListForm
    {
        private List<GoodColor> _all = new();
        public ColorsForm() : base("الألوان") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Colors.OrderBy(c => c.Code).ToList();
            grid.DataSource = _all.Select(c => new { Id = c.Id, الكود = c.Code, اللون = c.ColorName }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة لون");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Colors.Add(new GoodColor { Code = f.EnteredCode, ColorName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var c = db.Colors.Find(id)!;
            using var f = new LookupEditForm("تعديل لون", c.ColorName, c.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            c.Code = f.EnteredCode; c.ColorName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var c = db.Colors.Find(id); if (c != null) { db.Colors.Remove(c); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) { LoadData(); }
    }

    // ═══════════════════════ MARKETS ═══════════════════════
    public class MarketsForm : BaseListForm
    {
        private List<Market> _all = new();
        public MarketsForm() : base("الأسواق") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Markets.OrderBy(m => m.Code).ToList();
            grid.DataSource = _all.Select(m => new { Id = m.Id, الكود = m.Code, السوق = m.MarketName }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة سوق");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Markets.Add(new Market { Code = f.EnteredCode, MarketName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var m = db.Markets.Find(id)!;
            using var f = new LookupEditForm("تعديل سوق", m.MarketName, m.Code);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            m.Code = f.EnteredCode; m.MarketName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var m = db.Markets.Find(id); if (m != null) { db.Markets.Remove(m); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) { LoadData(); }
    }

    // ═══════════════════════ CARS ═══════════════════════
    public class CarsForm : BaseListForm
    {
        private List<Car> _all = new();
        public CarsForm() : base("السيارات") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Cars.OrderBy(c => c.Code).ToList();
            grid.DataSource = _all.Select(c => new { Id = c.Id, الكود = c.Code, الاسم = c.Name, التليفون = c.TelMobile, العنوان = c.Address }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("إضافة سيارة / مندوب توصيل",
                new[] { "الكود", "الاسم", "التليفون", "العنوان" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Cars.Add(new Car { Code = int.TryParse(f.Values[0], out int c) ? c : 0, Name = f.Values[1], TelMobile = f.Values[2], Address = f.Values[3] });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var item = db.Cars.Find(id)!;
            using var f = new SimpleEditForm("تعديل", new[] { "الكود", "الاسم", "التليفون", "العنوان" },
                new[] { item.Code.ToString(), item.Name, item.TelMobile ?? "", item.Address ?? "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            item.Code = int.TryParse(f.Values[0], out int c) ? c : 0; item.Name = f.Values[1]; item.TelMobile = f.Values[2]; item.Address = f.Values[3];
            db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var item = db.Cars.Find(id); if (item != null) { db.Cars.Remove(item); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all : _all.Where(c => c.Name.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(c => new { Id = c.Id, الكود = c.Code, الاسم = c.Name, التليفون = c.TelMobile }).ToList();
    }

    // ═══════════════════════ TRADERS / MANDUB ═══════════════════════
    public class TradersForm : BaseListForm
    {
        private List<Trader> _all = new();
        public TradersForm() : base("المناديب") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Traders.OrderBy(t => t.Code).ToList();
            grid.DataSource = _all.Select(t => new { Id = t.Id, الكود = t.Code, الاسم = t.Name, التليفون = t.Tel, الموبايل = t.Mobile, البريد = t.Email }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("إضافة مندوب", new[] { "الكود", "الاسم", "التليفون", "الموبايل", "البريد الإلكتروني", "العنوان" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Traders.Add(new Trader { Code = int.TryParse(f.Values[0], out int c) ? c : 0, Name = f.Values[1], Tel = f.Values[2], Mobile = f.Values[3], Email = f.Values[4], Address = f.Values[5] });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var t = db.Traders.Find(id)!;
            using var f = new SimpleEditForm("تعديل مندوب", new[] { "الكود", "الاسم", "التليفون", "الموبايل", "البريد الإلكتروني", "العنوان" },
                new[] { t.Code.ToString(), t.Name, t.Tel ?? "", t.Mobile ?? "", t.Email ?? "", t.Address ?? "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            t.Code = int.TryParse(f.Values[0], out int c) ? c : 0; t.Name = f.Values[1]; t.Tel = f.Values[2]; t.Mobile = f.Values[3]; t.Email = f.Values[4]; t.Address = f.Values[5];
            db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var t = db.Traders.Find(id); if (t != null) { db.Traders.Remove(t); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all : _all.Where(t => t.Name.Contains(k, StringComparison.OrdinalIgnoreCase) || (t.Mobile ?? "").Contains(k)).ToList())
                .Select(t => new { Id = t.Id, الكود = t.Code, الاسم = t.Name, الموبايل = t.Mobile }).ToList();
    }

    // ═══════════════════════ PLACES ═══════════════════════
    public class PlacesForm : BaseListForm
    {
        private List<Place> _all = new();
        public PlacesForm() : base("الأماكن") { }
        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Places.ToList();
            grid.DataSource = _all.Select(p => new { Id = p.Id, المكان = p.PlaceName, ملاحظات = p.Notes }).ToList();
        }
        protected override void OnAdd()
        {
            using var f = new LookupEditForm("إضافة مكان");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Places.Add(new Place { PlaceName = f.EnteredName! });
            db.SaveChanges(); LoadData();
        }
        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = db.Places.Find(id)!;
            using var f = new LookupEditForm("تعديل", p.PlaceName);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            p.PlaceName = f.EnteredName!; db.SaveChanges(); LoadData();
        }
        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var p = db.Places.Find(id); if (p != null) { db.Places.Remove(p); db.SaveChanges(); } LoadData();
        }
        protected override void OnSearch(string k) { LoadData(); }
    }

    // ═══════════════════════ SIMPLE MULTI-FIELD EDIT ═══════════════════════
    internal class SimpleEditForm : Form
    {
        private readonly TextBox[] _inputs;
        public string[] Values => _inputs.Select(t => t.Text.Trim()).ToArray();

        public SimpleEditForm(string title, string[] fields, string[]? defaults = null)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            _inputs = new TextBox[fields.Length];

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = fields.Length + 1,
                Padding = new Padding(14, 12, 14, 10), BackColor = Color.White
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < fields.Length; i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            for (int i = 0; i < fields.Length; i++)
            {
                tbl.Controls.Add(new Label
                {
                    Text = fields[i] + ":", Dock = DockStyle.Fill, Font = AppTheme.FontBold,
                    TextAlign = ContentAlignment.MiddleRight
                }, 0, i);
                _inputs[i] = new TextBox
                {
                    Dock = DockStyle.Fill, Font = AppTheme.FontNormal,
                    Text = defaults?[i] ?? "", BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(0, 5, 0, 5)
                };
                tbl.Controls.Add(_inputs[i], 1, i);
            }

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(110, 36), Point.Empty); btnSave.Margin   = new Padding(0, 0, 0, 0);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء",  AppTheme.Danger, new Size(100, 36), Point.Empty); btnCancel.Margin = new Padding(6, 0, 0, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) => { if (string.IsNullOrWhiteSpace(_inputs[0].Text)) { UIHelper.ShowError("الحقل الأول مطلوب"); DialogResult = DialogResult.None; } };
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(new Label(), 0, fields.Length);
            tbl.Controls.Add(btnFlow, 1, fields.Length);

            Controls.Add(tbl);
            ClientSize = new Size(420, fields.Length * 40 + 80);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }
    }
}
