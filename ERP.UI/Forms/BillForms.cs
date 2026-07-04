using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class SellBillsListForm : Form
    {
        private List<SellBill> _all = new();
        private DataGridView grid = null!;
        private ComboBox cboCustomer = null!;
        private DateTimePicker dtpFrom = null!, dtpTo = null!;
        private TextBox txtSearch = null!;
        private Label lblSummary = null!;

        public SellBillsListForm()
        {
            this.Text = "فواتير المبيعات";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            BuildLayout();
        }

        private void BuildLayout()
        {
            // ── filter bar: customer dropdown + date range + free-text search ──
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            pnlFilter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 9,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 8, 8, 8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: customer
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));    // cboCustomer
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: from
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // txtFrom
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: to
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // txtTo
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: search
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // txtSearch
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));  // btnShow

            cboCustomer = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 0, 8, 0) };
            dtpFrom = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-3), Margin = new Padding(0, 0, 8, 0) };
            dtpTo   = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 0, 8, 0) };
            txtSearch = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, PlaceholderText = "🔍 رقم فاتورة / ملاحظات", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 8, 0) };
            var btnShow = UIHelper.MakeButton("🔍 عرض", AppTheme.Primary, new Size(90, 36), Point.Empty);
            btnShow.Dock = DockStyle.Fill; btnShow.Margin = new Padding(0);

            tbl.Controls.Add(new Label { Text = "العميل:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            tbl.Controls.Add(cboCustomer, 1, 0);
            tbl.Controls.Add(new Label { Text = "من:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            tbl.Controls.Add(dtpFrom, 3, 0);
            tbl.Controls.Add(new Label { Text = "إلى:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 4, 0);
            tbl.Controls.Add(dtpTo, 5, 0);
            tbl.Controls.Add(new Label { Text = "بحث:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 6, 0);
            tbl.Controls.Add(txtSearch, 7, 0);
            tbl.Controls.Add(btnShow, 8, 0);
            pnlFilter.Controls.Add(tbl);

            // ── summary bar: totals for the current filtered result set ──
            var pnlSummary = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(239, 246, 255) };
            lblSummary = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };
            pnlSummary.Controls.Add(lblSummary);

            // ── action buttons: edit, delete, refresh ──
            var pnlBtns = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            pnlBtns.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlBtns.Height - 1, pnlBtns.Width, pnlBtns.Height - 1);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(8, 6, 8, 0) };
            var btnEdit = UIHelper.MakeButton("✏ تعديل", AppTheme.Primary, new Size(100, 36), Point.Empty); btnEdit.Margin = new Padding(0, 0, 8, 0);
            var btnDelete = UIHelper.MakeButton("🗑 حذف", AppTheme.Danger, new Size(100, 36), Point.Empty); btnDelete.Margin = new Padding(0, 0, 8, 0);
            var btnRefresh = UIHelper.MakeButton("🔄 تحديث", AppTheme.TextGray, new Size(100, 36), Point.Empty); btnRefresh.Margin = new Padding(0, 0, 8, 0);
            btnEdit.Click += (s, e) => OnEdit();
            btnDelete.Click += (s, e) => OnDelete();
            btnRefresh.Click += (s, e) => LoadData();
            flow.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnRefresh });
            pnlBtns.Controls.Add(flow);

            // ── data grid ──
            grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.DataBindingComplete += (s, e) =>
            {
                var g = s as DataGridView;
                var cols = g?.Columns;
                if (cols != null && cols.Contains("Id"))
                {
                    var col = cols["Id"];
                    if (col != null) col.Visible = false;
                }
            };
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OnEdit(); };
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "حالة_الدفع" && e.Value != null)
                {
                    string status = e.Value.ToString()!;
                    if (status.Contains("بالكامل"))
                    {
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); // green-100
                        e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61);   // green-700
                    }
                    else if (status.Contains("آجل"))
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); // red-100
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);   // red-700
                    }
                    else if (status.Contains("جزئياً"))
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // yellow-100
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);    // yellow-700
                    }
                }
            };

            var gridWrap = UIHelper.WrapGrid(grid);

            btnShow.Click += (s, e) => LoadData();
            txtSearch.TextChanged += (s, e) => FilterGrid();

            this.Controls.Add(gridWrap);
            this.Controls.Add(pnlSummary);
            this.Controls.Add(pnlBtns);
            this.Controls.Add(pnlFilter);

            LoadCustomerCombo();
        }

        private void LoadCustomerCombo()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var list = db.Customers.OrderBy(c => c.Name).ToList();
            list.Insert(0, new Customer { Id = 0, Name = "-- كل العملاء --" });
            cboCustomer.DisplayMember = "Name"; cboCustomer.ValueMember = "Id";
            cboCustomer.DataSource = list;
            cboCustomer.SelectedIndex = 0;
        }

        protected void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            var from = dtpFrom.Value.Date;
            var to   = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

            int custId = 0;
            if (cboCustomer.SelectedItem is Customer selCust && selCust.Id > 0)
                custId = selCust.Id;

            var query = db.SellBills.Include(b => b.Customer)
                          .Where(b => b.Date >= from && b.Date <= to);
            if (custId > 0) query = query.Where(b => b.CustomerId == custId);

            if (MainForm.CurrentFiscalYearId.HasValue)
                query = query.Where(b => b.FiscalYearId == MainForm.CurrentFiscalYearId.Value);

            _all = query.OrderByDescending(b => b.Date).ToList();
            FilterGrid();
        }

        private void FilterGrid()
        {
            var k = txtSearch.Text.Trim();
            var list = string.IsNullOrWhiteSpace(k) ? _all
                : _all.Where(b =>
                    b.Code.ToString().Contains(k) ||
                    (b.Customer?.Name ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    (b.Notes ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();

            grid.DataSource = list.Select(b => new
            {
                Id            = b.Id,
                رقم_الفاتورة  = b.Code,
                التاريخ       = b.Date.ToString("yyyy/MM/dd"),
                العميل        = b.Customer?.Name ?? "",
                الإجمالي      = b.Asked.ToString("N2"),
                المدفوع       = b.Paid.ToString("N2"),
                المتبقي       = Math.Max(0, b.Asked - b.Paid).ToString("N2"),
                حالة_الدفع    = b.Paid >= b.Asked ? "مدفوعة بالكامل ✅" : (b.Paid <= 0 ? "آجل ❌" : "مدفوعة جزئياً ⚠️"),
                الخصم         = b.DisPercent + "%",
                ملاحظات       = b.Notes
            }).ToList();

            double totAsked   = list.Sum(b => b.Asked);
            double totPaid    = list.Sum(b => b.Paid);
            double totRemain  = list.Sum(b => Math.Max(0, b.Asked - b.Paid));
            lblSummary.Text = $"عدد الفواتير: {list.Count}    |    إجمالي المبيعات: {totAsked:N2}    |    إجمالي المحصّل: {totPaid:N2}    |    متبقي لنا: {totRemain:N2}";
        }

        private int? GetSelectedId()
        {
            if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("اختار فاتورة أولاً"); return null; }
            var row = grid.SelectedRows[0];
            if (grid.Columns.Contains("Id") && row.Cells["Id"].Value is int id) return id;
            return null;
        }

        private void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new SellBillEditForm(id.Value);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه الفاتورة؟\nسيتم عكس أثرها على المخزن والخزينة.")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var bill = db.SellBills.Find(id);
            if (bill != null)
            {
                db.Movements.RemoveRange(db.Movements.Where(m => m.BillNo == bill.Code.ToString() && m.IsBill && m.Out).ToList());
                db.BoxTransactions.RemoveRange(db.BoxTransactions.Where(b => b.SellBillId == bill.Id).ToList());
                db.CustomerInstallments.RemoveRange(db.CustomerInstallments.Where(ci => ci.BillId == bill.Id).ToList());
                db.SellBills.Remove(bill);
                db.SaveChanges();
            }
            LoadData();
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); LoadData(); }
    }

    // ══════════════════════ SELL BILL (NEW) ══════════════════════
    // ── Shared DTO for goods combo ──
    internal sealed class GoodComboItem
    {
        public int    Id       { get; set; }
        public string Code     { get; set; } = "";
        public string Name     { get; set; } = "";
        public float  SellPrice{ get; set; }
        public float  BuyPrice { get; set; }
        public string UnitName { get; set; } = "";
        public string Label    => $"{Code} — {Name}";
    }

    public class SellBillForm : Form
    {
        private readonly User _user;

        // Header controls
        private ComboBox cboCustomer = null!, cboStore = null!, cboSeller = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtNotes = null!, txtDisPercent = null!;

        // Add-item bar
        private ComboBox cboGood = null!;
        private TextBox txtQty = null!, txtPrice = null!, txtItemDis = null!;
        private Label lblUnit = null!;

        // Items list
        private DataGridView dgItems = null!;
        private List<SellLineItem> _lines = new();
        private record SellLineItem(int GoodId, string Code, string Name, string Unit, float Qty, float Price, float Dis, float BuyPrice)
        { public double LineTotal => Math.Round(Qty * Price * (1 - Dis / 100.0), 2); }

        // Footer
        private Label lblTotal = null!, lblRemain = null!;
        private TextBox txtPaid = null!;

        public SellBillForm(User user) { _user = user; BuildUI(); LoadCombos(); }

        private void BuildUI()
        {
            Text = "🧾 فاتورة مبيعات جديدة";
            BackColor = AppTheme.Light;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            /* ── HEADER ── */
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(8, 6, 8, 4) };
            pnlHeader.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            var row1 = BillHelper.MakeRow(8);
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            cboCustomer  = Cbo(); dtpDate = new DateTimePicker { Value = DateTime.Today, Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Margin = new Padding(0,3,8,3) };
            cboStore     = Cbo(); cboSeller = Cbo();
            row1.Controls.Add(BillHelper.Lbl("العميل *:"), 0, 0);   row1.Controls.Add(cboCustomer, 1, 0);
            row1.Controls.Add(BillHelper.Lbl("التاريخ:"),  2, 0);   row1.Controls.Add(dtpDate,     3, 0);
            row1.Controls.Add(BillHelper.Lbl("المخزن:"),  4, 0);   row1.Controls.Add(cboStore,    5, 0);
            row1.Controls.Add(BillHelper.Lbl("المندوب:"), 6, 0);   row1.Controls.Add(cboSeller,   7, 0);

            var row2 = BillHelper.MakeRow(4);
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            txtDisPercent = Txt("0"); txtNotes = Txt("");
            row2.Controls.Add(BillHelper.Lbl("خصم إجمالي %:"), 0, 0); row2.Controls.Add(txtDisPercent, 1, 0);
            row2.Controls.Add(BillHelper.Lbl("ملاحظات:"),       2, 0); row2.Controls.Add(txtNotes, 3, 0);

            pnlHeader.Controls.Add(row2); pnlHeader.Controls.Add(row1);

            /* ── ADD-ITEM BAR ── */
            var pnlAdd = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(239, 246, 255), Padding = new Padding(8, 6, 8, 6) };
            pnlAdd.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlAdd.Height - 1, pnlAdd.Width, pnlAdd.Height - 1);

            var addRow = BillHelper.MakeRow(10); addRow.Dock = DockStyle.Fill;
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));

            cboGood   = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 6, 0) };
            cboGood.SelectedIndexChanged += SellGoodChanged;
            txtQty     = Txt("1"); txtPrice = Txt("0"); txtItemDis = Txt("0");
            lblUnit    = new Label { Text = "", Dock = DockStyle.Fill, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };

            var btnAddItem = UIHelper.MakeButton("➕", AppTheme.Accent, new Size(40, 36), Point.Empty);
            btnAddItem.Dock = DockStyle.Fill; btnAddItem.Click += SellAddItem;
            btnAddItem.Font = new Font("Segoe UI", 13); btnAddItem.Margin = new Padding(4, 0, 0, 0);

            var btnNewGood = UIHelper.MakeButton("🆕", AppTheme.Primary, new Size(40, 36), Point.Empty);
            btnNewGood.Dock = DockStyle.Fill; btnNewGood.Click += SellAddNewGood;
            btnNewGood.Font = new Font("Segoe UI", 11); btnNewGood.Margin = new Padding(2, 0, 0, 0);
            btnNewGood.Tag = "صنف جديد يُضاف لقائمة الأصناف";

            addRow.Controls.Add(BillHelper.Lbl("الصنف:"),  0, 0); addRow.Controls.Add(cboGood,    1, 0);
            addRow.Controls.Add(BillHelper.Lbl("كمية:"),   2, 0); addRow.Controls.Add(txtQty,     3, 0);
            addRow.Controls.Add(BillHelper.Lbl("وحدة:"),   4, 0); addRow.Controls.Add(lblUnit,    5, 0);
            addRow.Controls.Add(BillHelper.Lbl("سعر:"),    6, 0); addRow.Controls.Add(txtPrice,   7, 0);
            addRow.Controls.Add(BillHelper.Lbl("خصم%:"),   8, 0); addRow.Controls.Add(txtItemDis, 9, 0);

            var addWrap = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
            addWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            addWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            addWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            addWrap.Controls.Add(addRow, 0, 0); addWrap.Controls.Add(btnAddItem, 1, 0); addWrap.Controls.Add(btnNewGood, 2, 0);
            pnlAdd.Controls.Add(addWrap);

            /* ── GRID ── */
            dgItems = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(dgItems);
            dgItems.ReadOnly = true; dgItems.AllowUserToAddRows = false;
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code",  HeaderText = "الكود",    Width = 80  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",  HeaderText = "الصنف",    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit",  HeaderText = "الوحدة",   Width = 70  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty",   HeaderText = "الكمية",   Width = 80  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "السعر",    Width = 90  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dis",   HeaderText = "خصم%",     Width = 60  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 110 });
            dgItems.Columns.Add(new DataGridViewButtonColumn  { Name = "Del",   HeaderText = "",         Width = 44, Text = "🗑", UseColumnTextForButtonValue = true });
            dgItems.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgItems.Columns["Del"]!.Index)
                { _lines.RemoveAt(e.RowIndex); SellRefresh(); }
            };

            /* ── FOOTER ── */
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.White };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFooter.Width, 0);
            var fRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(4, 7, 8, 0) };

            lblTotal  = FLbl(Color.FromArgb(30, 41, 59));
            var lpLbl = new Label { Text = "المدفوع:", AutoSize = false, Size = new Size(72, 34), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(10, 0, 4, 0) };
            txtPaid   = new TextBox { Size = new Size(120, 28), Font = AppTheme.FontNormal, Text = "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 6, 0) };
            lblRemain = FLbl(AppTheme.Danger);
            txtPaid.TextChanged += (s, e) => SellUpdateFooter();

            var btnSave  = UIHelper.MakeButton("💾 حفظ الفاتورة", AppTheme.Accent,   new Size(155, 40), Point.Empty); btnSave.Margin  = new Padding(6, 3, 4, 0); btnSave.Click  += SellSave;
            var btnClear = UIHelper.MakeButton("🔄 تفريغ",         AppTheme.Warning,  new Size(100, 40), Point.Empty); btnClear.Margin = new Padding(0, 3, 4, 0); btnClear.Click += (s, e) => { _lines.Clear(); SellRefresh(); };

            fRow.Controls.AddRange(new Control[] { btnSave, btnClear, lblRemain, txtPaid, lpLbl, lblTotal });
            pnlFooter.Controls.Add(fRow);

            Controls.Add(UIHelper.WrapGrid(dgItems));
            Controls.Add(pnlFooter); Controls.Add(pnlAdd); Controls.Add(pnlHeader);
        }

        private void LoadGoods()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var list = db.Goods.Include(g => g.Unit).OrderBy(g => g.Name)
                .Select(g => new GoodComboItem { Id = g.Id, Code = g.Code, Name = g.Name, SellPrice = (float)g.SellPrice, BuyPrice = (float)g.BuyPrice, UnitName = g.Unit != null ? g.Unit.UnitName : "" })
                .ToList();
            cboGood.DisplayMember = "Label"; cboGood.ValueMember = "Id";
            cboGood.DataSource = list;
        }

        private void LoadCombos()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            cboCustomer.DisplayMember = "Name"; cboCustomer.ValueMember = "Id";
            cboCustomer.DataSource = db.Customers.OrderBy(c => c.Name).ToList();
            cboStore.DisplayMember = "StoreName"; cboStore.ValueMember = "Id";
            cboStore.DataSource = db.Stores.OrderBy(s => s.StoreName).ToList();
            var sellers = db.Traders.OrderBy(t => t.Name).Select(t => (object)t).ToList();
            sellers.Insert(0, new Trader { Id = 0, Name = "-- بدون مندوب --" });
            cboSeller.DisplayMember = "Name"; cboSeller.ValueMember = "Id"; cboSeller.DataSource = sellers;
            LoadGoods();
        }

        private void SellGoodChanged(object? sender, EventArgs e)
        {
            if (cboGood.SelectedItem is not GoodComboItem item) return;
            txtPrice.Text = item.SellPrice.ToString("N2");
            lblUnit.Text  = item.UnitName;
            txtQty.Focus(); txtQty.SelectAll();
        }

        private void SellAddItem(object? sender, EventArgs e)
        {
            if (cboGood.SelectedItem is not GoodComboItem sel) { UIHelper.ShowError("اختار صنفاً أولاً"); return; }
            if (!float.TryParse(txtQty.Text.Trim(),   out float qty)   || qty   <= 0) { UIHelper.ShowError("الكمية يجب أن تكون أكبر من صفر"); return; }
            if (!float.TryParse(txtPrice.Text.Trim(), out float price) || price <= 0) { UIHelper.ShowError("السعر يجب أن يكون أكبر من صفر"); return; }
            float dis = float.TryParse(txtItemDis.Text.Trim(), out float d) ? Math.Clamp(d, 0, 100) : 0;
            _lines.Add(new SellLineItem(sel.Id, sel.Code, sel.Name, sel.UnitName, qty, price, dis, sel.BuyPrice));
            SellRefresh(); txtQty.Text = "1"; txtItemDis.Text = "0";
        }

        private void SellAddNewGood(object? sender, EventArgs e)
        {
            using var dlg = new QuickAddGoodDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            LoadGoods();
            if (dlg.NewGoodId > 0)
                foreach (GoodComboItem item in cboGood.Items)
                    if (item.Id == dlg.NewGoodId) { cboGood.SelectedItem = item; break; }
        }

        private void SellRefresh()
        {
            dgItems.Rows.Clear();
            foreach (var ln in _lines)
                dgItems.Rows.Add(ln.Code, ln.Name, ln.Unit, ln.Qty.ToString("N2"), ln.Price.ToString("N2"), ln.Dis > 0 ? ln.Dis + "%" : "", ln.LineTotal.ToString("N2"), "🗑");
            SellUpdateFooter();
        }

        private void SellUpdateFooter()
        {
            double total  = _lines.Sum(l => l.LineTotal);
            double paid   = double.TryParse(txtPaid.Text, out double p) ? p : 0;
            double remain = total - paid;
            lblTotal.Text  = $"الإجمالي: {total:N2}";
            lblRemain.Text = remain > 0.005 ? $"⚠ متبقي: {remain:N2}" : "✅ مدفوع بالكامل";
            lblRemain.ForeColor = remain > 0.005 ? AppTheme.Danger : AppTheme.Accent;
        }

        private void SellSave(object? sender, EventArgs e)
        {
            if (cboCustomer.SelectedItem == null) { UIHelper.ShowError("اختار العميل"); return; }
            if (!_lines.Any())                    { UIHelper.ShowError("أضف صنفاً واحداً على الأقل"); return; }

            double asked  = _lines.Sum(l => l.LineTotal);
            double paid   = double.TryParse(txtPaid.Text,  out double pv) ? pv : 0;
            float  disPer = float.TryParse(txtDisPercent.Text, out float dv) ? dv : 0;
            int    custId = (int)cboCustomer.SelectedValue!;
            int    storeId = cboStore.SelectedIndex >= 0 ? (int)cboStore.SelectedValue! : 1;
            int?   sellId  = cboSeller.SelectedIndex > 0  ? (int?)cboSeller.SelectedValue : null;
            DateTime billDate = dtpDate.Value.Date;

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            int fyId = MainForm.CurrentFiscalYearId
                ?? db.FiscalYears.Where(f => !f.IsClosed).OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();
            if (fyId == 0) fyId = db.FiscalYears.OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();

            int nextCode = (db.SellBills.Max(b => (int?)b.Code) ?? 0) + 1;
            var bill = new SellBill { Code = nextCode, CustomerId = custId, Date = billDate, Time = DateTime.Now, Asked = asked, Paid = paid, Notes = txtNotes.Text.Trim(), DisPercent = disPer, UserId = _user.Id, StoreNo = storeId, BillType = 1, SellerId = sellId, FiscalYearId = fyId };
            db.SellBills.Add(bill); db.SaveChanges();

            if (paid > 0)
            {
                var cname = db.Customers.Find(custId)?.Name ?? "";
                db.BoxTransactions.Add(new BoxTransaction { Out = false, Value = paid, Date = billDate, Time = DateTime.Now, Notes = $"تحصيل فاتورة مبيعات #{nextCode} - {cname}", CustName = cname, SellBillId = bill.Id, FiscalYearId = fyId, No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1 });
            }

            foreach (var ln in _lines)
            {
                db.CustomerInstallments.Add(new CustomerInstallment { BillId = bill.Id, CustomerId = custId, Date = billDate, GoodId = ln.GoodId, Quantity = ln.Qty, Price = ln.Price, DisPerItem = ln.Dis, Total = (float)ln.LineTotal, Pay = 0, StoreId = storeId, BuyPrice = ln.BuyPrice });
                db.Movements.Add(new Movement { GoodId = ln.GoodId, Quantity = ln.Qty, Date = billDate, Out = true, IsBill = true, BillNo = nextCode.ToString(), SellPrice = ln.Price, BuyPrice = ln.BuyPrice, StoreNo = storeId, CustomerId = custId, FiscalYearId = fyId });
            }
            db.SaveChanges();

            UIHelper.ShowSuccess($"✅ تم حفظ الفاتورة رقم {nextCode}\nالإجمالي: {asked:N2}   المدفوع: {paid:N2}");
            _lines.Clear(); SellRefresh(); txtPaid.Text = "0";
        }

        // ── helpers ──
        private static ComboBox Cbo() => new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 2) };
        private static TextBox Txt(string v) => new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = v, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 8, 3) };
        private static Label FLbl(Color c) => new Label { Text = "", AutoSize = false, Size = new Size(190, 34), Font = AppTheme.FontBold, ForeColor = c, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0) };
    }


    // ══════════════════════════════════════════════════════════════
    // BUY BILLS LIST
    // Filterable by supplier, date range, and free-text search.
    // Shows a running summary bar (count, total, paid, remaining).
    // ══════════════════════════════════════════════════════════════
    public class BuyBillsListForm : Form
    {
        private List<BuyBill> _all = new();
        private DataGridView grid = null!;
        private ComboBox cboImporter = null!;
        private DateTimePicker dtpFrom = null!, dtpTo = null!;
        private TextBox txtSearch = null!;
        private Label lblSummary = null!;

        public BuyBillsListForm()
        {
            this.Text = "فواتير المشتريات";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            BuildLayout();
        }

        private void BuildLayout()
        {
            // ── filter bar: supplier dropdown + date range + free-text search ──
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            pnlFilter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 9,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 8, 8, 8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: supplier
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));    // cboImporter
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: from
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // txtFrom
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: to
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));  // txtTo
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));       // label: search
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // txtSearch
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));  // btnShow

            cboImporter = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4, 0, 8, 0) };
            dtpFrom = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-3), Margin = new Padding(0, 0, 8, 0) };
            dtpTo   = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 0, 8, 0) };
            txtSearch = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, PlaceholderText = "🔍 رقم فاتورة / ملاحظات", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 8, 0) };
            var btnShow = UIHelper.MakeButton("🔍 عرض", AppTheme.Primary, new Size(90, 36), Point.Empty);
            btnShow.Dock = DockStyle.Fill; btnShow.Margin = new Padding(0);

            tbl.Controls.Add(new Label { Text = "المورد:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            tbl.Controls.Add(cboImporter, 1, 0);
            tbl.Controls.Add(new Label { Text = "من:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            tbl.Controls.Add(dtpFrom, 3, 0);
            tbl.Controls.Add(new Label { Text = "إلى:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 4, 0);
            tbl.Controls.Add(dtpTo, 5, 0);
            tbl.Controls.Add(new Label { Text = "بحث:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 6, 0);
            tbl.Controls.Add(txtSearch, 7, 0);
            tbl.Controls.Add(btnShow, 8, 0);
            pnlFilter.Controls.Add(tbl);

            // ── summary bar: totals for the current filtered result set ──
            var pnlSummary = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(239, 246, 255) };
            lblSummary = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };
            pnlSummary.Controls.Add(lblSummary);

            // ── action buttons: edit, delete, refresh ──
            var pnlBtns = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
            pnlBtns.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlBtns.Height - 1, pnlBtns.Width, pnlBtns.Height - 1);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(8, 6, 8, 0) };
            var btnEdit = UIHelper.MakeButton("✏ تعديل", AppTheme.Primary, new Size(100, 36), Point.Empty); btnEdit.Margin = new Padding(0, 0, 8, 0);
            var btnDelete = UIHelper.MakeButton("🗑 حذف", AppTheme.Danger, new Size(100, 36), Point.Empty); btnDelete.Margin = new Padding(0, 0, 8, 0);
            var btnRefresh = UIHelper.MakeButton("🔄 تحديث", AppTheme.TextGray, new Size(100, 36), Point.Empty); btnRefresh.Margin = new Padding(0, 0, 8, 0);
            btnEdit.Click += (s, e) => OnEdit();
            btnDelete.Click += (s, e) => OnDelete();
            btnRefresh.Click += (s, e) => LoadData();
            flow.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnRefresh });
            pnlBtns.Controls.Add(flow);

            // ── data grid ──
            grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.DataBindingComplete += (s, e) =>
            {
                var g = s as DataGridView;
                var cols = g?.Columns;
                if (cols != null && cols.Contains("Id"))
                {
                    var col = cols["Id"];
                    if (col != null) col.Visible = false;
                }
            };
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OnEdit(); };
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "حالة_الدفع" && e.Value != null)
                {
                    string status = e.Value.ToString()!;
                    if (status.Contains("بالكامل"))
                    {
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); // green-100
                        e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61);   // green-700
                    }
                    else if (status.Contains("آجل"))
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); // red-100
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);   // red-700
                    }
                    else if (status.Contains("جزئياً"))
                    {
                        e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // yellow-100
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);    // yellow-700
                    }
                }
            };

            btnShow.Click += (s, e) => LoadData();
            txtSearch.TextChanged += (s, e) => FilterGrid();

            this.Controls.Add(UIHelper.WrapGrid(grid));
            this.Controls.Add(pnlSummary);
            this.Controls.Add(pnlBtns);
            this.Controls.Add(pnlFilter);

            LoadImporterCombo();
        }

        private void LoadImporterCombo()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var list = db.Importers.OrderBy(i => i.Name).ToList();
            list.Insert(0, new Importer { Id = 0, Name = "-- كل الموردين --" });
            cboImporter.DisplayMember = "Name"; cboImporter.ValueMember = "Id";
            cboImporter.DataSource = list;
            cboImporter.SelectedIndex = 0;
        }

        private void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            var from = dtpFrom.Value.Date;
            var to   = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

            int impId = 0;
            if (cboImporter.SelectedItem is Importer selImp && selImp.Id > 0)
                impId = selImp.Id;

            var query = db.BuyBills.Include(b => b.Importer)
                          .Where(b => b.Date >= from && b.Date <= to);
            if (impId > 0) query = query.Where(b => b.ImporterId == impId);

            if (MainForm.CurrentFiscalYearId.HasValue)
                query = query.Where(b => b.FiscalYearId == MainForm.CurrentFiscalYearId.Value);

            _all = query.OrderByDescending(b => b.Date).ToList();
            FilterGrid();
        }

        private void FilterGrid()
        {
            var k = txtSearch.Text.Trim();
            var list = string.IsNullOrWhiteSpace(k) ? _all
                : _all.Where(b =>
                    b.Code.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    (b.Importer?.Name ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    (b.Notes ?? "").Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();

            grid.DataSource = list.Select(b => new
            {
                Id            = b.Id,
                رقم_الفاتورة  = b.Code,
                التاريخ       = b.Date.ToString("yyyy/MM/dd"),
                المورد        = b.Importer?.Name ?? "",
                الإجمالي      = b.Asked.ToString("N2"),
                المدفوع       = b.Paid.ToString("N2"),
                المتبقي       = Math.Max(0, b.Asked - b.Paid).ToString("N2"),
                حالة_الدفع    = b.Paid >= b.Asked ? "مدفوعة بالكامل ✅" : (b.Paid <= 0 ? "آجل ❌" : "مدفوعة جزئياً ⚠️"),
                ملاحظات       = b.Notes
            }).ToList();

            double totAsked  = list.Sum(b => (double)b.Asked);
            double totPaid   = list.Sum(b => (double)b.Paid);
            double totRemain = list.Sum(b => Math.Max(0, b.Asked - b.Paid));
            lblSummary.Text = $"عدد الفواتير: {list.Count}    |    إجمالي المشتريات: {totAsked:N2}    |    إجمالي المدفوع: {totPaid:N2}    |    متبقي علينا: {totRemain:N2}";
        }

        private int? GetSelectedId()
        {
            if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("اختار فاتورة أولاً"); return null; }
            var row = grid.SelectedRows[0];
            if (grid.Columns.Contains("Id") && row.Cells["Id"].Value is int id) return id;
            return null;
        }

        private void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new BuyBillEditForm(id.Value);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه الفاتورة؟\nسيتم عكس أثرها على المخزن والخزينة.")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var bill = db.BuyBills.Find(id);
            if (bill != null)
            {
                // Reverse stock movements linked to this purchase bill
                db.Movements.RemoveRange(db.Movements.Where(m => m.BillNo == bill.Code && !m.Out && m.IsBill).ToList());
                // Remove the treasury outflow entry linked by BuyBillId
                db.BoxTransactions.RemoveRange(db.BoxTransactions.Where(b => b.BuyBillId == bill.Id).ToList());
                db.ImporterInstallments.RemoveRange(db.ImporterInstallments.Where(ii => ii.BillId == bill.Id).ToList());
                db.BuyBills.Remove(bill);
                db.SaveChanges();
            }
            LoadData();
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); LoadData(); }
    }

    // ══════════════════════ BUY BILL (NEW) ══════════════════════
    public class BuyBillForm : Form
    {
        private readonly User _user;
        private ComboBox cboImporter = null!, cboStore = null!, cboGood = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtNotes = null!, txtCode = null!;
        private TextBox txtQty = null!, txtPrice = null!;
        private Label lblUnit = null!;
        private DataGridView dgItems = null!;
        private Label lblTotal = null!, lblRemain = null!;
        private TextBox txtPaid = null!;
        private List<BuyLineItem> _lines = new();
        private record BuyLineItem(int GoodId, string Code, string Name, string Unit, float Qty, float Price)
        { public float LineTotal => Qty * Price; }

        public BuyBillForm(User user) { _user = user; BuildUI(); LoadCombos(); }

        private void BuildUI()
        {
            Text = "🛒 فاتورة مشتريات جديدة";
            BackColor = AppTheme.Light;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;

            /* ── HEADER ── */
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(8, 6, 8, 4) };
            pnlHeader.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            var row1 = BillHelper.MakeRow(8);
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cboImporter = BCbo(); dtpDate = new DateTimePicker { Value = DateTime.Today, Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Margin = new Padding(0, 3, 8, 3) };
            cboStore    = BCbo(); txtCode = BTxt("");
            row1.Controls.Add(BillHelper.Lbl("المورد *:"),     0, 0); row1.Controls.Add(cboImporter, 1, 0);
            row1.Controls.Add(BillHelper.Lbl("التاريخ:"),      2, 0); row1.Controls.Add(dtpDate,     3, 0);
            row1.Controls.Add(BillHelper.Lbl("المخزن:"),       4, 0); row1.Controls.Add(cboStore,    5, 0);
            row1.Controls.Add(BillHelper.Lbl("رقم الفاتورة:"), 6, 0); row1.Controls.Add(txtCode,     7, 0);

            var row2 = BillHelper.MakeRow(2);
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            txtNotes = BTxt("");
            row2.Controls.Add(BillHelper.Lbl("ملاحظات:"), 0, 0); row2.Controls.Add(txtNotes, 1, 0);

            pnlHeader.Controls.Add(row2); pnlHeader.Controls.Add(row1);

            /* ── ADD-ITEM BAR ── */
            var pnlAdd = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(239, 246, 255), Padding = new Padding(8, 6, 8, 6) };
            pnlAdd.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlAdd.Height - 1, pnlAdd.Width, pnlAdd.Height - 1);

            var addRow = BillHelper.MakeRow(8); addRow.Dock = DockStyle.Fill;
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            cboGood  = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 6, 0) };
            cboGood.SelectedIndexChanged += BuyGoodChanged;
            txtQty   = BTxt("1"); txtPrice = BTxt("0");
            lblUnit  = new Label { Text = "", Dock = DockStyle.Fill, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };

            var btnAddItem = UIHelper.MakeButton("➕", AppTheme.Accent, new Size(40, 36), Point.Empty);
            btnAddItem.Dock = DockStyle.Fill; btnAddItem.Font = new Font("Segoe UI", 13);
            btnAddItem.Margin = new Padding(4, 0, 0, 0); btnAddItem.Click += BuyAddItem;

            var btnNewGood = UIHelper.MakeButton("🆕", AppTheme.Primary, new Size(40, 36), Point.Empty);
            btnNewGood.Dock = DockStyle.Fill; btnNewGood.Font = new Font("Segoe UI", 11);
            btnNewGood.Margin = new Padding(2, 0, 0, 0); btnNewGood.Click += BuyAddNewGood;

            addRow.Controls.Add(BillHelper.Lbl("الصنف:"),    0, 0); addRow.Controls.Add(cboGood,  1, 0);
            addRow.Controls.Add(BillHelper.Lbl("كمية:"),     2, 0); addRow.Controls.Add(txtQty,   3, 0);
            addRow.Controls.Add(BillHelper.Lbl("وحدة:"),     4, 0); addRow.Controls.Add(lblUnit,  5, 0);
            addRow.Controls.Add(BillHelper.Lbl("سعر شراء:"), 6, 0); addRow.Controls.Add(txtPrice, 7, 0);

            var addWrap = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
            addWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            addWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            addWrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            addWrap.Controls.Add(addRow, 0, 0); addWrap.Controls.Add(btnAddItem, 1, 0); addWrap.Controls.Add(btnNewGood, 2, 0);
            pnlAdd.Controls.Add(addWrap);

            /* ── GRID ── */
            dgItems = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(dgItems);
            dgItems.ReadOnly = true; dgItems.AllowUserToAddRows = false;
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code",  HeaderText = "الكود",      Width = 80  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",  HeaderText = "الصنف",      AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit",  HeaderText = "الوحدة",     Width = 70  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty",   HeaderText = "الكمية",     Width = 80  });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "سعر الشراء", Width = 100 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي",   Width = 110 });
            dgItems.Columns.Add(new DataGridViewButtonColumn  { Name = "Del",   HeaderText = "",           Width = 44, Text = "🗑", UseColumnTextForButtonValue = true });
            dgItems.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgItems.Columns["Del"]!.Index)
                { _lines.RemoveAt(e.RowIndex); BuyRefresh(); }
            };

            /* ── FOOTER ── */
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.White };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFooter.Width, 0);
            var fRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(4, 7, 8, 0) };

            lblTotal  = new Label { Text = "الإجمالي: 0.00",  AutoSize = false, Size = new Size(200, 34), Font = AppTheme.FontBold, ForeColor = AppTheme.TextDark,  TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0, 0, 8, 0) };
            var lpLbl = new Label { Text = "المدفوع:", AutoSize = false, Size = new Size(72, 34), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(10, 0, 4, 0) };
            txtPaid   = new TextBox { Size = new Size(120, 28), Font = AppTheme.FontNormal, Text = "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 6, 0) };
            lblRemain = new Label { Text = "",  AutoSize = false, Size = new Size(190, 34), Font = AppTheme.FontBold, ForeColor = AppTheme.Danger, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0) };
            txtPaid.TextChanged += (s, e) => BuyUpdateFooter();

            var btnSave  = UIHelper.MakeButton("💾 حفظ الفاتورة", AppTheme.Accent,  new Size(155, 40), Point.Empty); btnSave.Margin  = new Padding(6, 3, 4, 0); btnSave.Click  += BuySave;
            var btnClear = UIHelper.MakeButton("🔄 تفريغ",         AppTheme.Warning, new Size(100, 40), Point.Empty); btnClear.Margin = new Padding(0, 3, 4, 0); btnClear.Click += (s, e) => { _lines.Clear(); BuyRefresh(); };

            fRow.Controls.AddRange(new Control[] { btnSave, btnClear, lblRemain, txtPaid, lpLbl, lblTotal });
            pnlFooter.Controls.Add(fRow);

            Controls.Add(UIHelper.WrapGrid(dgItems));
            Controls.Add(pnlFooter); Controls.Add(pnlAdd); Controls.Add(pnlHeader);
        }

        private void LoadGoods()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var list = db.Goods.Include(g => g.Unit).OrderBy(g => g.Name)
                .Select(g => new GoodComboItem { Id = g.Id, Code = g.Code, Name = g.Name, SellPrice = (float)g.SellPrice, BuyPrice = (float)g.BuyPrice, UnitName = g.Unit != null ? g.Unit.UnitName : "" })
                .ToList();
            cboGood.DisplayMember = "Label"; cboGood.ValueMember = "Id";
            cboGood.DataSource = list;
        }

        private void LoadCombos()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            cboImporter.DisplayMember = "Name"; cboImporter.ValueMember = "Id";
            cboImporter.DataSource = db.Importers.OrderBy(i => i.Name).ToList();
            cboStore.DisplayMember = "StoreName"; cboStore.ValueMember = "Id";
            cboStore.DataSource = db.Stores.OrderBy(s => s.StoreName).ToList();
            LoadGoods();
        }

        private void BuyGoodChanged(object? sender, EventArgs e)
        {
            if (cboGood.SelectedItem is not GoodComboItem item) return;
            txtPrice.Text = item.BuyPrice.ToString("N2");
            lblUnit.Text  = item.UnitName;
            txtQty.Focus(); txtQty.SelectAll();
        }

        private void BuyAddItem(object? sender, EventArgs e)
        {
            if (cboGood.SelectedItem is not GoodComboItem sel) { UIHelper.ShowError("اختار صنفاً أولاً"); return; }
            if (!float.TryParse(txtQty.Text.Trim(),   out float qty)   || qty   <= 0) { UIHelper.ShowError("الكمية يجب أن تكون أكبر من صفر"); return; }
            if (!float.TryParse(txtPrice.Text.Trim(), out float price) || price <  0) { UIHelper.ShowError("أدخل سعر الشراء"); return; }
            _lines.Add(new BuyLineItem(sel.Id, sel.Code, sel.Name, sel.UnitName, qty, price));
            BuyRefresh(); txtQty.Text = "1";
        }

        private void BuyAddNewGood(object? sender, EventArgs e)
        {
            using var dlg = new QuickAddGoodDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            LoadGoods();
            if (dlg.NewGoodId > 0)
                foreach (GoodComboItem item in cboGood.Items)
                    if (item.Id == dlg.NewGoodId) { cboGood.SelectedItem = item; break; }
        }

        private void BuyRefresh()
        {
            dgItems.Rows.Clear();
            foreach (var ln in _lines)
                dgItems.Rows.Add(ln.Code, ln.Name, ln.Unit, ln.Qty.ToString("N2"), ln.Price.ToString("N2"), ln.LineTotal.ToString("N2"), "🗑");
            BuyUpdateFooter();
        }

        private void BuyUpdateFooter()
        {
            float total  = _lines.Sum(l => l.LineTotal);
            double paid  = double.TryParse(txtPaid.Text, out double p) ? p : 0;
            double remain = total - paid;
            lblTotal.Text  = $"الإجمالي: {total:N2}";
            lblRemain.Text = remain > 0.005 ? $"⚠ متبقي: {remain:N2}" : "✅ مدفوع بالكامل";
            lblRemain.ForeColor = remain > 0.005 ? AppTheme.Danger : AppTheme.Accent;
        }

        private void BuySave(object? sender, EventArgs e)
        {
            if (cboImporter.SelectedItem == null) { UIHelper.ShowError("اختار المورد"); return; }
            if (!_lines.Any())                    { UIHelper.ShowError("أضف صنفاً واحداً على الأقل"); return; }

            float  asked  = _lines.Sum(l => l.LineTotal);
            float  paid   = float.TryParse(txtPaid.Text, out float pv) ? pv : 0;
            int    impId  = (int)cboImporter.SelectedValue!;
            int    storeId= cboStore.SelectedIndex >= 0 ? (int)cboStore.SelectedValue! : 1;
            DateTime billDate = dtpDate.Value.Date;
            string code   = string.IsNullOrWhiteSpace(txtCode.Text) ? $"P{DateTime.Now:yyyyMMddHHmm}" : txtCode.Text.Trim();

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            int fyId = MainForm.CurrentFiscalYearId
                ?? db.FiscalYears.Where(f => !f.IsClosed).OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();
            if (fyId == 0) fyId = db.FiscalYears.OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();

            var bill = new BuyBill { ImporterId = impId, Code = code, Date = billDate, Asked = asked, Paid = paid, Notes = txtNotes.Text.Trim(), FiscalYearId = fyId };
            db.BuyBills.Add(bill); db.SaveChanges();

            if (paid > 0)
            {
                var iname = db.Importers.Find(impId)?.Name ?? "";
                db.BoxTransactions.Add(new BoxTransaction { Out = true, Value = paid, Date = billDate, Time = DateTime.Now, Notes = $"دفعة فاتورة مشتريات {code} - {iname}", CustName = iname, BuyBillId = bill.Id, FiscalYearId = fyId, No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1 });
            }

            foreach (var ln in _lines)
            {
                var good = db.Goods.Find(ln.GoodId);
                if (good == null) continue;
                db.ImporterInstallments.Add(new ImporterInstallment { BillId = bill.Id, ImporterId = impId, Date = billDate, GoodId = ln.GoodId, Quantity = ln.Qty, Price = ln.Price, Total = ln.LineTotal, Pay = 0, StoreId = storeId });
                db.Movements.Add(new Movement { GoodId = ln.GoodId, Quantity = ln.Qty, Date = billDate, Out = false, IsBill = true, BillNo = code, BuyPrice = ln.Price, StoreNo = storeId, ImporterId = impId, FiscalYearId = fyId });
            }
            db.SaveChanges();

            UIHelper.ShowSuccess($"✅ تم حفظ فاتورة المشتريات {code}\nالإجمالي: {asked:N2}   المدفوع: {paid:N2}");
            _lines.Clear(); BuyRefresh(); txtPaid.Text = "0"; txtCode.Text = "";
        }

        private static ComboBox BCbo() => new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 2) };
        private static TextBox  BTxt(string v) => new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = v, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 8, 3) };
    }

    public class SellBillEditForm : Form
    {
        private readonly int _billId;
        private SellBill _bill = null!;

        private ComboBox cboCustomer = null!, cboStore = null!, cboSeller = null!;
        private DateTimePicker dtpDate = null!;
        private TextBox txtNotes = null!, txtDisPercent = null!, txtPaid = null!;
        private DataGridView dgItems = null!;
        private Label lblTotal = null!, lblRemain = null!;

        public SellBillEditForm(int billId)
        {
            _billId = billId;
            BuildUI();
            LoadBill();
        }

        private void BuildUI()
        {
            this.Text = "✏ تعديل فاتورة مبيعات";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(780, 560);
            this.Size = new Size(900, 640);
            this.StartPosition = FormStartPosition.CenterParent;

            // ─── Header ───
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.White, Padding = new Padding(8, 6, 8, 6) };
            pnlHeader.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            var row1 = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 4, 6, 0),
                Margin = new Padding(0)
            };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            var lblCust = new Label { Text = "العميل *:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            cboCustomer = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 2, 8, 2) };
            var lblDate = new Label { Text = "التاريخ:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            dtpDate = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 3, 8, 3) };
            var lblStore = new Label { Text = "المخزن:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            cboStore = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 2, 0, 2) };

            row1.Controls.Add(lblCust, 0, 0); row1.Controls.Add(cboCustomer, 1, 0);
            row1.Controls.Add(lblDate, 2, 0); row1.Controls.Add(dtpDate, 3, 0);
            row1.Controls.Add(lblStore, 4, 0); row1.Controls.Add(cboStore, 5, 0);

            var row2 = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 2, 6, 4),
                Margin = new Padding(0)
            };
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblSeller = new Label { Text = "المندوب:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            cboSeller = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 2, 8, 2) };
            var lblDis = new Label { Text = "خصم %:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            txtDisPercent = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 8, 3) };
            var lblNotes = new Label { Text = "ملاحظات:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
            txtNotes = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };

            row2.Controls.Add(lblSeller, 0, 0); row2.Controls.Add(cboSeller, 1, 0);
            row2.Controls.Add(lblDis, 2, 0); row2.Controls.Add(txtDisPercent, 3, 0);
            row2.Controls.Add(lblNotes, 4, 0); row2.Controls.Add(txtNotes, 5, 0);

            pnlHeader.Controls.Add(row2);
            pnlHeader.Controls.Add(row1);

            // ─── Items Grid ───
            dgItems = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(dgItems);
            dgItems.ReadOnly = false;
            dgItems.AllowUserToAddRows = true;
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoodCode", HeaderText = "كود الصنف", Width = 90 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoodName", HeaderText = "اسم الصنف", Width = 200 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "الكمية", Width = 80 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "السعر", Width = 90 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dis", HeaderText = "خصم%", Width = 70 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", ReadOnly = true, Width = 110 });
            dgItems.CellValueChanged += DgItems_CellValueChanged;

            // ─── Footer ───
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            pnlFooter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFooter.Width, 0);

            var footerFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 6, 6, 0)
            };

            lblTotal = new Label { Text = "الإجمالي: 0.00", AutoSize = false, Size = new Size(170, 32), Font = AppTheme.FontBold, ForeColor = AppTheme.Dark, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0, 0, 4, 0) };
            var lblP = new Label { Text = "المدفوع:", AutoSize = false, Size = new Size(65, 32), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(8, 0, 0, 0) };
            txtPaid = new TextBox { Size = new Size(110, 28), Font = AppTheme.FontNormal, Text = "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 4, 0) };
            lblRemain = new Label { Text = "المتبقي: 0.00", AutoSize = false, Size = new Size(150, 32), Font = AppTheme.FontBold, ForeColor = AppTheme.Danger, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0) };
            txtPaid.TextChanged += (s, e) => UpdateFooter();

            var btnSave = UIHelper.MakeButton("💾 حفظ التعديل", AppTheme.Accent, new Size(150, 40), Point.Empty);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.TextGray, new Size(100, 40), Point.Empty);
            btnSave.Margin = btnCancel.Margin = new Padding(4, 3, 4, 0);
            btnSave.Click += BtnSave_Click;
            btnCancel.DialogResult = DialogResult.Cancel;

            footerFlow.Controls.AddRange(new Control[] { lblTotal, lblP, txtPaid, lblRemain, btnSave, btnCancel });
            pnlFooter.Controls.Add(footerFlow);

            this.Controls.Add(dgItems);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        private void LoadBill()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            _bill = db.SellBills
                .Include(b => b.Customer)
                .Include(b => b.Items).ThenInclude(i => i.Good)
                .FirstOrDefault(b => b.Id == _billId)!;

            if (_bill == null) { this.DialogResult = DialogResult.Cancel; return; }

            this.Text = $"✏ تعديل فاتورة مبيعات رقم {_bill.Code}";

            // ─── Combos ───
            var customers = db.Customers.OrderBy(c => c.Name).ToList();
            cboCustomer.DisplayMember = "Name"; cboCustomer.ValueMember = "Id";
            cboCustomer.DataSource = customers;
            cboCustomer.SelectedValue = _bill.CustomerId;

            var stores = db.Stores.OrderBy(s => s.StoreName).ToList();
            cboStore.DisplayMember = "StoreName"; cboStore.ValueMember = "Id";
            cboStore.DataSource = stores;
            cboStore.SelectedValue = _bill.StoreNo;

            var sellers = db.Traders.OrderBy(t => t.Name).ToList();
            var sellerList = sellers.Select(t => (object)t).ToList();
            sellerList.Insert(0, new Trader { Id = 0, Name = "-- بدون مندوب --" });
            cboSeller.DisplayMember = "Name"; cboSeller.ValueMember = "Id";
            cboSeller.DataSource = sellerList;
            if (_bill.SellerId.HasValue)
                cboSeller.SelectedValue = _bill.SellerId.Value;
            else
                cboSeller.SelectedIndex = 0;

            dtpDate.Value = _bill.Date;
            txtNotes.Text = _bill.Notes ?? "";
            txtDisPercent.Text = _bill.DisPercent.ToString("N2");
            txtPaid.Text = _bill.Paid.ToString("N2");

            dgItems.CellValueChanged -= DgItems_CellValueChanged;
            foreach (var item in _bill.Items)
            {
                dgItems.Rows.Add(
                    item.Good?.Code ?? "",
                    item.Good?.Name ?? "",
                    item.Quantity.ToString("N2"),
                    item.Price.ToString("N2"),
                    item.DisPerItem.ToString("N2"),
                    item.Total.ToString("N2")
                );
            }
            dgItems.CellValueChanged += DgItems_CellValueChanged;

            UpdateFooter();
        }

        private void DgItems_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgItems.Rows[e.RowIndex];

            if (e.ColumnIndex == 0) 
            {
                var code = row.Cells["GoodCode"].Value?.ToString();
                if (!string.IsNullOrEmpty(code))
                {
                    using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                    var good = db.Goods.FirstOrDefault(g => g.Code == code);
                    if (good != null)
                    {
                        row.Cells["GoodName"].Value = good.Name;
                        row.Cells["Price"].Value = good.SellPrice.ToString("N2");
                    }
                }
            }

            float qty = float.TryParse(row.Cells["Qty"].Value?.ToString(), out float q) ? q : 0;
            float price = float.TryParse(row.Cells["Price"].Value?.ToString(), out float p) ? p : 0;
            float dis = float.TryParse(row.Cells["Dis"].Value?.ToString(), out float d) ? d : 0;
            row.Cells["Total"].Value = (qty * price * (1 - dis / 100f)).ToString("N2");
            UpdateFooter();
        }

        private void UpdateFooter()
        {
            double total = 0;
            foreach (DataGridViewRow row in dgItems.Rows)
                if (!row.IsNewRow)
                    total += double.TryParse(row.Cells["Total"].Value?.ToString(), out double t) ? t : 0;

            double paid = double.TryParse(txtPaid.Text, out double p) ? p : 0;
            double remain = total - paid;

            lblTotal.Text = $"الإجمالي: {total:N2}";
            lblRemain.Text = $"المتبقي: {remain:N2}";
            lblRemain.ForeColor = remain > 0 ? AppTheme.Danger : AppTheme.Accent;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboCustomer.SelectedItem == null) { UIHelper.ShowError("اختار العميل"); return; }

            var newItems = new List<(string code, string name, float qty, float price, float dis)>();
            foreach (DataGridViewRow row in dgItems.Rows)
            {
                if (row.IsNewRow) continue;
                string code = row.Cells["GoodCode"].Value?.ToString() ?? "";
                string name = row.Cells["GoodName"].Value?.ToString() ?? "";
                float qty = float.TryParse(row.Cells["Qty"].Value?.ToString(), out float q) ? q : 0;
                float price = float.TryParse(row.Cells["Price"].Value?.ToString(), out float p) ? p : 0;
                float dis = float.TryParse(row.Cells["Dis"].Value?.ToString(), out float d) ? d : 0;
                if (qty > 0 && price > 0) newItems.Add((code, name, qty, price, dis));
            }

            if (!newItems.Any()) { UIHelper.ShowError("أضف صنفاً واحداً على الأقل"); return; }

            double newAsked = newItems.Sum(i => i.qty * i.price * (1 - i.dis / 100f));
            double newPaid = double.TryParse(txtPaid.Text, out double pv) ? pv : 0;
            float disPer = float.TryParse(txtDisPercent.Text, out float dv) ? dv : 0;
            int custId = (int)cboCustomer.SelectedValue!;
            int storeId = cboStore.SelectedIndex >= 0 ? (int)cboStore.SelectedValue! : _bill.StoreNo;
            int? sellerId = cboSeller.SelectedValue is int sv && sv > 0 ? sv : null;
            DateTime billDate = dtpDate.Value.Date;

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            // Get current fiscal year from the existing bill
            int fiscalYearId = _bill.FiscalYearId;

            var oldMovements = db.Movements
                .Where(m => m.BillNo == _bill.Code.ToString() && m.IsBill && m.Out)
                .ToList();
            db.Movements.RemoveRange(oldMovements);

            var oldBox = db.BoxTransactions.FirstOrDefault(b => b.SellBillId == _bill.Id);
            if (newPaid > 0)
            {
                var custName = db.Customers.Find(custId)?.Name ?? "";
                if (oldBox != null)
                {
                    oldBox.Value = newPaid;
                    oldBox.Date = billDate;
                    oldBox.Notes = $"تحصيل فاتورة مبيعات رقم {_bill.Code} - {custName}";
                    oldBox.CustName = custName;
                }
                else
                {
                    db.BoxTransactions.Add(new BoxTransaction
                    {
                        Out = false,
                        Value = newPaid,
                        Date = billDate,
                        Time = DateTime.Now,
                        Notes = $"تحصيل فاتورة مبيعات رقم {_bill.Code} - {custName}",
                        CustName = custName,
                        SellBillId = _bill.Id,
                        FiscalYearId = fiscalYearId,
                        No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1
                    });
                }
            }
            else if (oldBox != null)
            {
                db.BoxTransactions.Remove(oldBox);
            }

            var oldInstallments = db.CustomerInstallments
                .Where(ci => ci.BillId == _bill.Id)
                .ToList();
            db.CustomerInstallments.RemoveRange(oldInstallments);

            var bill = db.SellBills.Find(_bill.Id)!;
            bill.CustomerId = custId;
            bill.Date = billDate;
            bill.Time = DateTime.Now;
            bill.Asked = newAsked;
            bill.Paid = newPaid;
            bill.Notes = txtNotes.Text.Trim();
            bill.DisPercent = disPer;
            bill.StoreNo = storeId;
            bill.SellerId = sellerId;

            foreach (var (code, name, qty, price, dis) in newItems)
            {
                var good = db.Goods.FirstOrDefault(g => g.Code == code);
                if (good == null) continue;

                db.CustomerInstallments.Add(new CustomerInstallment
                {
                    BillId = _bill.Id,
                    CustomerId = custId,
                    Date = billDate,
                    GoodId = good.Id,
                    Quantity = qty,
                    Price = price,
                    DisPerItem = dis,
                    Total = qty * price * (1 - dis / 100f),
                    Pay = 0,
                    StoreId = storeId,
                    BuyPrice = good.BuyPrice
                });

                db.Movements.Add(new Movement
                {
                    GoodId = good.Id,
                    Quantity = qty,
                    Date = billDate,
                    Out = true,
                    IsBill = true,
                    BillNo = _bill.Code.ToString(),
                    SellPrice = price,
                    BuyPrice = good.BuyPrice,
                    StoreNo = storeId,
                    CustomerId = custId,
                    FiscalYearId = fiscalYearId
                });
            }

            db.SaveChanges();

            UIHelper.ShowSuccess($"✅ تم حفظ التعديل على فاتورة رقم {_bill.Code}\nالإجمالي: {newAsked:N2}   المدفوع: {newPaid:N2}");
            this.DialogResult = DialogResult.OK;
        }
    }

    // ══════════════════════ BUY BILL EDIT ══════════════════════
    public class BuyBillEditForm : Form
    {
        private readonly int _billId;
        private BuyBill _bill = null!;

        private ComboBox cboImporter = null!, cboStore = null!;
        private TextBox txtDate = null!, txtNotes = null!, txtCode = null!, txtPaid = null!;
        private DataGridView dgItems = null!;
        private Label lblTotal = null!, lblRemain = null!;

        public BuyBillEditForm(int billId)
        {
            _billId = billId;
            BuildUI();
            LoadBill();
        }

        private void BuildUI()
        {
            this.Text = "✏ تعديل فاتورة مشتريات";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.MinimumSize = new Size(780, 520);
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.White, Padding = new Padding(8, 6, 8, 6) };
            pnlHeader.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            var row1 = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 4, 6, 0)
            };
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

            cboImporter = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 2) };
            txtDate = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 8, 3) };
            txtCode = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };

            row1.Controls.Add(new Label { Text = "المورد *:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            row1.Controls.Add(cboImporter, 1, 0);
            row1.Controls.Add(new Label { Text = "التاريخ:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            row1.Controls.Add(txtDate, 3, 0);
            row1.Controls.Add(new Label { Text = "رقم الفاتورة:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 4, 0);
            row1.Controls.Add(txtCode, 5, 0);

            var row2 = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 2, 6, 4)
            };
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cboStore = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 2) };
            txtNotes = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };

            row2.Controls.Add(new Label { Text = "المخزن:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            row2.Controls.Add(cboStore, 1, 0);
            row2.Controls.Add(new Label { Text = "ملاحظات:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            row2.Controls.Add(txtNotes, 3, 0);

            pnlHeader.Controls.Add(row2);
            pnlHeader.Controls.Add(row1);

            dgItems = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(dgItems);
            dgItems.ReadOnly = false;
            dgItems.AllowUserToAddRows = true;
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoodCode", HeaderText = "كود الصنف", Width = 90 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoodName", HeaderText = "اسم الصنف", Width = 200 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "الكمية", Width = 80 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "سعر الشراء", Width = 100 });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", ReadOnly = true, Width = 110 });
            dgItems.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgItems.Rows[e.RowIndex];
                if (e.ColumnIndex == 0)
                {
                    var code = row.Cells["GoodCode"].Value?.ToString();
                    if (!string.IsNullOrEmpty(code))
                    {
                        using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                        var g = db.Goods.FirstOrDefault(x => x.Code == code);
                        if (g != null) { row.Cells["GoodName"].Value = g.Name; row.Cells["Price"].Value = g.BuyPrice.ToString("N2"); }
                    }
                }
                float q = float.TryParse(row.Cells["Qty"].Value?.ToString(), out float qv) ? qv : 0;
                float p = float.TryParse(row.Cells["Price"].Value?.ToString(), out float pv) ? pv : 0;
                row.Cells["Total"].Value = (q * p).ToString("N2");
                UpdateFooter();
            };

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFooter.Width, 0);

            var footerFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(4, 6, 6, 0) };

            lblTotal = new Label { Text = "الإجمالي: 0.00", AutoSize = false, Size = new Size(170, 32), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0, 0, 4, 0) };
            var lblP = new Label { Text = "المدفوع:", AutoSize = false, Size = new Size(65, 32), Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(8, 0, 0, 0) };
            txtPaid = new TextBox { Size = new Size(110, 28), Font = AppTheme.FontNormal, Text = "0", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 4, 0) };
            lblRemain = new Label { Text = "المتبقي: 0.00", AutoSize = false, Size = new Size(150, 32), Font = AppTheme.FontBold, ForeColor = AppTheme.Danger, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(0) };
            txtPaid.TextChanged += (s, e) => UpdateFooter();

            var btnSave = UIHelper.MakeButton("💾 حفظ التعديل", AppTheme.Accent, new Size(150, 40), Point.Empty);
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.TextGray, new Size(100, 40), Point.Empty);
            btnSave.Margin = btnCancel.Margin = new Padding(4, 3, 4, 0);
            btnSave.Click += BtnSave_Click;
            btnCancel.DialogResult = DialogResult.Cancel;

            footerFlow.Controls.AddRange(new Control[] { lblTotal, lblP, txtPaid, lblRemain, btnSave, btnCancel });
            pnlFooter.Controls.Add(footerFlow);

            this.Controls.Add(dgItems);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        private void LoadBill()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _bill = db.BuyBills
                .Include(b => b.Importer)
                .Include(b => b.Items).ThenInclude(i => i.Good)
                .FirstOrDefault(b => b.Id == _billId)!;
            if (_bill == null) { this.DialogResult = DialogResult.Cancel; return; }

            this.Text = $"✏ تعديل فاتورة مشتريات رقم {_bill.Code}";

            cboImporter.DisplayMember = "Name"; cboImporter.ValueMember = "Id";
            cboImporter.DataSource = db.Importers.OrderBy(i => i.Name).ToList();
            cboImporter.SelectedValue = _bill.ImporterId;

            cboStore.DisplayMember = "StoreName"; cboStore.ValueMember = "Id";
            cboStore.DataSource = db.Stores.OrderBy(s => s.StoreName).ToList();

            txtCode.Text = _bill.Code;
            txtDate.Text = _bill.Date.ToString("yyyy/MM/dd");
            txtNotes.Text = _bill.Notes ?? "";
            txtPaid.Text = _bill.Paid.ToString("N2");

            foreach (var item in _bill.Items)
                dgItems.Rows.Add(item.Good?.Code ?? "", item.Good?.Name ?? "",
                    item.Quantity.ToString("N2"), item.Price.ToString("N2"),
                    (item.Quantity * item.Price).ToString("N2"));

            UpdateFooter();
        }

        private void UpdateFooter()
        {
            double total = 0;
            foreach (DataGridViewRow row in dgItems.Rows)
                if (!row.IsNewRow)
                    total += double.TryParse(row.Cells["Total"].Value?.ToString(), out double t) ? t : 0;
            double paid = double.TryParse(txtPaid.Text, out double p) ? p : 0;
            lblTotal.Text = $"الإجمالي: {total:N2}";
            lblRemain.Text = $"المتبقي: {(total - paid):N2}";
            lblRemain.ForeColor = (total - paid) > 0 ? AppTheme.Danger : AppTheme.Accent;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboImporter.SelectedItem == null) { UIHelper.ShowError("اختار المورد"); return; }

            var newItems = new List<(string code, float qty, float price)>();
            foreach (DataGridViewRow row in dgItems.Rows)
            {
                if (row.IsNewRow) continue;
                float qty = float.TryParse(row.Cells["Qty"].Value?.ToString(), out float q) ? q : 0;
                float price = float.TryParse(row.Cells["Price"].Value?.ToString(), out float p) ? p : 0;
                if (qty > 0) newItems.Add((row.Cells["GoodCode"].Value?.ToString() ?? "", qty, price));
            }
            if (!newItems.Any()) { UIHelper.ShowError("أضف صنفاً على الأقل"); return; }

            float newAsked = (float)newItems.Sum(i => i.qty * i.price);
            float newPaid = float.TryParse(txtPaid.Text, out float pv) ? pv : 0;
            int impId = (int)cboImporter.SelectedValue!;
            int storeId = cboStore.SelectedIndex >= 0 ? (int)cboStore.SelectedValue! : 1;
            if (!DateTime.TryParse(txtDate.Text, out DateTime billDate)) billDate = _bill.Date;

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            // Get current fiscal year from the existing bill
            int fiscalYearId = _bill.FiscalYearId;

            // 1. عكس الحركات القديمة
            var oldMov = db.Movements.Where(m => m.BillNo == _bill.Code && !m.Out && m.IsBill).ToList();
            db.Movements.RemoveRange(oldMov);

            // 2. تحديث حركة الخزينة
            var oldBox = db.BoxTransactions.FirstOrDefault(b => b.BuyBillId == _bill.Id);
            var impName = db.Importers.Find(impId)?.Name ?? "";
            if (newPaid > 0)
            {
                if (oldBox != null)
                { oldBox.Value = newPaid; oldBox.Date = billDate; oldBox.Notes = $"دفعة فاتورة مشتريات رقم {_bill.Code} - {impName}"; }
                else
                    db.BoxTransactions.Add(new BoxTransaction
                    {
                        Out = true,
                        Value = newPaid,
                        Date = billDate,
                        Time = DateTime.Now,
                        Notes = $"دفعة فاتورة مشتريات رقم {_bill.Code} - {impName}",
                        CustName = impName,
                        BuyBillId = _bill.Id,
                        FiscalYearId = fiscalYearId,
                        No = (db.BoxTransactions.Max(b => (int?)b.No) ?? 0) + 1
                    });
            }
            else if (oldBox != null)
                db.BoxTransactions.Remove(oldBox);

            // 3. حذف أصناف الفاتورة القديمة
            db.ImporterInstallments.RemoveRange(db.ImporterInstallments.Where(ii => ii.BillId == _bill.Id).ToList());

            // 4. تحديث رأس الفاتورة
            var bill = db.BuyBills.Find(_bill.Id)!;
            bill.ImporterId = impId; bill.Code = txtCode.Text.Trim();
            bill.Date = billDate; bill.Asked = newAsked; bill.Paid = newPaid;
            bill.Notes = txtNotes.Text.Trim();

            // 5. أصناف وحركات جديدة
            foreach (var (code, qty, price) in newItems)
            {
                var good = db.Goods.FirstOrDefault(g => g.Code == code);
                if (good == null) continue;
                db.ImporterInstallments.Add(new ImporterInstallment
                {
                    BillId = _bill.Id,
                    ImporterId = impId,
                    Date = billDate,
                    GoodId = good.Id,
                    Quantity = qty,
                    Price = price,
                    Total = qty * price,
                    Pay = 0,
                    StoreId = storeId
                });
                db.Movements.Add(new Movement
                {
                    GoodId = good.Id,
                    Quantity = qty,
                    Date = billDate,
                    Out = false,
                    IsBill = true,
                    BillNo = bill.Code,
                    BuyPrice = price,
                    StoreNo = storeId,
                    ImporterId = impId,
                    FiscalYearId = fiscalYearId
                });

                // تحديث متوسط سعر الشراء (اقتراح #17)
                good.BuyAverage = good.BuyAverage == 0
                    ? price
                    : (good.BuyAverage + price) / 2f;
                good.LastBuy = billDate;
                good.LastBuyValue = qty;
            }

            db.SaveChanges();
            UIHelper.ShowSuccess($"✅ تم حفظ التعديل على فاتورة رقم {bill.Code}\nالإجمالي: {newAsked:N2}   المدفوع: {newPaid:N2}");
            this.DialogResult = DialogResult.OK;
        }
    }

    // ── Shared helpers used by SellBillForm and BuyBillForm ──
    // ══════════════════════ QUICK ADD GOOD DIALOG ══════════════════════
    /// <summary>
    /// Dialog بسيط لإضافة صنف جديد من داخل الفاتورة.
    /// بيتضاف الصنف في جدول Goods مباشرةً، وبتقدري تعدليه بعدين من صفحة الأصناف.
    /// </summary>
    internal class QuickAddGoodDialog : Form
    {
        public int NewGoodId { get; private set; }

        private TextBox txtName = null!, txtCode = null!, txtSell = null!, txtBuy = null!;

        public QuickAddGoodDialog()
        {
            Text = "➕ إضافة صنف جديد";
            Size = new Size(420, 270);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White;
            RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(14, 12, 14, 8), BackColor = Color.White };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtName = TB(); txtCode = TB($"G{DateTime.Now:yyMMddHHmm}");
            txtSell = TB("0"); txtBuy  = TB("0");

            tbl.Controls.Add(L("الاسم *:"),      0, 0); tbl.Controls.Add(txtName, 1, 0);
            tbl.Controls.Add(L("الكود:"),         0, 1); tbl.Controls.Add(txtCode, 1, 1);
            tbl.Controls.Add(L("سعر البيع:"),     0, 2); tbl.Controls.Add(txtSell, 1, 2);
            tbl.Controls.Add(L("سعر الشراء:"),    0, 3); tbl.Controls.Add(txtBuy,  1, 3);

            var note = new Label { Text = "💡 يمكنك تعديل باقي بيانات الصنف لاحقاً من صفحة الأصناف", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f), ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };
            tbl.Controls.Add(note, 0, 4); tbl.SetColumnSpan(note, 2);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            var btnOk  = UIHelper.MakeButton("✔ إضافة", AppTheme.Accent, new Size(120, 36), Point.Empty); btnOk.Margin  = new Padding(0, 4, 0, 0); btnOk.Click  += Save;
            var btnNo  = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(90,  36), Point.Empty); btnNo.Margin  = new Padding(6, 4, 0, 0); btnNo.DialogResult = DialogResult.Cancel;
            btnFlow.Controls.AddRange(new Control[] { btnOk, btnNo });
            tbl.Controls.Add(btnFlow, 0, 5); tbl.SetColumnSpan(btnFlow, 2);

            Controls.Add(tbl);
            AcceptButton = btnOk; CancelButton = btnNo;
            txtName.Focus();
        }

        private void Save(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { UIHelper.ShowError("اسم الصنف مطلوب"); return; }
            float.TryParse(txtSell.Text, out float sell);
            float.TryParse(txtBuy.Text,  out float buy);

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            // تأكد من عدم تكرار الكود
            string code = txtCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code)) code = $"G{DateTime.Now:yyMMddHHmmss}";
            while (db.Goods.Any(g => g.Code == code)) code += "_1";

            var store = db.Stores.FirstOrDefault();
            var good  = new Good { Name = txtName.Text.Trim(), Code = code, SellPrice = sell, BuyPrice = buy, DayOfRegister = DateTime.Now, StoreId = store?.Id };
            db.Goods.Add(good);
            db.SaveChanges();
            NewGoodId = good.Id;
            UIHelper.ShowSuccess($"✅ تم إضافة الصنف \"{good.Name}\"\nيمكنك تعديل بياناته من صفحة الأصناف");
            DialogResult = DialogResult.OK;
        }

        private static TextBox TB(string v = "") => new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = v, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 0, 4) };
        private static Label   L(string t)       => new Label   { Text = t, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight };
    }

    internal static class BillHelper
    {
        public static Label Lbl(string text) =>
            new Label { Text = text, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, AutoSize = false };

        public static TableLayoutPanel MakeRow(int cols) =>
            new TableLayoutPanel { Dock = DockStyle.Top, Height = 36, ColumnCount = cols, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(6, 2, 6, 2), Margin = new Padding(0) };
    }
}
