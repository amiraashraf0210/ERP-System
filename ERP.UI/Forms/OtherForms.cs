using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    // ══════════════════════ ORDERS ══════════════════════
    public class OrdersForm : BaseListForm
    {
        private List<Order> _all = new();
        public OrdersForm() : base("الطلبات") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Orders.OrderByDescending(o => o.DateStart).Take(200).ToList();
            grid.DataSource = _all.Select(o => new
            {
                Id = o.Code, الرقم = o.Code, التاريخ = o.DateStart.ToString("yyyy/MM/dd"),
                العميل = o.CustName, التليفون = o.CustTel,
                الإجمالي = o.Total.ToString("N2"), المدفوع = o.Pay.ToString("N2"),
                حالة_الدفع = o.Paid ? "مدفوع ✅" : "غير مدفوع ❌",
                التوصيل = o.Delivery ? "نعم 🚚" : "لا"
            }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("طلب جديد",
                new[] { "اسم العميل", "التليفون", "العنوان", "التفاصيل", "الإجمالي", "المدفوع" },
                new[] { "", "", "", "", "0", "0" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            int nextCode = (db.Orders.Max(o => (int?)o.Code) ?? 0) + 1;
            db.Orders.Add(new Order
            {
                Code = nextCode, CustName = f.Values[0], CustTel = f.Values[1],
                CustAddress = f.Values[2], Detail = f.Values[3],
                Total = float.TryParse(f.Values[4], out float t) ? t : 0,
                Pay   = float.TryParse(f.Values[5], out float p) ? p : 0,
                DateStart = DateTime.Today, DateEnd = DateTime.Today.AddDays(3),
                Paid = false
            });
            db.SaveChanges(); LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId("Id"); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var o = db.Orders.Find(id);
            if (o == null) { UIHelper.ShowError("الطلب غير موجود"); return; }

            using var f = new OrderEditForm(o);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId("Id"); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا الطلب؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var o = db.Orders.Find(id);
            if (o != null) { db.Orders.Remove(o); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k)
        {
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(o => (o.CustName ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) || (o.CustTel ?? "").Contains(k)).ToList())
                .Select(o => new { Id = o.Code, الرقم = o.Code, العميل = o.CustName, التليفون = o.CustTel, الإجمالي = o.Total.ToString("N2") }).ToList();
        }
    }

    // ══════════════════════ ORDER EDIT FORM ══════════════════════
    internal class OrderEditForm : Form
    {
        private readonly Order _order;
        private TextBox txtName = null!, txtTel = null!, txtAddr = null!,
                        txtDetail = null!, txtTotal = null!, txtPay = null!;
        private CheckBox chkPaid = null!, chkDelivery = null!;

        public OrderEditForm(Order order)
        {
            _order = order;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = $"تعديل طلب رقم {_order.Code}";
            this.Size = new Size(440, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9,
                Padding = new Padding(16, 12, 16, 8), BackColor = Color.White
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            void AddRow(int r, string lbl, Control ctrl)
            {
                tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, r);
                tbl.Controls.Add(ctrl, 1, r);
            }

            TextBox MakeTxt(string val) => new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = val, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 5, 0, 5) };
            CheckBox MakeChk(string lbl, bool val) => new CheckBox { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Checked = val, Margin = new Padding(0, 8, 0, 0) };

            txtName    = MakeTxt(_order.CustName ?? "");
            txtTel     = MakeTxt(_order.CustTel ?? "");
            txtAddr    = MakeTxt(_order.CustAddress ?? "");
            txtDetail  = MakeTxt(_order.Detail ?? "");
            txtTotal   = MakeTxt(_order.Total.ToString("N2"));
            txtPay     = MakeTxt(_order.Pay.ToString("N2"));
            chkPaid    = MakeChk("مدفوع ✅", _order.Paid);
            chkDelivery= MakeChk("يشمل توصيل 🚚", _order.Delivery);

            AddRow(0, "اسم العميل:",  txtName);
            AddRow(1, "التليفون:",    txtTel);
            AddRow(2, "العنوان:",     txtAddr);
            AddRow(3, "التفاصيل:",   txtDetail);
            AddRow(4, "الإجمالي:",   txtTotal);
            AddRow(5, "المدفوع:",    txtPay);
            AddRow(6, "الدفع:",      chkPaid);
            AddRow(7, "التوصيل:",    chkDelivery);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.White };
            var btnSave = UIHelper.MakeButton("💾 حفظ", AppTheme.Accent, new Size(130, 38), Point.Empty);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(110, 38), Point.Empty);
            btnSave.Margin = btnCancel.Margin = new Padding(0, 4, 8, 0);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) =>
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var o = db.Orders.Find(_order.Code);
                if (o == null) return;
                o.CustName    = txtName.Text.Trim();
                o.CustTel     = txtTel.Text.Trim();
                o.CustAddress = txtAddr.Text.Trim();
                o.Detail      = txtDetail.Text.Trim();
                o.Total    = float.TryParse(txtTotal.Text, out float t) ? t : o.Total;
                o.Pay      = float.TryParse(txtPay.Text,   out float p) ? p : o.Pay;
                o.Paid     = chkPaid.Checked;
                o.Delivery = chkDelivery.Checked;
                db.SaveChanges();
            };
            btnCancel.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            tbl.Controls.Add(new Label(), 0, 8);
            tbl.Controls.Add(btnFlow, 1, 8);

            this.Controls.Add(tbl);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }
    }

    // ══════════════════════ DELIVERY ══════════════════════
    public class DeliveryForm : BaseListForm
    {
        public DeliveryForm() : base("التوصيل") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var bills = db.DeliveryBills.Include(b => b.Movements).OrderByDescending(b => b.Date).Take(100).ToList();
            grid.DataSource = bills.Select(b => new
            {
                Id = b.Id, الرقم = b.Code, التاريخ = b.Date.ToString("yyyy/MM/dd"),
                الإجمالي = b.Asked.ToString("N2"), المدفوع = b.Paid.ToString("N2"),
                عدد_الأصناف = b.Movements.Count
            }).ToList();
        }

        protected override void OnAdd() => UIHelper.ShowSuccess("استخدمي شاشة فواتير المبيعات وحدد خيار التوصيل");
        protected override void OnSearch(string k) => LoadData();
    }

    // ══════════════════════ COMPANY SETTINGS ══════════════════════
    public class CompanyForm : Form
    {
        private TextBox txtName = null!, txtTel = null!, txtFax = null!, txtAddress = null!;
        private AppData? _data;

        public CompanyForm()
        {
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            this.Text = "🏢 بيانات الشركة";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // الكارت الخارجي
            var card = new Panel
            {
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 20),
                Size = new Size(560, 320)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.FillRectangle(new SolidBrush(AppTheme.Primary), 0, 0, card.Width, 42);
                using var tf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                g.DrawString("بيانات الشركة", AppTheme.FontBold, Brushes.White,
                    new RectangleF(0, 0, card.Width - 12, 42), tf);
                using var pen = new Pen(AppTheme.Border);
                g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Location = new Point(12, 52),
                Size = new Size(536, 220),
                ColumnCount = 2,
                RowCount = 5,
                BackColor = Color.White
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // row for button

            txtName    = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 4, 6) };
            txtTel     = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 4, 6) };
            txtFax     = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 4, 6) };
            txtAddress = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 6, 4, 6) };

            void AddRow(int row, string lbl, TextBox txt)
            {
                tbl.Controls.Add(new Label
                {
                    Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold,
                    TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0, 0, 8, 0)
                }, 0, row);
                tbl.Controls.Add(txt, 1, row);
            }

            AddRow(0, "اسم الشركة *:", txtName);
            AddRow(1, "التليفون:",      txtTel);
            AddRow(2, "الفاكس:",        txtFax);
            AddRow(3, "العنوان:",       txtAddress);

            var btnSave = UIHelper.MakeButton("💾 حفظ البيانات", AppTheme.Accent, new Size(170, 38), Point.Empty);
            btnSave.Margin = new Padding(0, 6, 0, 0);
            btnSave.Click += BtnSave_Click;

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            btnPanel.Controls.Add(btnSave);
            tbl.Controls.Add(new Label(), 0, 4); // spacer
            tbl.Controls.Add(btnPanel, 1, 4);

            card.Controls.Add(tbl);

            this.Resize += (s, e) =>
            {
                card.Width = Math.Min(600, this.ClientSize.Width - 40);
                tbl.Width  = card.Width - 24;
            };

            this.Controls.Add(card);
        }

        private void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _data = db.AppData.FirstOrDefault();
            if (_data != null)
            {
                txtName.Text = _data.Name; txtTel.Text = _data.Tel;
                txtFax.Text = _data.Fax; txtAddress.Text = _data.Address;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { UIHelper.ShowError("اسم الشركة مطلوب"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var data = _data == null ? new AppData() : db.AppData.Find(_data.Id)!;
            data.Name = txtName.Text.Trim(); data.Tel = txtTel.Text.Trim();
            data.Fax = txtFax.Text.Trim(); data.Address = txtAddress.Text.Trim();
            if (_data == null) db.AppData.Add(data);
            db.SaveChanges();
            UIHelper.ShowSuccess("تم حفظ بيانات الشركة ✅");
        }
    }

    // ══════════════════════ USERS ══════════════════════
    public class UsersForm : BaseListForm
    {
        private List<User> _all = new();
        public UsersForm() : base("المستخدمين") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.Users.ToList();
            grid.DataSource = _all.Select(u => new { Id = u.Id, الاسم = u.Name, التليفون = u.Tel, الموبايل = u.Mobile }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new SimpleEditForm("مستخدم جديد", new[] { "اسم المستخدم *", "كلمة المرور *", "التليفون", "الموبايل" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(f.Values[0]) || string.IsNullOrWhiteSpace(f.Values[1])) { UIHelper.ShowError("الاسم وكلمة المرور مطلوبان"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            if (db.Users.Any(u => u.Name == f.Values[0])) { UIHelper.ShowError("اسم المستخدم موجود مسبقاً"); return; }
            db.Users.Add(new User { Name = f.Values[0], Pass = f.Values[1], Tel = f.Values[2], Mobile = f.Values[3] });
            db.SaveChanges(); LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var u = db.Users.Find(id)!;
            using var f = new SimpleEditForm("تعديل مستخدم", new[] { "اسم المستخدم *", "كلمة المرور *", "التليفون", "الموبايل" },
                new[] { u.Name, u.Pass, u.Tel ?? "", u.Mobile ?? "" });
            if (f.ShowDialog(this) != DialogResult.OK) return;
            u.Name = f.Values[0]; u.Pass = f.Values[1]; u.Tel = f.Values[2]; u.Mobile = f.Values[3];
            db.SaveChanges(); LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذا المستخدم؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var u = db.Users.Find(id);
            if (u != null) { db.Users.Remove(u); db.SaveChanges(); }
            LoadData();
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all : _all.Where(u => u.Name.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList())
                .Select(u => new { Id = u.Id, الاسم = u.Name, التليفون = u.Tel }).ToList();
    }

    // ══════════════════════ REPORTS ══════════════════════
    public class ReportCustomerStatement : Form
    {
        public ReportCustomerStatement() { BuildUI(); }
        private void BuildUI()
        {
            this.Text = "📊 كشف حساب عميل";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(700, 400);

            // Filter bar with TableLayoutPanel — no fixed pixel positions
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            pnlFilter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7, RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 8, 8, 8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label العميل
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));    // cboCustomer
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label من
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));  // dtpFrom
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label إلى
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));  // dtpTo
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // btnShow

            var cboCustomer = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 0, 8, 0) };
            var lblCust  = new Label { Text = "العميل:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var lblFrom  = new Label { Text = "من:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var dtpFrom  = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Today.Year, 1, 1), Margin = new Padding(0, 0, 8, 0) };
            var lblTo    = new Label { Text = "إلى:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var dtpTo    = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 0, 8, 0) };
            var btnShow  = UIHelper.MakeButton("📊 عرض", AppTheme.Primary, new Size(100, 36), Point.Empty);
            btnShow.Dock = DockStyle.Fill; btnShow.Margin = new Padding(0);

            tbl.Controls.Add(lblCust,  0, 0);
            tbl.Controls.Add(cboCustomer, 1, 0);
            tbl.Controls.Add(lblFrom,  2, 0);
            tbl.Controls.Add(dtpFrom,  3, 0);
            tbl.Controls.Add(lblTo,    4, 0);
            tbl.Controls.Add(dtpTo,    5, 0);
            tbl.Controls.Add(btnShow,  6, 0);
            pnlFilter.Controls.Add(tbl);

            var grid = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(grid);

            btnShow.Click += (s, e) =>
            {
                if (cboCustomer.SelectedItem == null) { UIHelper.ShowError("اختار العميل"); return; }
                int custId = (int)cboCustomer.SelectedValue!;
                var from = dtpFrom.Value.Date;
                var to   = dtpTo.Value.Date;

                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var bills = db.SellBills.Where(b => b.CustomerId == custId && b.Date >= from && b.Date <= to).OrderBy(b => b.Date).ToList();

                double totalAsked = 0, totalPaid = 0;
                grid.DataSource = bills.Select(b =>
                {
                    totalAsked += b.Asked; totalPaid += b.Paid;
                    return new { التاريخ = b.Date.ToString("yyyy/MM/dd"), رقم_الفاتورة = b.Code, مطلوب = b.Asked.ToString("N2"), مدفوع = b.Paid.ToString("N2"), متبقي = (b.Asked - b.Paid).ToString("N2"), ملاحظات = b.Notes };
                }).ToList();

                this.Text = $"كشف حساب: {((Customer)cboCustomer.SelectedItem).Name}  |  إجمالي: {totalAsked:N2}  |  مدفوع: {totalPaid:N2}  |  متبقي: {(totalAsked - totalPaid):N2}";
            };

            using var db2 = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            cboCustomer.DisplayMember = "Name"; cboCustomer.ValueMember = "Id";
            cboCustomer.DataSource = db2.Customers.OrderBy(c => c.Name).ToList();

            this.Controls.Add(grid);
            this.Controls.Add(pnlFilter);
        }
    }

    public class ReportImporterStatement : Form
    {
        public ReportImporterStatement() { BuildUI(); }
        private void BuildUI()
        {
            this.Text = "📊 كشف حساب مورد";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(700, 400);

            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            pnlFilter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7, RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 8, 8, 8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            var cbo    = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 0, 8, 0) };
            var lblImp = new Label { Text = "المورد:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var lblFrom= new Label { Text = "من:",    Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var dtpFrom= new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Today.Year, 1, 1), Margin = new Padding(0, 0, 8, 0) };
            var lblTo  = new Label { Text = "إلى:",  Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            var dtpTo  = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 0, 8, 0) };
            var btnShow= UIHelper.MakeButton("📊 عرض", AppTheme.Primary, new Size(100, 36), Point.Empty);
            btnShow.Dock = DockStyle.Fill; btnShow.Margin = new Padding(0);

            tbl.Controls.Add(lblImp,  0, 0);
            tbl.Controls.Add(cbo,     1, 0);
            tbl.Controls.Add(lblFrom, 2, 0);
            tbl.Controls.Add(dtpFrom, 3, 0);
            tbl.Controls.Add(lblTo,   4, 0);
            tbl.Controls.Add(dtpTo,   5, 0);
            tbl.Controls.Add(btnShow, 6, 0);
            pnlFilter.Controls.Add(tbl);

            var grid = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(grid);

            btnShow.Click += (s, e) =>
            {
                if (cbo.SelectedItem == null) return;
                int impId = (int)cbo.SelectedValue!;
                var from = dtpFrom.Value.Date;
                var to   = dtpTo.Value.Date;
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                grid.DataSource = db.BuyBills.Where(b => b.ImporterId == impId && b.Date >= from && b.Date <= to).OrderBy(b => b.Date)
                    .Select(b => new { التاريخ = b.Date.ToString("yyyy/MM/dd"), رقم_الفاتورة = b.Code, مطلوب = (double)b.Asked, مدفوع = (double)b.Paid, متبقي = (double)(b.Asked - b.Paid) }).ToList();
            };

            using var db2 = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            cbo.DisplayMember = "Name"; cbo.ValueMember = "Id";
            cbo.DataSource = db2.Importers.OrderBy(i => i.Name).ToList();

            this.Controls.Add(grid);
            this.Controls.Add(pnlFilter);
        }
    }

    public class ReportStock : BaseListForm
    {
        public ReportStock() : base("📊 تقرير المخزون") { }

        protected override void AddExtraButtons(FlowLayoutPanel toolbar)
        {
            // Hide Add, Edit, Delete buttons for read-only report
            btnAdd.Visible = false;
            btnEdit.Visible = false;
            btnDelete.Visible = false;
        }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var goods = db.Goods.Include(g => g.Group).Include(g => g.Unit).OrderBy(g => g.Code).ToList();
            var movements = db.Movements.GroupBy(m => m.GoodId)
                .Select(g => new { GoodId = g.Key, StockIn = g.Where(m => !m.Out).Sum(m => m.Quantity), StockOut = g.Where(m => m.Out).Sum(m => m.Quantity) }).ToList();

            grid.DataSource = goods.Select(g =>
            {
                var mv = movements.FirstOrDefault(m => m.GoodId == g.Id);
                double stock = (mv?.StockIn ?? 0) - (mv?.StockOut ?? 0);
                return new
                {
                    Id = g.Id, الكود = g.Code, الاسم = g.Name, المجموعة = g.Group?.GroupName, الوحدة = g.Unit?.UnitName,
                    الرصيد = stock, سعر_البيع = g.SellPrice.ToString("N2"), سعر_الشراء = g.BuyPrice.ToString("N2"),
                    حالة = stock <= g.MinStock ? "⚠ أقل من الحد الأدنى" : "✅ طبيعي"
                };
            }).ToList();
        }

        protected override void OnSearch(string k) => LoadData();
    }

    public class ReportSales : BaseListForm
    {
        public ReportSales() : base("📊 تقرير المبيعات") { }

        protected override void AddExtraButtons(FlowLayoutPanel toolbar)
        {
            // Hide Add, Edit, Delete buttons for read-only report
            btnAdd.Visible = false;
            btnEdit.Visible = false;
            btnDelete.Visible = false;
        }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var data = db.SellBills.Include(b => b.Customer)
                .Where(b => b.Date.Month == DateTime.Today.Month && b.Date.Year == DateTime.Today.Year)
                .OrderByDescending(b => b.Date).ToList();
            grid.DataSource = data.Select(b => new
            {
                Id = b.Id, التاريخ = b.Date.ToString("yyyy/MM/dd"), العميل = b.Customer?.Name ?? "",
                إجمالي_الفاتورة = b.Asked.ToString("N2"), المدفوع = b.Paid.ToString("N2"), المتبقي = (b.Asked - b.Paid).ToString("N2")
            }).ToList();
            this.Text = $"تقرير المبيعات - {DateTime.Today:MMMM yyyy}  |  إجمالي: {data.Sum(b => b.Asked):N2}";
        }

        protected override void OnSearch(string k) => LoadData();
    }

    public class ReportPurchases : BaseListForm
    {
        public ReportPurchases() : base("📊 تقرير المشتريات") { }

        protected override void AddExtraButtons(FlowLayoutPanel toolbar)
        {
            btnAdd.Visible = false;
            btnEdit.Visible = false;
            btnDelete.Visible = false;
        }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var data = db.BuyBills.Include(b => b.Importer)
                .Where(b => b.Date.Month == DateTime.Today.Month && b.Date.Year == DateTime.Today.Year)
                .OrderByDescending(b => b.Date).ToList();
            grid.DataSource = data.Select(b => new
            {
                Id = b.Id, التاريخ = b.Date.ToString("yyyy/MM/dd"), المورد = b.Importer?.Name ?? "",
                الإجمالي = b.Asked.ToString("N2"), المدفوع = b.Paid.ToString("N2")
            }).ToList();
        }

        protected override void OnSearch(string k) => LoadData();
    }

    public class ReportProfits : Form
    {
        private NumericUpDown numYear = null!;
        private ComboBox cboMonth = null!;
        private DataGridView grid = null!;
        private Label lblSales = null!, lblCost = null!, lblExpenses = null!, lblNetProfit = null!;

        public ReportProfits()
        {
            this.Text = "📊 تقرير الأرباح والخسائر الدوري";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Top Panel ──
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(12, 10, 12, 10) };
            pnlFilter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var lblY = new Label { Text = "السنة:", AutoSize = true, Font = AppTheme.FontBold, Margin = new Padding(0, 6, 4, 0) };
            numYear = new NumericUpDown { Minimum = 2020, Maximum = 2100, Value = DateTime.Today.Year, Font = AppTheme.FontNormal, Width = 80, Margin = new Padding(0, 2, 16, 0) };

            var lblM = new Label { Text = "الشهر:", AutoSize = true, Font = AppTheme.FontBold, Margin = new Padding(0, 6, 4, 0) };
            cboMonth = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontNormal, Width = 90, Margin = new Padding(0, 2, 16, 0) };
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(i.ToString());
            cboMonth.SelectedIndex = DateTime.Today.Month - 1;

            var btnLoad = UIHelper.MakeButton("🔍 عرض التقرير", AppTheme.Primary, new Size(130, 34), Point.Empty);
            btnLoad.Click += (s, e) => LoadData();

            var flowFilter = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            flowFilter.Controls.AddRange(new Control[] { btnLoad, cboMonth, lblM, numYear, lblY });
            pnlFilter.Controls.Add(flowFilter);

            // ── Cards Panel (Summary) ──
            var pnlSummary = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 110, ColumnCount = 4, RowCount = 1,
                Padding = new Padding(10, 8, 10, 8), BackColor = Color.FromArgb(241, 245, 249)
            };
            pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            Panel MakeCard(string title, Color accentColor, string icon, out Label lblVal)
            {
                var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5, 0, 5, 0) };
                card.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    using var path = UIHelper.RoundedRect(rect, 8);
                    using var bg = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.White, Color.FromArgb(248, 250, 252), 90f);
                    g.FillPath(bg, path);
                    g.FillRectangle(new SolidBrush(accentColor), new Rectangle(0, 0, card.Width, 4));
                    g.FillRectangle(new SolidBrush(accentColor), new Rectangle(0, 0, card.Width, 2));
                    using var border = new Pen(Color.FromArgb(20, accentColor.R, accentColor.G, accentColor.B), 1.5f);
                    g.DrawPath(border, path);
                };

                var lblIcon = new Label
                {
                    Text = icon, Dock = DockStyle.Right, Width = 40,
                    Font = new Font("Segoe UI Emoji", 18), ForeColor = Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B),
                    TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(0, 8, 4, 0)
                };
                var pnlText = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(8, 6, 4, 4) };
                var lblTitle = new Label
                {
                    Text = title, Dock = DockStyle.Top, Height = 22,
                    Font = new Font("Segoe UI", 8.5f), ForeColor = AppTheme.TextGray,
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblVal = new Label
                {
                    Text = "—", Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = accentColor,
                    TextAlign = ContentAlignment.MiddleRight
                };
                pnlText.Controls.Add(lblVal);
                pnlText.Controls.Add(lblTitle);
                card.Controls.Add(pnlText);
                card.Controls.Add(lblIcon);
                return card;
            }

            pnlSummary.Controls.Add(MakeCard("إجمالي المبيعات",          AppTheme.Accent,              "💰", out lblSales),     3, 0);
            pnlSummary.Controls.Add(MakeCard("تكلفة الخامات المستهلكة",  AppTheme.Danger,              "📦", out lblCost),      2, 0);
            pnlSummary.Controls.Add(MakeCard("المصروفات",                 AppTheme.Warning,             "💸", out lblExpenses),  1, 0);
            pnlSummary.Controls.Add(MakeCard("صافي الأرباح",             Color.FromArgb(39, 174, 96),  "📈", out lblNetProfit), 0, 0);

            // ── Grid Setup ──
            grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Good", HeaderText = "الصنف", Width = 180, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Beg", HeaderText = "أول الشهر", Width = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pur", HeaderText = "المشتريات", Width = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Act", HeaderText = "الفعلي", Width = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cons", HeaderText = "الاستهلاك", Width = 95 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "التكلفة", Width = 85 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ConsCost", HeaderText = "تكلفة الاستهلاك", Width = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sales", HeaderText = "المبيعات", Width = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GrossProfit", HeaderText = "مجمل الربح", Width = 110 });

            this.Controls.Add(UIHelper.WrapGrid(grid));
            this.Controls.Add(pnlSummary);
            this.Controls.Add(pnlFilter);
        }

        private void LoadData()
        {
            int year = (int)numYear.Value;
            int month = cboMonth.SelectedIndex + 1;

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddSeconds(-1);

            // 1. Total Sales in selected month/year
            double totalSales = db.SellBills
                .Where(b => b.Date >= startDate && b.Date <= endDate)
                .Sum(b => (double?)b.Asked) ?? 0;

            // 2. Total Expenses in selected month/year
            double totalExpenses = db.Expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .Sum(e => (double?)e.Value) ?? 0;

            // 3. Load Monthly Inventory records for this month
            var inventoryList = db.MonthlyInventories.Include(mi => mi.Good)
                .Where(mi => mi.Year == year && mi.Month == month)
                .ToList();

            double totalCostOfGoods = 0;
            grid.Rows.Clear();

            // Load all goods to display breakdown
            var goods = db.Goods.OrderBy(g => g.Name).ToList();

            foreach (var good in goods)
            {
                var inv = inventoryList.FirstOrDefault(mi => mi.GoodId == good.Id);
                double beg = 0;
                double pur = 0;
                double act = 0;
                double cons = 0;
                double costPrice = good.BuyPrice;

                if (inv != null)
                {
                    beg = inv.BeginningStock;
                    pur = inv.Purchases;
                    act = inv.ActualStock;
                    cons = inv.Consumption;
                }
                else
                {
                    // Dynamic calculation
                    int prevMonth = month == 1 ? 12 : month - 1;
                    int prevYear = month == 1 ? year - 1 : year;
                    var prevInv = db.MonthlyInventories
                        .FirstOrDefault(mi => mi.Year == prevYear && mi.Month == prevMonth && mi.GoodId == good.Id);

                    if (prevInv != null) beg = prevInv.ActualStock;
                    else
                    {
                        var prevIn = db.Movements.Where(m => m.GoodId == good.Id && !m.Out && m.Date < startDate).Sum(m => (double?)m.Quantity) ?? 0;
                        var prevOut = db.Movements.Where(m => m.GoodId == good.Id && m.Out && m.Date < startDate).Sum(m => (double?)m.Quantity) ?? 0;
                        beg = Math.Max(0, prevIn - prevOut);
                    }

                    pur = db.Movements.Where(m => m.GoodId == good.Id && !m.Out && m.Date >= startDate && m.Date <= endDate && !(m.Notes ?? "").StartsWith("تسوية جرد")).Sum(m => (double?)m.Quantity) ?? 0;
                    double salesQty = db.Movements.Where(m => m.GoodId == good.Id && m.Out && m.Date >= startDate && m.Date <= endDate && !(m.Notes ?? "").StartsWith("تسوية جرد")).Sum(m => (double?)m.Quantity) ?? 0;
                    double systemStock = beg + pur - salesQty;
                    act = systemStock;
                    cons = beg + pur - act;
                }

                double consumptionCost = cons * costPrice;
                totalCostOfGoods += consumptionCost;

                // Find sales revenue for this good in this month from CustomerInstallments
                double goodSalesRevenue = db.CustomerInstallments
                    .Include(ci => ci.Bill)
                    .Where(ci => ci.GoodId == good.Id && ci.Bill != null && ci.Bill.Date >= startDate && ci.Bill.Date <= endDate)
                    .Sum(ci => (double?)ci.Total) ?? 0;

                double grossProfit = goodSalesRevenue - consumptionCost;

                int rIdx = grid.Rows.Add();
                var row = grid.Rows[rIdx];
                row.Cells["Good"].Value = good.Name;
                row.Cells["Beg"].Value = beg.ToString("N2");
                row.Cells["Pur"].Value = pur.ToString("N2");
                row.Cells["Act"].Value = act.ToString("N2");
                row.Cells["Cons"].Value = cons.ToString("N2");
                row.Cells["Price"].Value = costPrice.ToString("N2");
                row.Cells["ConsCost"].Value = consumptionCost.ToString("N2");
                row.Cells["Sales"].Value = goodSalesRevenue.ToString("N2");
                row.Cells["GrossProfit"].Value = grossProfit.ToString("N2");

                row.Cells["GrossProfit"].Style.ForeColor = grossProfit >= 0 ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
                row.Cells["GrossProfit"].Style.Font = AppTheme.FontBold;
            }

            double netProfit = totalSales - totalCostOfGoods - totalExpenses;

            lblSales.Text = totalSales.ToString("N2");
            lblCost.Text = totalCostOfGoods.ToString("N2");
            lblExpenses.Text = totalExpenses.ToString("N2");
            lblNetProfit.Text = netProfit.ToString("N2");

            lblNetProfit.ForeColor = netProfit >= 0 ? Color.FromArgb(39, 174, 96) : Color.FromArgb(231, 76, 60);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }
    }
}
