using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class ImportersForm : BaseListForm
    {
        private List<Importer> _all = new();
        public ImportersForm() : base("الموردين") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Importers.Include(i => i.Group).OrderBy(i => i.Code).ToList();
            BindGrid(_all);
        }

        private void BindGrid(List<Importer> list) =>
            grid.DataSource = list.Select(i => new
            {
                Id = i.Id, الكود = i.Code, الاسم = i.Name,
                المدير = i.Manager, المجموعة = i.Group?.GroupName,
                التليفون = i.Tel, الموبايل = i.Mobile, ملاحظات = i.Notes
            }).ToList();

        protected override void OnAdd()
        {
            using var f = new ImporterEditForm(null);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new ImporterEditForm(id);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا المورد؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            if (db.BuyBills.Any(b => b.ImporterId == id))
            { UIHelper.ShowError("لا يمكن حذف هذا المورد لوجود فواتير مشتريات مرتبطة به.\nاحذف الفواتير أولاً."); return; }
            var i = db.Importers.Find(id);
            if (i != null) { db.Importers.Remove(i); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            BindGrid(string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(i => i.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || i.Code.ToString().Contains(k)
                    || (i.Mobile ?? "").Contains(k)).ToList());
    }

    public class ImporterEditForm : Form
    {
        private readonly int? _importerId;
        private TextBox txtCode = null!, txtName = null!, txtManager = null!,
                        txtTel = null!, txtMobile = null!, txtFax = null!, txtNotes = null!;
        private ComboBox cboGroup = null!;

        public ImporterEditForm(int? importerId)
        {
            _importerId = importerId;
            BuildUI();
            LoadGroups();
            if (importerId != null)
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var i = db.Importers.Find(importerId);
                if (i != null) FillFields(i);
            }
        }

        private void BuildUI()
        {
            this.Text = _importerId == null ? "➕ مورد جديد" : "✏ تعديل مورد";
            this.Size = new Size(460, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var pnlForm = new Panel { BackColor = Color.White, Dock = DockStyle.Fill, Padding = new Padding(20) };

            var fields = new (string Label, Control Ctrl)[]
            {
                ("الكود",     txtCode    = new TextBox()),
                ("الاسم *",   txtName    = new TextBox()),
                ("المدير",    txtManager = new TextBox()),
                ("المجموعة",  cboGroup   = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }),
                ("التليفون",  txtTel     = new TextBox()),
                ("الموبايل",  txtMobile  = new TextBox()),
                ("الفاكس",    txtFax     = new TextBox()),
                ("ملاحظات",   txtNotes   = new TextBox()),
            };

            int y = 20;
            foreach (var (label, ctrl) in fields)
            {
                var lbl = new Label { Text = label, Location = new Point(330, y + 4), Size = new Size(90, 22), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
                ctrl.Location = new Point(20, y); ctrl.Size = new Size(290, 28); ctrl.Font = AppTheme.FontNormal;
                if (ctrl is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
                pnlForm.Controls.AddRange(new Control[] { lbl, ctrl });
                y += 40;
            }

            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(130, 40), new Point(170, y + 5));
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(120, 40), new Point(30, y + 5));
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += BtnSave_Click;
            btnCancel.DialogResult = DialogResult.Cancel;
            pnlForm.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.Controls.Add(pnlForm);
            this.ClientSize = new Size(440, y + 75);
            this.AcceptButton = btnSave; this.CancelButton = btnCancel;
        }

        private void LoadGroups()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var groups = db.ImporterGroups.OrderBy(g => g.GroupName).ToList();
            groups.Insert(0, new ImporterGroup { Id = 0, GroupName = "-- بدون مجموعة --" });
            cboGroup.DisplayMember = "GroupName"; cboGroup.ValueMember = "Id";
            cboGroup.DataSource = groups; cboGroup.SelectedIndex = 0;
        }

        private void FillFields(Importer i)
        {
            txtCode.Text = i.Code.ToString(); txtName.Text = i.Name;
            txtManager.Text = i.Manager; txtTel.Text = i.Tel;
            txtMobile.Text = i.Mobile; txtFax.Text = i.Fax; txtNotes.Text = i.Notes;
            if (i.GroupId.HasValue && i.GroupId > 0) cboGroup.SelectedValue = i.GroupId.Value;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { UIHelper.ShowError("اسم المورد مطلوب"); this.DialogResult = DialogResult.None; return; }

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var i = _importerId == null ? new Importer() : db.Importers.Find(_importerId)!;
            i.Code = int.TryParse(txtCode.Text, out int c) ? c : 0;
            i.Name = txtName.Text.Trim(); i.Manager = txtManager.Text.Trim();
            i.Tel = txtTel.Text.Trim(); i.Mobile = txtMobile.Text.Trim();
            i.Fax = txtFax.Text.Trim(); i.Notes = txtNotes.Text.Trim();
            var gid = cboGroup.SelectedValue as int?;
            i.GroupId = gid > 0 ? gid : null;

            if (_importerId == null) db.Importers.Add(i);
            db.SaveChanges();
            UIHelper.ShowSuccess("تم الحفظ ✅");
        }
    }
}
