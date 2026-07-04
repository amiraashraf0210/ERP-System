using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    // ══════════════════════ PRODUCTION ORDERS LIST ══════════════════════
    public class ProductionOrdersForm : Form
    {
        private DataGridView grid = null!;
        private ComboBox cboStatus = null!;
        private TextBox txtSearch = null!;
        private Label lblSummary = null!;
        private List<ProductionOrder> _all = new();

        public ProductionOrdersForm()
        {
            Text = "أوامر الإنتاج";
            BackColor = AppTheme.Light;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            BuildUI();
        }

        private void BuildUI()
        {
            // Filter bar
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            pnlFilter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(8, 8, 8, 8) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            cboStatus = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 8, 0) };
            cboStatus.Items.AddRange(new[] { "الكل", "تحت التشغيل", "تم", "تم التسليم" });
            cboStatus.SelectedIndex = 0;

            txtSearch = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, PlaceholderText = "🔍 رقم / عميل / موديل", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 8, 0) };
            txtSearch.TextChanged += (s, e) => FilterGrid();

            var btnNew = UIHelper.MakeButton("➕ أمر جديد", AppTheme.Accent, new Size(90, 36), Point.Empty);
            btnNew.Dock = DockStyle.Fill; btnNew.Click += (s, e) => OpenEdit(null);

            tbl.Controls.Add(new Label { Text = "الحالة:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            tbl.Controls.Add(cboStatus, 1, 0);
            tbl.Controls.Add(new Label { Text = "بحث:", Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            tbl.Controls.Add(txtSearch, 3, 0);
            tbl.Controls.Add(btnNew, 4, 0);
            pnlFilter.Controls.Add(tbl);

            // Summary bar
            var pnlSum = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(239, 246, 255) };
            lblSummary = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };
            pnlSum.Controls.Add(lblSummary);

            // Action buttons
            var pnlBtns = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };
            pnlBtns.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlBtns.Height - 1, pnlBtns.Width, pnlBtns.Height - 1);
            var bflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(8, 6, 8, 0) };
            var btnEdit   = UIHelper.MakeButton("✏ فتح / تعديل", AppTheme.Primary, new Size(130, 34), Point.Empty); btnEdit.Margin   = new Padding(0, 0, 8, 0); btnEdit.Click   += (s, e) => OpenEdit(GetSelectedId());
            var btnDelete = UIHelper.MakeButton("🗑 حذف",          AppTheme.Danger,  new Size(90,  34), Point.Empty); btnDelete.Margin = new Padding(0, 0, 8, 0); btnDelete.Click += (s, e) => DeleteOrder();
            var btnRefresh= UIHelper.MakeButton("🔄 تحديث",        AppTheme.TextGray,new Size(90,  34), Point.Empty); btnRefresh.Margin= new Padding(0, 0, 8, 0); btnRefresh.Click+= (s, e) => LoadData();
            bflow.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnRefresh });
            pnlBtns.Controls.Add(bflow);

            // Grid
            grid = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(grid);
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OpenEdit(GetSelectedId()); };
            grid.CellFormatting  += GridFormatting;
            cboStatus.SelectedIndexChanged += (s, e) => FilterGrid();

            Controls.Add(UIHelper.WrapGrid(grid));
            Controls.Add(pnlSum);
            Controls.Add(pnlBtns);
            Controls.Add(pnlFilter);
        }

        private void GridFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "الحالة" || e.Value == null) return;
            var v = e.Value.ToString()!;
            if (v.Contains("التشغيل")) { e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14); }
            else if (v.Contains("تم التسليم")) { e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61); }
            else if (v.Contains("تم")) { e.CellStyle.BackColor = Color.FromArgb(240, 228, 210); e.CellStyle.ForeColor = AppTheme.TextDark; }
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); LoadData(); }

        private void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.ProductionOrders.Include(o => o.Customer).Include(o => o.Materials).ThenInclude(m => m.RawMaterial)
                .OrderByDescending(o => o.OrderDate).ToList();
            FilterGrid();
        }

        private void FilterGrid()
        {
            var status = cboStatus.SelectedIndex switch { 1 => "InProgress", 2 => "Done", 3 => "Delivered", _ => "" };
            var k = txtSearch.Text.Trim();
            var list = _all
                .Where(o => string.IsNullOrEmpty(status) || o.Status == status)
                .Where(o => string.IsNullOrWhiteSpace(k) ||
                    o.Code.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    (o.Customer?.Name ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    (o.ModelName ?? "").Contains(k, StringComparison.OrdinalIgnoreCase))
                .ToList();

            grid.DataSource = list.Select(o => new
            {
                Id            = o.Id,
                الكود         = o.Code,
                العميل        = o.Customer?.Name ?? "—",
                الموديل       = o.ModelName ?? "—",
                الكمية_المطلوبة = o.RequestedQty.HasValue ? o.RequestedQty.Value.ToString("N0") : "—",
                تاريخ_الإنتاج = o.OrderDate.ToString("yyyy/MM/dd"),
                تاريخ_التسليم = o.DeliveryDate.HasValue ? o.DeliveryDate.Value.ToString("yyyy/MM/dd") : "—",
                الوزن_المنتج  = o.ProducedWeight > 0 ? o.ProducedWeight.ToString("N2") + " كجم" : "—",
                الهالك        = o.WasteWeight > 0 ? o.WasteWeight.ToString("N2") + " كجم" : "—",
                المواد        = string.Join("، ", o.Materials.Select(m => m.RawMaterial?.Name ?? "")),
                الحالة        = o.Status switch { "InProgress" => "⚙ تحت التشغيل", "Done" => "✅ تم", "Delivered" => "🚚 تم التسليم", _ => o.Status }
            }).ToList();

            double totProd  = list.Sum(o => o.ProducedWeight);
            double totWaste = list.Sum(o => o.WasteWeight);
            lblSummary.Text = $"الأوامر: {list.Count}    |    إجمالي الإنتاج: {totProd:N2} كجم    |    إجمالي الهالك: {totWaste:N2} كجم";
        }

        private int? GetSelectedId()
        {
            if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("اختار أمر إنتاج أولاً"); return null; }
            var row = grid.SelectedRows[0];
            return grid.Columns.Contains("Id") && row.Cells["Id"].Value is int id ? id : null;
        }

        private void OpenEdit(int? id)
        {
            using var f = new ProductionOrderEditForm(id);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void DeleteOrder()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف أمر الإنتاج؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var o = db.ProductionOrders.Find(id);
            if (o == null) return;
            db.ProductionOrders.Remove(o);
            db.SaveChanges();
            LoadData();
        }
    }

    // ══════════════════════ PRODUCTION ORDER EDIT FORM ══════════════════════
    public class ProductionOrderEditForm : Form
    {
        private readonly int? _id;
        private TextBox txtCode = null!, txtModel = null!, txtReqQty = null!, txtProdWeight = null!, txtWaste = null!, txtNotes = null!;
        private ComboBox cboCustomer = null!, cboStatus = null!;
        private DateTimePicker dtpOrder = null!, dtpDelivery = null!;
        private CheckedListBox lstMaterials = null!;
        private DataGridView dgMaterials = null!;

        public ProductionOrderEditForm(int? id)
        {
            _id = id;
            Text = id == null ? "➕ أمر إنتاج جديد" : "✏ تعديل أمر إنتاج";
            Size = new Size(820, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            BuildUI();
            LoadCombos();
            if (id != null) LoadOrder(id.Value);
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Color.White, Padding = new Padding(12, 8, 12, 4) };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);

            var r1 = MakeRow(6);
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            txtCode = F(""); cboCustomer = C(); cboStatus = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 2) };
            cboStatus.Items.AddRange(new[] { "InProgress", "Done", "Delivered" }); cboStatus.SelectedIndex = 0;
            r1.Controls.Add(L("رقم الأمر *:"), 0, 0); r1.Controls.Add(txtCode,      1, 0);
            r1.Controls.Add(L("العميل:"),       2, 0); r1.Controls.Add(cboCustomer,  3, 0);
            r1.Controls.Add(L("الحالة:"),       4, 0); r1.Controls.Add(cboStatus,    5, 0);

            var r2 = MakeRow(8);
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            txtModel = F(""); txtReqQty = F(""); txtProdWeight = F("0"); txtWaste = F("0");
            r2.Controls.Add(L("الموديل:"),           0, 0); r2.Controls.Add(txtModel,      1, 0);
            r2.Controls.Add(L("الكمية المطلوبة:"),   2, 0); r2.Controls.Add(txtReqQty,     3, 0);
            r2.Controls.Add(L("الوزن المنتج كجم:"),  4, 0); r2.Controls.Add(txtProdWeight, 5, 0);
            r2.Controls.Add(L("الهالك كجم:"),        6, 0); r2.Controls.Add(txtWaste,      7, 0);

            var r3 = MakeRow(6);
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            dtpOrder = DTP(); dtpDelivery = DTP(); dtpDelivery.Value = DateTime.Today.AddDays(7); txtNotes = F("");
            r3.Controls.Add(L("تاريخ الإنتاج:"), 0, 0); r3.Controls.Add(dtpOrder,    1, 0);
            r3.Controls.Add(L("تاريخ التسليم:"), 2, 0); r3.Controls.Add(dtpDelivery, 3, 0);
            r3.Controls.Add(L("ملاحظات:"),        4, 0); r3.Controls.Add(txtNotes,    5, 0);

            pnlTop.Controls.Add(r3); pnlTop.Controls.Add(r2); pnlTop.Controls.Add(r1);

            // ── Materials checklist ──
            var pnlMid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White, Padding = new Padding(12, 6, 12, 4) };
            pnlMid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            pnlMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Left: checklist
            var pnlLeft = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var lblMat = new Label { Text = "☑ المواد الخام المستخدمة", Dock = DockStyle.Top, Height = 28, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight };
            var lblHint = new Label { Text = "اختاري المواد المستخدمة في هذا الأمر", Dock = DockStyle.Top, Height = 20, Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };
            lstMaterials = new CheckedListBox
            {
                Dock = DockStyle.Fill, Font = AppTheme.FontNormal, CheckOnClick = true,
                RightToLeft = RightToLeft.Yes, BackColor = Color.FromArgb(248, 250, 252), BorderStyle = BorderStyle.FixedSingle
            };
            pnlLeft.Controls.Add(lstMaterials);
            pnlLeft.Controls.Add(lblHint);
            pnlLeft.Controls.Add(lblMat);

            // Right: quantities grid
            var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8, 0, 0, 0) };
            var lblQty = new Label { Text = "الكميات المستخدمة (اختياري)", Dock = DockStyle.Top, Height = 28, Font = AppTheme.FontBold, ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };
            var lblQtyHint = new Label { Text = "يمكن تركها فارغة لو مش هتحسبي الكميات", Dock = DockStyle.Top, Height = 20, Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleRight };
            dgMaterials = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(dgMaterials);
            dgMaterials.ReadOnly = false; dgMaterials.AllowUserToAddRows = false;
            dgMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatId",    Visible = false });
            dgMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatName",  HeaderText = "المادة",             ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatQty",   HeaderText = "الكمية (اختياري)",   Width = 140 });
            dgMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatNotes", HeaderText = "ملاحظة",             Width = 120 });
            dgMaterials.CurrentCellDirtyStateChanged += (s, e) => { if (dgMaterials.IsCurrentCellDirty) dgMaterials.CommitEdit(DataGridViewDataErrorContexts.Commit); };

            lstMaterials.ItemCheck += (s, e) =>
            {
                var mat = (RawMaterial)lstMaterials.Items[e.Index];
                if (e.NewValue == CheckState.Checked)
                    dgMaterials.Rows.Add(mat.Id, mat.Name, "", "");
                else
                    foreach (DataGridViewRow row in dgMaterials.Rows)
                        if (row.Cells["MatId"].Value is int rid && rid == mat.Id) { dgMaterials.Rows.Remove(row); break; }
            };

            pnlRight.Controls.Add(dgMaterials); pnlRight.Controls.Add(lblQtyHint); pnlRight.Controls.Add(lblQty);
            pnlMid.Controls.Add(pnlLeft, 0, 0); pnlMid.Controls.Add(pnlRight, 1, 0);

            // ── Footer ──
            var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Color.White };
            pnlFoot.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFoot.Width, 0);
            var fflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(8, 8, 8, 0) };
            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(120, 36), Point.Empty); btnSave.Margin   = new Padding(0, 0, 8, 0); btnSave.Click   += Save;
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(100, 36), Point.Empty); btnCancel.Margin = new Padding(0, 0, 8, 0); btnCancel.DialogResult = DialogResult.Cancel;
            fflow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            pnlFoot.Controls.Add(fflow);

            Controls.Add(pnlMid); Controls.Add(pnlFoot); Controls.Add(pnlTop);
            AcceptButton = btnSave; CancelButton = btnCancel;
        }

        private void LoadCombos()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            var custs = db.Customers.OrderBy(c => c.Name).ToList<object>();
            custs.Insert(0, new Customer { Id = 0, Name = "-- بدون عميل --" });
            cboCustomer.DisplayMember = "Name"; cboCustomer.ValueMember = "Id";
            cboCustomer.DataSource = custs;

            // Load raw materials into checklist
            var mats = db.RawMaterials.OrderBy(m => m.Name).ToList();
            lstMaterials.Items.Clear();
            foreach (var m in mats)
                lstMaterials.Items.Add(m, false);
        }

        private void LoadOrder(int id)
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var o = db.ProductionOrders.Include(x => x.Materials).ThenInclude(m => m.RawMaterial).FirstOrDefault(x => x.Id == id);
            if (o == null) return;

            txtCode.Text       = o.Code;
            txtModel.Text      = o.ModelName ?? "";
            txtReqQty.Text     = o.RequestedQty.HasValue ? o.RequestedQty.Value.ToString() : "";
            dtpOrder.Value     = o.OrderDate;
            dtpDelivery.Value  = o.DeliveryDate ?? DateTime.Today.AddDays(7);
            txtProdWeight.Text = o.ProducedWeight.ToString("N2");
            txtWaste.Text      = o.WasteWeight.ToString("N2");
            txtNotes.Text      = o.Notes ?? "";
            cboStatus.SelectedItem = o.Status;
            if (o.CustomerId.HasValue) cboCustomer.SelectedValue = o.CustomerId.Value;

            // Check used materials and fill grid
            var usedIds = o.Materials.Select(m => m.RawMaterialId).ToHashSet();
            for (int i = 0; i < lstMaterials.Items.Count; i++)
            {
                var mat = (RawMaterial)lstMaterials.Items[i];
                if (usedIds.Contains(mat.Id))
                {
                    lstMaterials.SetItemChecked(i, true);
                    var pm = o.Materials.First(m => m.RawMaterialId == mat.Id);
                    dgMaterials.Rows.Add(mat.Id, mat.Name, pm.Quantity.HasValue ? pm.Quantity.Value.ToString() : "", pm.Notes ?? "");
                }
            }
        }

        private void Save(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text)) { UIHelper.ShowError("رقم الأمر مطلوب"); return; }

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            var o = _id == null ? new ProductionOrder() : db.ProductionOrders.Include(x => x.Materials).First(x => x.Id == _id);

            o.Code          = txtCode.Text.Trim();
            o.ModelName     = string.IsNullOrWhiteSpace(txtModel.Text) ? null : txtModel.Text.Trim();
            o.RequestedQty  = double.TryParse(txtReqQty.Text, out double rq) && rq > 0 ? rq : null;
            o.OrderDate     = dtpOrder.Value.Date;
            o.DeliveryDate  = dtpDelivery.Value.Date;
            o.ProducedWeight= double.TryParse(txtProdWeight.Text, out double pw) ? pw : 0;
            o.WasteWeight   = double.TryParse(txtWaste.Text,      out double ww) ? ww : 0;
            o.Notes         = txtNotes.Text.Trim();
            o.Status        = cboStatus.SelectedItem?.ToString() ?? "InProgress";
            o.CustomerId    = cboCustomer.SelectedItem is Customer c && c.Id > 0 ? c.Id : null;

            if (_id == null) db.ProductionOrders.Add(o);
            else o.Materials.Clear();
            db.SaveChanges();

            // Save materials from grid
            foreach (DataGridViewRow row in dgMaterials.Rows)
            {
                if (row.Cells["MatId"].Value is not int matId) continue;
                double? qty = double.TryParse(row.Cells["MatQty"].Value?.ToString(), out double q) && q > 0 ? q : null;
                string? notes = row.Cells["MatNotes"].Value?.ToString();
                db.ProductionMaterials.Add(new ProductionMaterial { ProductionOrderId = o.Id, RawMaterialId = matId, Quantity = qty, Notes = notes });
            }
            db.SaveChanges();

            UIHelper.ShowSuccess($"✅ تم حفظ أمر الإنتاج {o.Code}");
            DialogResult = DialogResult.OK;
        }

        // helpers
        private static Label L(string t) => new Label { Text = t, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, AutoSize = false };
        private static TextBox F(string v) => new TextBox { Dock = DockStyle.Fill, Text = v, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 2, 8, 2) };
        private static ComboBox C() => new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 2) };
        private static DateTimePicker DTP() => new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = AppTheme.FontNormal, Margin = new Padding(0, 2, 8, 2) };
        private static TableLayoutPanel MakeRow(int cols) => new TableLayoutPanel { Dock = DockStyle.Top, Height = 36, ColumnCount = cols, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0, 2, 0, 2), Margin = new Padding(0) };
    }

    // ══════════════════════ RAW MATERIALS FORM (stub — existing) ══════════════════════
    public class RawMaterialsForm : BaseListForm
    {
        private List<RawMaterial> _all = new();
        public RawMaterialsForm() : base("المواد الخام") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.RawMaterials.Include(r => r.Unit).OrderBy(r => r.Name).ToList();
            BindGrid(_all);
        }

        private void BindGrid(List<RawMaterial> list) =>
            grid.DataSource = list.Select(r => new
            {
                Id = r.Id,
                الكود = r.Code,
                الاسم = r.Name,
                الوحدة = r.Unit?.UnitName ?? "",
                سعر_الشراء = r.BuyPrice.ToString("N2"),
                الرصيد_الحالي = r.CurrentStock.ToString("N2"),
                الحد_الأدنى = r.MinStock,
                آخر_شراء = r.LastPurchase.HasValue ? r.LastPurchase.Value.ToString("yyyy/MM/dd") : ""
            }).ToList();

        protected override void OnAdd()
        {
            using var dlg = new RawMaterialEditForm(null);
            if (dlg.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var dlg = new RawMaterialEditForm(id);
            if (dlg.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه المادة الخام؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var r = db.RawMaterials.Find(id);
            if (r != null) { db.RawMaterials.Remove(r); db.SaveChanges(); LoadData(); }
        }

        protected override void OnSearch(string k) =>
            BindGrid(string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(r => r.Name.Contains(k, StringComparison.OrdinalIgnoreCase) || r.Code.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    // ══════════════════════ RAW MATERIAL EDIT ══════════════════════
    internal class RawMaterialEditForm : Form
    {
        private readonly int? _id;
        private TextBox txtCode = null!, txtName = null!, txtPrice = null!, txtStock = null!, txtMin = null!, txtNotes = null!;
        private ComboBox cboUnit = null!;

        public RawMaterialEditForm(int? id)
        {
            _id = id;
            Text = id == null ? "➕ إضافة مادة خام" : "✏ تعديل مادة خام";
            Size = new Size(400, 330); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.White; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
            BuildUI(); LoadCombos();
            if (id != null) LoadData(id.Value);
        }

        private void BuildUI()
        {
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(12), BackColor = Color.White };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtCode = TB(); txtName = TB(); cboUnit = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0,3,0,3) };
            txtPrice = TB("0"); txtStock = TB("0"); txtMin = TB("0"); txtNotes = TB();

            tbl.Controls.Add(BillHelper.Lbl("الكود *:"),       0, 0); tbl.Controls.Add(txtCode,  1, 0);
            tbl.Controls.Add(BillHelper.Lbl("الاسم *:"),       0, 1); tbl.Controls.Add(txtName,  1, 1);
            tbl.Controls.Add(BillHelper.Lbl("الوحدة:"),        0, 2); tbl.Controls.Add(cboUnit,  1, 2);
            tbl.Controls.Add(BillHelper.Lbl("سعر الشراء:"),    0, 3); tbl.Controls.Add(txtPrice, 1, 3);
            tbl.Controls.Add(BillHelper.Lbl("الرصيد الحالي:"), 0, 4); tbl.Controls.Add(txtStock, 1, 4);
            tbl.Controls.Add(BillHelper.Lbl("الحد الأدنى:"),   0, 5); tbl.Controls.Add(txtMin,   1, 5);
            tbl.Controls.Add(BillHelper.Lbl("ملاحظات:"),       0, 6); tbl.Controls.Add(txtNotes, 1, 6);

            var bflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            var btnOk = UIHelper.MakeButton("💾 حفظ", AppTheme.Accent, new Size(110, 34), Point.Empty); btnOk.Margin = new Padding(0, 4, 0, 0); btnOk.Click += Save;
            var btnNo = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(90, 34), Point.Empty); btnNo.Margin = new Padding(6, 4, 0, 0); btnNo.DialogResult = DialogResult.Cancel;
            bflow.Controls.AddRange(new Control[] { btnOk, btnNo });
            tbl.Controls.Add(new Label(), 0, 7); tbl.Controls.Add(bflow, 1, 7);

            Controls.Add(tbl); AcceptButton = btnOk; CancelButton = btnNo;
        }

        private void LoadCombos()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            cboUnit.DisplayMember = "UnitName"; cboUnit.ValueMember = "Id";
            var units = db.Units.OrderBy(u => u.UnitName).ToList<object>();
            units.Insert(0, new Unit { Id = 0, UnitName = "-- بدون --" });
            cboUnit.DataSource = units;
        }

        private void LoadData(int id)
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var r = db.RawMaterials.Find(id); if (r == null) return;
            txtCode.Text = r.Code; txtName.Text = r.Name;
            txtPrice.Text = r.BuyPrice.ToString(); txtStock.Text = r.CurrentStock.ToString();
            txtMin.Text = r.MinStock.ToString(); txtNotes.Text = r.Notes ?? "";
            if (r.UnitId.HasValue) cboUnit.SelectedValue = r.UnitId.Value;
        }

        private void Save(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { UIHelper.ShowError("الاسم مطلوب"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var r = _id == null ? new RawMaterial() : db.RawMaterials.Find(_id)!;
            r.Code = txtCode.Text.Trim(); r.Name = txtName.Text.Trim();
            r.BuyPrice = double.TryParse(txtPrice.Text, out double p) ? p : 0;
            r.CurrentStock = double.TryParse(txtStock.Text, out double s) ? s : 0;
            r.MinStock = int.TryParse(txtMin.Text, out int m) ? m : 0;
            r.Notes = txtNotes.Text.Trim();
            r.UnitId = cboUnit.SelectedItem is Unit u && u.Id > 0 ? u.Id : null;
            if (_id == null) db.RawMaterials.Add(r);
            db.SaveChanges();
            UIHelper.ShowSuccess("تم الحفظ ✅"); DialogResult = DialogResult.OK;
        }

        private static TextBox TB(string v = "") => new TextBox { Dock = DockStyle.Fill, Text = v, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 0, 3) };
    }

    // ══════════════════════ STUBS for unused production pages ══════════════════════
    public class ProductionRecipesForm : BaseListForm
    {
        private List<ProductionRecipe> _all = new();
        public ProductionRecipesForm() : base("وصفات الإنتاج") { }

        protected override void LoadData()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            _all = db.ProductionRecipes.Include(r => r.Good).Include(r => r.RecipeItems).ThenInclude(ri => ri.RawMaterial)
                .OrderBy(r => r.Name).ToList();
            grid.DataSource = _all.Select(r => new
            {
                Id            = r.Id,
                الكود         = r.Code,
                الاسم         = r.Name,
                المنتج_النهائي = r.Good?.Name ?? "",
                الكمية_الناتجة = r.OutputQuantity.ToString("N0"),
                المواد        = string.Join("، ", r.RecipeItems.Select(ri => ri.RawMaterial?.Name ?? "")),
                الحالة        = r.IsActive ? "نشطة ✅" : "موقوفة",
                تاريخ_الإنشاء = r.CreatedDate.ToString("yyyy/MM/dd")
            }).ToList();
        }

        protected override void OnAdd()
        {
            using var f = new RecipeEditForm(null);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnEdit()
        {
            var id = GetSelectedId(); if (id == null) return;
            using var f = new RecipeEditForm(id);
            if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        protected override void OnDelete()
        {
            var id = GetSelectedId(); if (id == null) return;
            if (!UIHelper.Confirm("حذف هذه الوصفة؟")) return;
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var r = db.ProductionRecipes.Find(id);
            if (r != null) { db.ProductionRecipes.Remove(r); db.SaveChanges(); LoadData(); }
        }

        protected override void OnSearch(string k) =>
            grid.DataSource = (string.IsNullOrWhiteSpace(k) ? _all :
                _all.Where(r => r.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                    || (r.Good?.Name ?? "").Contains(k, StringComparison.OrdinalIgnoreCase))).Select(r => new
                    { Id = r.Id, الكود = r.Code, الاسم = r.Name, المنتج = r.Good?.Name ?? "" }).ToList();
    }

    // ── Recipe Edit Form ──
    internal class RecipeEditForm : Form
    {
        private readonly int? _id;
        private TextBox txtCode = null!, txtName = null!, txtQty = null!, txtNotes = null!;
        private ComboBox cboGood = null!;
        private CheckBox chkActive = null!;
        private DataGridView dgItems = null!;

        public RecipeEditForm(int? id)
        {
            _id = id;
            Text = id == null ? "➕ وصفة جديدة" : "✏ تعديل وصفة";
            Size = new Size(680, 560); StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            BackColor = Color.White; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
            BuildUI(); LoadCombos();
            if (id != null) LoadRecipe(id.Value);
        }

        private void BuildUI()
        {
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 155, BackColor = Color.White, Padding = new Padding(12, 8, 12, 4) };
            pnlTop.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlTop.Height - 1, pnlTop.Width, pnlTop.Height - 1);

            var r1 = Row(6);
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            r1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            txtCode = TB(); txtName = TB(); txtQty = TB("1");
            r1.Controls.Add(Lb("الكود *:"),  0, 0); r1.Controls.Add(txtCode, 1, 0);
            r1.Controls.Add(Lb("الاسم *:"),  2, 0); r1.Controls.Add(txtName, 3, 0);
            r1.Controls.Add(Lb("الكمية الناتجة:"), 4, 0); r1.Controls.Add(txtQty, 5, 0);

            var r2 = Row(4);
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            r2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cboGood = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0,2,8,2) };
            txtNotes = TB();
            r2.Controls.Add(Lb("المنتج النهائي:"), 0, 0); r2.Controls.Add(cboGood,   1, 0);
            r2.Controls.Add(Lb("ملاحظات:"),        2, 0); r2.Controls.Add(txtNotes, 3, 0);

            var r3 = Row(2);
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            r3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chkActive = new CheckBox { Text = "وصفة نشطة", Checked = true, Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Margin = new Padding(0,5,0,0) };
            r3.Controls.Add(Lb("الحالة:"), 0, 0); r3.Controls.Add(chkActive, 1, 0);

            pnlTop.Controls.Add(r3); pnlTop.Controls.Add(r2); pnlTop.Controls.Add(r1);

            // Items grid
            var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12, 4, 12, 4) };
            var lblItems = new Label { Text = "المواد الخام في الوصفة", Dock = DockStyle.Top, Height = 26, Font = AppTheme.FontBold, ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight };

            var pnlAddItem = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(239,246,255) };
            var addRow = Row(4); addRow.Dock = DockStyle.Fill;
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            addRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            var cboMat = new ComboBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0,2,6,2) };
            var txtMatQty = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Text = "1", BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0,2,6,2) };
            var btnAdd = UIHelper.MakeButton("➕ إضافة", AppTheme.Accent, new Size(85, 32), Point.Empty); btnAdd.Dock = DockStyle.Fill; btnAdd.Margin = new Padding(0,2,0,2);
            addRow.Controls.Add(cboMat, 0, 0); addRow.Controls.Add(Lb("كمية:"), 1, 0); addRow.Controls.Add(txtMatQty, 2, 0); addRow.Controls.Add(btnAdd, 3, 0);
            pnlAddItem.Controls.Add(addRow);

            dgItems = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(dgItems); dgItems.ReadOnly = true; dgItems.AllowUserToAddRows = false;
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatId",  Visible = false });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatName", HeaderText = "المادة الخام", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "MatQty",  HeaderText = "الكمية", Width = 100 });
            dgItems.Columns.Add(new DataGridViewButtonColumn  { Name = "Del",     HeaderText = "", Width = 44, Text = "🗑", UseColumnTextForButtonValue = true });
            dgItems.CellClick += (s, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == dgItems.Columns["Del"]!.Index) dgItems.Rows.RemoveAt(e.RowIndex); };

            btnAdd.Click += (s, e) =>
            {
                if (cboMat.SelectedItem is not RawMaterial mat) { UIHelper.ShowError("اختاري مادة أولاً"); return; }
                if (!double.TryParse(txtMatQty.Text, out double q) || q <= 0) { UIHelper.ShowError("الكمية غير صحيحة"); return; }
                dgItems.Rows.Add(mat.Id, mat.Name, q.ToString("N3"));
                txtMatQty.Text = "1";
            };

            pnlGrid.Controls.Add(UIHelper.WrapGrid(dgItems)); pnlGrid.Controls.Add(pnlAddItem); pnlGrid.Controls.Add(lblItems);

            var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
            pnlFoot.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFoot.Width, 0);
            var fflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent, Padding = new Padding(8,8,8,0) };
            var btnSave   = UIHelper.MakeButton("💾 حفظ",   AppTheme.Accent, new Size(120,36), Point.Empty); btnSave.Margin   = new Padding(0,0,8,0); btnSave.Click += Save;
            var btnCancel = UIHelper.MakeButton("✖ إلغاء", AppTheme.Danger, new Size(100,36), Point.Empty); btnCancel.Margin = new Padding(0,0,8,0); btnCancel.DialogResult = DialogResult.Cancel;
            fflow.Controls.AddRange(new Control[] { btnSave, btnCancel }); pnlFoot.Controls.Add(fflow);

            Controls.Add(pnlGrid); Controls.Add(pnlFoot); Controls.Add(pnlTop);
            AcceptButton = btnSave; CancelButton = btnCancel;

            // load materials combo after controls are ready
            Load += (s, e) =>
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                cboMat.DisplayMember = "Name"; cboMat.ValueMember = "Id";
                cboMat.DataSource = db.RawMaterials.OrderBy(m => m.Name).ToList();
            };
        }

        private void LoadCombos()
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            cboGood.DisplayMember = "Name"; cboGood.ValueMember = "Id";
            var goods = db.Goods.OrderBy(g => g.Name).ToList<object>();
            goods.Insert(0, new Good { Id = 0, Name = "-- اختر المنتج --" });
            cboGood.DataSource = goods;
        }

        private void LoadRecipe(int id)
        {
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var r = db.ProductionRecipes.Include(x => x.RecipeItems).ThenInclude(ri => ri.RawMaterial).FirstOrDefault(x => x.Id == id);
            if (r == null) return;
            txtCode.Text = r.Code; txtName.Text = r.Name;
            txtQty.Text = r.OutputQuantity.ToString(); txtNotes.Text = r.Notes ?? ""; chkActive.Checked = r.IsActive;
            if (r.GoodId > 0) cboGood.SelectedValue = r.GoodId;
            foreach (var item in r.RecipeItems)
                dgItems.Rows.Add(item.RawMaterialId, item.RawMaterial?.Name ?? "", item.Quantity.ToString("N3"));
        }

        private void Save(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text)) { UIHelper.ShowError("الكود والاسم مطلوبان"); return; }
            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var r = _id == null ? new ProductionRecipe() : db.ProductionRecipes.Include(x => x.RecipeItems).First(x => x.Id == _id);
            r.Code = txtCode.Text.Trim(); r.Name = txtName.Text.Trim();
            r.OutputQuantity = double.TryParse(txtQty.Text, out double q) ? q : 1;
            r.Notes = txtNotes.Text.Trim(); r.IsActive = chkActive.Checked;
            r.GoodId = cboGood.SelectedItem is Good g && g.Id > 0 ? g.Id : 0;
            if (_id == null) { r.CreatedDate = DateTime.Now; db.ProductionRecipes.Add(r); db.SaveChanges(); }
            else { r.RecipeItems.Clear(); db.SaveChanges(); }
            foreach (DataGridViewRow row in dgItems.Rows)
                if (row.Cells["MatId"].Value is int matId)
                    db.RecipeItems.Add(new RecipeItem { RecipeId = r.Id, RawMaterialId = matId, Quantity = double.TryParse(row.Cells["MatQty"].Value?.ToString(), out double rq) ? rq : 0 });
            db.SaveChanges();
            UIHelper.ShowSuccess("تم الحفظ ✅"); DialogResult = DialogResult.OK;
        }

        private static TableLayoutPanel Row(int cols) => new TableLayoutPanel { Dock = DockStyle.Top, Height = 36, ColumnCount = cols, RowCount = 1, BackColor = Color.Transparent, Padding = new Padding(0,2,0,2), Margin = new Padding(0) };
        private static Label  Lb(string t) => new Label  { Text = t, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight, AutoSize = false };
        private static TextBox TB(string v = "") => new TextBox { Dock = DockStyle.Fill, Text = v, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0,2,8,2) };
    }

    public class RestockingForm : Form
    {
        public RestockingForm() { Text = "إعادة التخزين"; BackColor = AppTheme.Light; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true; Controls.Add(new Label { Text = "راجع تقرير المخزون لمعرفة الأصناف التي وصلت للحد الأدنى.", Dock = DockStyle.Fill, Font = AppTheme.FontMedium, ForeColor = AppTheme.TextGray, TextAlign = ContentAlignment.MiddleCenter }); }
    }
}
