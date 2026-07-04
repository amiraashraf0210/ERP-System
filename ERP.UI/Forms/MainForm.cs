﻿using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    public class MainForm : Form
    {
        private readonly User _currentUser;
        private Panel pnlContent = null!;
        private Label lblPageTitle = null!;
        private Panel? _activeBtn = null;
        private ComboBox cboFiscalYear = null!;
        private static int? _currentFiscalYearId = null;

        public static int? CurrentFiscalYearId
        {
            get => _currentFiscalYearId;
            set => _currentFiscalYearId = value;
        }

        public MainForm(User user)
        {
            _currentUser = user;
            this.RightToLeft = RightToLeft.No;
            this.RightToLeftLayout = false;
            Build();
            this.Load += (s, e) => { ScrollSidebarToTop(); LoadFiscalYears(); Task.Run(AutoDailyBackup); };
            ShowDashboard();
        }

        private void ScrollSidebarToTop()
        {
            foreach (Control c in this.Controls)
                if (c is Panel sb && sb.Dock == DockStyle.Right)
                    foreach (Control inner in sb.Controls)
                        if (inner is FlowLayoutPanel m && m.Controls.Count > 0)
                            m.ScrollControlIntoView(m.Controls[0]);
        }
        private void LoadFiscalYears()
        {
            try
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var years = db.FiscalYears.OrderByDescending(f => f.Year).ToList();
                if (!years.Any()) { var fy = new FiscalYear { Year = DateTime.Today.Year, StartDate = new DateTime(DateTime.Today.Year,1,1), EndDate = new DateTime(DateTime.Today.Year,12,31), IsClosed = false }; db.FiscalYears.Add(fy); db.SaveChanges(); years = db.FiscalYears.OrderByDescending(f => f.Year).ToList(); }
                cboFiscalYear.DisplayMember = "Year"; cboFiscalYear.ValueMember = "Id"; cboFiscalYear.DataSource = years;
                var cur = years.FirstOrDefault(f => f.Year == DateTime.Today.Year && !f.IsClosed) ?? years.FirstOrDefault(f => !f.IsClosed) ?? years.First();
                cboFiscalYear.SelectedItem = cur; _currentFiscalYearId = cur.Id;
            }
            catch (Exception ex) { UIHelper.ShowError($"Error: {ex.Message}"); }
        }

        private static void AutoDailyBackup()
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ERP_Backups");
                Directory.CreateDirectory(folder);
                string prefix = $"ERP_Backup_{DateTime.Today:yyyy-MM-dd}";
                if (Directory.GetFiles(folder, $"{prefix}*.bak").Length > 0) return;
                string? cs = ConnectionManager.Load(); if (string.IsNullOrEmpty(cs)) return;
                var bl = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs);
                string dbName = bl.InitialCatalog; if (string.IsNullOrEmpty(dbName)) return;
                bl.InitialCatalog = "master";
                string file = Path.Combine(folder, $"{prefix}_auto.bak");
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(bl.ConnectionString); conn.Open();
                using var cmd = conn.CreateCommand(); cmd.CommandTimeout = 120;
                cmd.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = N'{file}' WITH FORMAT, STATS = 10"; cmd.ExecuteNonQuery();
                foreach (var f in Directory.GetFiles(folder, "*.bak")) if (File.GetCreationTime(f) < DateTime.Now.AddDays(-30)) try { File.Delete(f); } catch { }
            }
            catch { }
        }

        private void Build()
        {
            this.Text = "نظام ERP"; this.WindowState = FormWindowState.Maximized; this.MinimumSize = new Size(1000,600); this.BackColor = AppTheme.Light;
            var sidebar = new Panel { Dock = DockStyle.Right, Width = AppTheme.SidebarWidth, BackColor = AppTheme.SidebarBg };
            FillSidebar(sidebar);
            var topBar = new Panel { Dock = DockStyle.Top, Height = AppTheme.TopBarHeight, BackColor = AppTheme.Surface };
            topBar.Paint += (s,e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, topBar.Height-1, topBar.Width, topBar.Height-1);
            lblPageTitle = new Label { Text = "Dashboard", Font = AppTheme.FontMedium, ForeColor = AppTheme.TextDark, Dock = DockStyle.Right, Width = 350, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0,0,18,0) };
            var pnlFy = new Panel { Dock = DockStyle.Right, Width = 200, BackColor = Color.Transparent, Padding = new Padding(0,0,12,0) };
            var lblFy = new Label { Text = "السنة المالية:", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextGray, Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleRight };
            cboFiscalYear = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Height = 28 };
            cboFiscalYear.SelectedIndexChanged += (s,e) => { if (cboFiscalYear.SelectedItem is FiscalYear fy) { _currentFiscalYearId = fy.Id; UIHelper.ShowSuccess($"السنة المالية: {fy.Year}"); } };
            pnlFy.Controls.Add(cboFiscalYear); pnlFy.Controls.Add(lblFy);
            var lblUser = new Label { Text = $"  {_currentUser.Name}  {DateTime.Now:dd/MM/yyyy}", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextGray, Dock = DockStyle.Left, Width = 280, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(18,0,0,0) };
            topBar.Controls.Add(lblPageTitle); topBar.Controls.Add(pnlFy); topBar.Controls.Add(lblUser);
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Light, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(pnlContent); this.Controls.Add(topBar); this.Controls.Add(sidebar);
        }
        private void FillSidebar(Panel sidebar)
        {
            var logo = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = AppTheme.SidebarLogo };
            logo.Paint += (s,e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using var ab = new SolidBrush(AppTheme.Primary); g.FillRectangle(ab, 0, logo.Height-3, logo.Width, 3);
                var cr = new RectangleF(logo.Width/2f-16, 8, 32, 32);
                using var cb = new SolidBrush(Color.FromArgb(50, AppTheme.Primary)); g.FillEllipse(cb, cr);
                using var tf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("*", new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(AppTheme.Primary), cr, tf);
                g.DrawString("نظام ERP", new Font("Segoe UI", 9, FontStyle.Bold), new SolidBrush(AppTheme.SidebarText), new RectangleF(0,42,logo.Width,22), tf);
            };
            var logout = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.FromArgb(50,35,25), Cursor = Cursors.Hand };
            logout.Paint += (s,e) => { using var tf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; e.Graphics.DrawString("تسجيل الخروج", AppTheme.FontMenu, new SolidBrush(Color.FromArgb(180,150,120)), new RectangleF(0,0,sidebar.Width,46), tf); };
            logout.Click += (s,e) => { if (UIHelper.Confirm("تسجيل الخروج؟")) this.Close(); };
            logout.MouseEnter += (s,e) => { logout.BackColor = Color.FromArgb(80,50,30); logout.Invalidate(); };
            logout.MouseLeave += (s,e) => { logout.BackColor = Color.FromArgb(50,35,25); logout.Invalidate(); };
            var menu = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.SidebarBg, AutoScroll = true, Padding = new Padding(0,4,0,4) };
            var items = new List<Control>();

            void Sec(string t) => items.Add(new Label { Text = t, Dock = DockStyle.Top, Height = 24, Font = AppTheme.FontSection, ForeColor = AppTheme.SidebarMuted, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0,0,14,0), BackColor = Color.Transparent });

            void Btn(string label, Action act) {
                var p = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = AppTheme.SidebarBg, Cursor = Cursors.Hand };
                p.Paint += (s,e) => {
                    bool active = _activeBtn == p; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                    if (active) { using var br = new SolidBrush(Color.FromArgb(55,AppTheme.Primary)); g.FillRectangle(br,0,0,p.Width,p.Height); using var ar = new SolidBrush(AppTheme.Primary); g.FillRectangle(ar,p.Width-3,6,3,p.Height-12); }
                    var fg = active ? AppTheme.Primary : AppTheme.SidebarText;
                    using var tf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None, FormatFlags = StringFormatFlags.NoClip };
                    g.DrawString(label, AppTheme.FontMenu, new SolidBrush(fg), new RectangleF(4,0,p.Width-16,p.Height), tf);
                };
                p.Click += (s,e) => { if (_activeBtn != null) { _activeBtn.BackColor = AppTheme.SidebarBg; _activeBtn.Invalidate(); } _activeBtn = p; p.BackColor = AppTheme.SidebarBg; p.Invalidate(); act(); };
                p.MouseEnter += (s,e) => { if (_activeBtn != p) { p.BackColor = AppTheme.SidebarHover; p.Invalidate(); } };
                p.MouseLeave += (s,e) => { if (_activeBtn != p) { p.BackColor = AppTheme.SidebarBg; p.Invalidate(); } };
                items.Add(p);
            }

            Sec("الرئيسية");         Btn("لوحة التحكم", ShowDashboard);
            Sec("الأطراف");          Btn("العملاء", () => OpenPage(new CustomersForm(),"العملاء")); Btn("الموردين", () => OpenPage(new ImportersForm(),"الموردين")); Btn("المناديب", () => OpenPage(new TradersForm(),"المناديب"));
            Sec("الأصناف والمخازن"); Btn("الأصناف", () => OpenPage(new GoodsForm(),"الأصناف")); Btn("المخازن", () => OpenPage(new StoresForm(),"المخازن")); Btn("تحويل مخازن", () => OpenPage(new StoreTransferForm(),"تحويل مخازن")); Btn("جرد نهاية الشهر", () => OpenPage(new MonthlyInventoryForm(),"جرد نهاية الشهر"));
            Sec("الإنتاج");          Btn("المواد الخام", () => OpenPage(new RawMaterialsForm(),"المواد الخام")); Btn("وصفات الإنتاج", () => OpenPage(new ProductionRecipesForm(),"وصفات الإنتاج")); Btn("أوامر الإنتاج", () => OpenPage(new ProductionOrdersForm(),"أوامر الإنتاج")); Btn("إعادة التخزين", () => OpenPage(new RestockingForm(),"إعادة التخزين"));
            Sec("المبيعات");         Btn("فاتورة مبيعات", () => OpenPage(new SellBillForm(_currentUser),"فاتورة مبيعات")); Btn("سجل المبيعات", () => OpenPage(new SellBillsListForm(),"سجل المبيعات")); Btn("التحصيلات", () => OpenPage(new PaymentsForm(true),"التحصيلات"));
            Sec("المشتريات");        Btn("فاتورة مشتريات", () => OpenPage(new BuyBillForm(_currentUser),"فاتورة مشتريات")); Btn("سجل المشتريات", () => OpenPage(new BuyBillsListForm(),"سجل المشتريات")); Btn("المدفوعات", () => OpenPage(new PaymentsForm(false),"المدفوعات"));
            Sec("الحسابات");         Btn("الخزينة", () => OpenPage(new BoxForm(),"الخزينة")); Btn("البنوك", () => OpenPage(new BanksForm(),"البنوك")); Btn("المصروفات", () => OpenPage(new ExpensesForm(),"المصروفات")); Btn("الإيرادات", () => OpenPage(new IncomesForm(),"الإيرادات")); Btn("القيد اليومي", () => OpenPage(new JournalForm(),"القيد اليومي")); Btn("شجرة الحسابات", () => OpenPage(new AccountsTreeForm(),"شجرة الحسابات"));
            Sec("الموظفين");         Btn("الموظفون", () => OpenPage(new EmployeesForm(),"الموظفين"));
            Sec("الطلبات والتوصيل"); Btn("الطلبات", () => OpenPage(new OrdersForm(),"الطلبات")); Btn("التوصيل", () => OpenPage(new DeliveryForm(),"التوصيل")); Btn("السيارات", () => OpenPage(new CarsForm(),"السيارات"));
            Sec("التقارير");         Btn("تقارير شهرية", () => OpenPage(new MonthlyReportsForm(),"التقارير الشهرية")); Btn("كشف عميل", () => OpenPage(new ReportCustomerStatement(),"كشف حساب عميل")); Btn("كشف مورد", () => OpenPage(new ReportImporterStatement(),"كشف حساب مورد")); Btn("المخزون", () => OpenPage(new ReportStock(),"تقرير المخزون")); Btn("المبيعات", () => OpenPage(new ReportSales(),"تقرير المبيعات")); Btn("المشتريات", () => OpenPage(new ReportPurchases(),"تقرير المشتريات")); Btn("الأرباح", () => OpenPage(new ReportProfits(),"تقرير الأرباح"));
            Sec("الإعدادات");        Btn("بيانات الشركة", () => OpenPage(new CompanyForm(),"بيانات الشركة")); Btn("السنوات المالية", () => OpenPage(new FiscalYearForm(),"السنوات المالية")); Btn("المستخدمين", () => OpenPage(new UsersForm(),"المستخدمين")); Btn("أنواع المصروفات", () => OpenPage(new CostTypesForm(),"أنواع المصروفات")); Btn("النسخ الاحتياطي", () => OpenPage(new BackupForm(),"النسخ الاحتياطي")); Btn("إعداد الاتصال", () => VendorAction(() => new SetupConnectionForm().ShowDialog(this))); Btn("توليد ترخيص", () => VendorAction(() => new LicenseGeneratorForm().ShowDialog(this)));

            foreach (var item in items) { menu.Controls.Add(item); menu.Controls.SetChildIndex(item,0); }
            sidebar.Controls.Add(menu); sidebar.Controls.Add(logout); sidebar.Controls.Add(logo);
        }
        private void OpenPage(Form form, string title) { lblPageTitle.Text = title; pnlContent.Controls.Clear(); form.TopLevel = false; form.FormBorderStyle = FormBorderStyle.None; form.Dock = DockStyle.Fill; form.RightToLeft = RightToLeft.Yes; form.RightToLeftLayout = true; pnlContent.Controls.Add(form); form.Show(); }

        private void VendorAction(Action action) {
            using var dlg = new Form { Text = "Vendor", Size = new Size(340,160), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, BackColor = AppTheme.Surface, RightToLeft = RightToLeft.Yes, RightToLeftLayout = true };
            var lbl = new Label { Text = "كلمة مرور المورد:", Top = 18, Left = 10, Width = 300, Height = 22, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
            var txt = new TextBox { Top = 44, Left = 10, Width = 300, Height = 30, Font = new Font("Segoe UI", 10), PasswordChar = '*', BorderStyle = BorderStyle.FixedSingle };
            var btnOk = new Button { Text = "دخول", Top = 84, Left = 10, Width = 140, Height = 36, BackColor = AppTheme.Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI",10,FontStyle.Bold), DialogResult = DialogResult.OK };
            var btnNo = new Button { Text = "إلغاء", Top = 84, Left = 160, Width = 140, Height = 36, BackColor = AppTheme.Danger, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI",10,FontStyle.Bold), DialogResult = DialogResult.Cancel };
            btnOk.FlatAppearance.BorderSize = btnNo.FlatAppearance.BorderSize = 0;
            dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnNo }); dlg.AcceptButton = btnOk; dlg.CancelButton = btnNo;
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (txt.Text != "ERP@Amira#2026") { MessageBox.Show("كلمة المرور غير صحيحة","خطأ",MessageBoxButtons.OK,MessageBoxIcon.Error); return; }
            action();
        }

        private void ShowDashboard() {
            if (lblPageTitle != null) lblPageTitle.Text = "لوحة التحكم";
            pnlContent?.Controls.Clear();
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var query = db.SellBills.Include(b => b.Customer).AsQueryable();
            if (_currentFiscalYearId.HasValue) query = query.Where(b => b.FiscalYearId == _currentFiscalYearId.Value);
            var bills = query.OrderByDescending(b => b.Date).Take(8).ToList();
            var boxQ = db.BoxTransactions.AsQueryable();
            if (_currentFiscalYearId.HasValue) boxQ = boxQ.Where(bt => bt.FiscalYearId == _currentFiscalYearId.Value);
            var stats = new[] {
                ("اجمالي العملاء",  db.Customers.Count().ToString(), AppTheme.Primary, "C"),
                ("اجمالي الموردين", db.Importers.Count().ToString(), AppTheme.Accent,  "S"),
                ("اجمالي الاصناف",  db.Goods.Count().ToString(),    AppTheme.Warning,  "G"),
                ("فواتير اليوم",    query.Count(b => b.Date.Date == DateTime.Today).ToString(), AppTheme.Info, "فواتير"),
                ("مبيعات اليوم",    (query.Where(b=>b.Date.Date==DateTime.Today).Sum(b=>(double?)b.Asked)??0).ToString("N0"), AppTheme.Danger, "مبيعات"),
                ("رصيد الخزينة",    (boxQ.Sum(b=>b.Out?-(double)b.Value:b.Value)).ToString("N0"), AppTheme.AccentDark, "الخزينة"),
            };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Color.Transparent };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var cardsRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0,4,0,4) };
            for (int i = 0; i < 6; i++) cardsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            for (int i = 0; i < stats.Length; i++) { var (t,v,c,ic) = stats[i]; cardsRow.Controls.Add(StatCard(t,v,c,ic), i, 0); }
            var secHdr = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(0,8,0,0) };
            secHdr.Controls.Add(new Label { Text = "آخر فواتير المبيعات", Dock = DockStyle.Fill, Font = AppTheme.FontMedium, ForeColor = AppTheme.TextDark, TextAlign = ContentAlignment.MiddleRight, RightToLeft = RightToLeft.Yes });
            var grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.DataSource = bills.Select(b => new { رقم = b.Code, التاريخ = b.Date.ToString("yyyy/MM/dd"), العميل = b.Customer?.Name??"", الاجمالي = b.Asked.ToString("N2"), المدفوع = b.Paid.ToString("N2"), المتبقي = Math.Max(0,b.Asked-b.Paid).ToString("N2") }).ToList();
            layout.Controls.Add(cardsRow,0,0); layout.Controls.Add(secHdr,0,1); layout.Controls.Add(UIHelper.WrapGrid(grid),0,2);
            pnlContent?.Controls.Add(layout);
        }

        private static Panel StatCard(string title, string value, Color color, string icon) {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface, Margin = new Padding(5,0,5,0), Padding = new Padding(14,10,14,10) };
            card.Paint += (s,e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0,0,card.Width-1,card.Height-1);
                using var path = UIHelper.RoundedRect(r,AppTheme.Radius);
                using var bg = new SolidBrush(AppTheme.Surface); g.FillPath(bg,path);
                using var ac = new SolidBrush(color); g.FillRectangle(ac,r.Right-4,r.Y+8,4,r.Height-16);
                using var bd = new Pen(AppTheme.Border,1f); g.DrawPath(bd,path);
            };
            var lblTitle = new Label { Text = title, Font = AppTheme.FontBold, ForeColor = color, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleRight, BackColor = Color.Transparent };
            var lblValue = new Label { Text = value, Font = new Font("Segoe UI",18,FontStyle.Bold), ForeColor = AppTheme.TextDark, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, BackColor = Color.Transparent };
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            return card;
        }
    }
}