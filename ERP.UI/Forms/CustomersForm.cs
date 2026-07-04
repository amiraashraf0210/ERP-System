using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    public class CustomersForm : BaseListForm
    {
        private List<Customer> _all = new();
        public CustomersForm() : base("العملاء") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Customers.Include(c => c.Group).OrderBy(c => c.Code).ToList();
            BindGrid(_all);
        }

        private void BindGrid(List<Customer> list) =>
            grid.DataSource = list.Select(c => new
            {
                Id = c.Id, الكود = c.Code, الاسم = c.Name,
                المدير = c.Manager, المجموعة = c.Group?.GroupName,
                التليفون = c.Tel, الموبايل = c.Mobile, الفاكس = c.Fax, ملاحظات = c.Notes
            }).ToList();

        protected override void OnAdd()
        {
            using var f = new CustomerEditForm(null);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new CustomerEditForm(id);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا العميل؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            if (db.SellBills.Any(b => b.CustomerId == id))
            { UIHelper.ShowError("لا يمكن حذف هذا العميل لوجود فواتير مرتبطة به.\nاحذف الفواتير أولاً."); return; }
            var c = db.Customers.Find(id);
            if (c != null) { db.Customers.Remove(c); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            BindGrid(string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(c => c.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || c.Code.ToString().Contains(k)
                    || (c.Tel ?? "").Contains(k)
                    || (c.Mobile ?? "").Contains(k)).ToList());
    }

    // ─────────────────────────── CUSTOMER EDIT ───────────────────────────
    public class CustomerEditForm : Form
    {
        private readonly int? _customerId;
        private TextBox txtCode = null!, txtName = null!, txtManager = null!,
                        txtTel = null!, txtMobile = null!, txtFax = null!, txtNotes = null!;
        private ComboBox cboGroup = null!;

        public CustomerEditForm(int? customerId)
        {
            _customerId = customerId;
            BuildUI();
            LoadGroups();
            if (customerId != null)
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var c = db.Customers.Find(customerId);
                if (c != null) FillFields(c);
            }
        }

        private void BuildUI()
        {
            this.Text = _customerId == null ? "➕ عميل جديد" : "✏ تعديل عميل";
            this.Size = new Size(460, 450);
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

            var pnlForm = new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill, Padding = new Padding(20), RightToLeft = RightToLeft.Yes };

            var fields = new (string Label, Control Ctrl, Color LabelColor, Color BackColor)[]
            {
                ("الكود *",     txtCode    = new TextBox(), Color.FromArgb(0, 80, 160), Color.FromArgb(230, 240, 255)),
                ("الاسم *",     txtName    = new TextBox(), Color.FromArgb(0, 80, 160), Color.FromArgb(230, 240, 255)),
                ("المدير",      txtManager = new TextBox(), Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)),
                ("المجموعة",    cboGroup   = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, RightToLeft = RightToLeft.Yes }, Color.FromArgb(0, 120, 200), Color.FromArgb(220, 235, 255)),
                ("التليفون",    txtTel     = new TextBox(), Color.FromArgb(0, 150, 0), Color.FromArgb(220, 255, 220)),
                ("الموبايل",    txtMobile  = new TextBox(), Color.FromArgb(0, 150, 0), Color.FromArgb(220, 255, 220)),
                ("الفاكس",      txtFax     = new TextBox(), Color.FromArgb(80, 80, 80), Color.FromArgb(245, 245, 245)),
                ("ملاحظات",     txtNotes   = new TextBox(), Color.FromArgb(80, 80, 80), Color.FromArgb(245, 245, 245)),
            };

            int y = 20;
            foreach (var (label, ctrl, labelColor, backColor) in fields)
            {
                var lbl = new Label { Text = label, Location = new Point(330, y + 4), Size = new Size(90, 22), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes, ForeColor = labelColor, BackColor = Color.Transparent };
                ctrl.Location = new Point(20, y);
                ctrl.Size = new Size(290, 28);
                ctrl.Font = AppTheme.FontNormal;
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
                pnlForm.Controls.AddRange(new Control[] { lbl, ctrl });
                y += 40;
            }

            var btnSave   = UIHelper.MakeButton("💾 حفظ",   Color.FromArgb(0, 150, 0),  new Size(130, 40), new Point(170, y + 5));
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", Color.FromArgb(200, 50, 50),  new Size(120, 40), new Point(30, y + 5));
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += BtnSave_Click;
            btnCancel.DialogResult = DialogResult.Cancel;
            pnlForm.Controls.AddRange(new Control[] { btnSave, btnCancel });

            pnlGlass.Controls.Add(pnlForm);
            this.Controls.Add(pnlGlass);
            this.ClientSize = new Size(440, y + 75);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadGroups()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var groups = db.CustomerGroups.OrderBy(g => g.GroupName).ToList();
            groups.Insert(0, new CustomerGroup { Id = 0, GroupName = "-- بدون مجموعة --" });
            cboGroup.DisplayMember = "GroupName"; cboGroup.ValueMember = "Id";
            cboGroup.DataSource = groups; cboGroup.SelectedIndex = 0;
        }

        private void FillFields(Customer c)
        {
            txtCode.Text = c.Code.ToString(); txtName.Text = c.Name;
            txtManager.Text = c.Manager; txtTel.Text = c.Tel;
            txtMobile.Text = c.Mobile; txtFax.Text = c.Fax; txtNotes.Text = c.Notes;
            if (c.GroupId.HasValue && c.GroupId > 0) cboGroup.SelectedValue = c.GroupId.Value;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { UIHelper.ShowError("اسم العميل مطلوب"); this.DialogResult = DialogResult.None; return; }

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var c = _customerId == null ? new Customer() : db.Customers.Find(_customerId)!;
            c.Code = int.TryParse(txtCode.Text, out int code) ? code : 0;
            c.Name = txtName.Text.Trim(); c.Manager = txtManager.Text.Trim();
            c.Tel = txtTel.Text.Trim(); c.Mobile = txtMobile.Text.Trim();
            c.Fax = txtFax.Text.Trim(); c.Notes = txtNotes.Text.Trim();
            var gid = cboGroup.SelectedValue as int?;
            c.GroupId = gid > 0 ? gid : null;

            if (_customerId == null) db.Customers.Add(c);
            db.SaveChanges();
            UIHelper.ShowSuccess("تم الحفظ بنجاح ✅");
        }
    }
}
