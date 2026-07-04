using System.Drawing;
using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class MonthlyInventoryForm : Form
    {
        private NumericUpDown numYear = null!;
        private ComboBox cboMonth = null!;
        private DataGridView grid = null!;
        private Button btnLoad = null!, btnSave = null!, btnClose = null!;
        private Label lblStatus = null!, lblSummary = null!;

        public MonthlyInventoryForm()
        {
            Text = "جرد نهاية الشهر";
            BackColor = AppTheme.Light;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            BuildUI();
        }

        private void BuildUI()
        {
            // ── Filter bar ──
            var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White, Padding = new Padding(10, 8, 10, 8) };
            pnlFilter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            numYear = new NumericUpDown { Minimum = 2020, Maximum = 2100, Value = DateTime.Today.Year, Font = AppTheme.FontNormal, Width = 80, Margin = new Padding(0, 2, 12, 0) };
            cboMonth = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontNormal, Width = 90, Margin = new Padding(0, 2, 12, 0) };
            for (int i = 1; i <= 12; i++) cboMonth.Items.Add(i.ToString("D2"));
            cboMonth.SelectedIndex = DateTime.Today.Month - 1;

            btnLoad = UIHelper.MakeButton("🔄 تحميل", AppTheme.Primary, new Size(110, 36), Point.Empty);
            btnLoad.Margin = new Padding(0, 0, 8, 0);
            btnLoad.Click += (s, e) => LoadData();

            lblStatus  = new Label { Text = "", AutoSize = true, Font = AppTheme.FontBold, ForeColor = AppTheme.Accent,   Margin = new Padding(16, 8, 0, 0) };
            lblSummary = new Label { Text = "", AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextGray, Margin = new Padding(8, 10, 0, 0) };

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            flow.Controls.AddRange(new Control[]
            {
                // RightToLeft: أضيف بالعكس عشان يظهر السنة أقصى يمين
                lblSummary,
                lblStatus,
                btnLoad,
                cboMonth,
                new Label { Text = "الشهر:", AutoSize = true, Font = AppTheme.FontBold, Margin = new Padding(0,8,4,0) },
                numYear,
                new Label { Text = "السنة:", AutoSize = true, Font = AppTheme.FontBold, Margin = new Padding(0,8,4,0) }
            });
            pnlFilter.Controls.Add(flow);

            // ── Grid ──
            grid = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleGrid(grid);
            grid.AllowUserToAddRows    = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = false;

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GoodId",      Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code",        HeaderText = "الكود",             Width = 90,  ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",        HeaderText = "الصنف",             AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SystemStock", HeaderText = "الرصيد الدفتري",   Width = 130, ReadOnly = true });

            // ── خانة الجرد الفعلي — قابلة للتعديل فقط ──
            var colActual = new DataGridViewTextBoxColumn
            {
                Name = "ActualStock",
                HeaderText = "✏ الرصيد الفعلي (الجرد)",
                Width = 160,
                ReadOnly = false
            };
            colActual.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 220);
            colActual.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            colActual.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(colActual);

            // عمود الفرق يُحسب تلقائياً
            var colDiff = new DataGridViewTextBoxColumn { Name = "Diff", HeaderText = "الفرق", Width = 100, ReadOnly = true };
            colDiff.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns.Add(colDiff);

            grid.CellValueChanged       += OnCellChanged;
            grid.CurrentCellDirtyStateChanged += (s, e) => { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };

            // ── تلوين صفوف بحسب الفرق ──
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Diff" || e.Value == null) return;
                if (double.TryParse(e.Value.ToString(), out double d))
                {
                    if (d < -0.001)      { e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);  e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); }
                    else if (d > 0.001)  { e.CellStyle.ForeColor = Color.FromArgb(21, 128, 61);  e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); }
                    else                 { e.CellStyle.ForeColor = AppTheme.TextGray; }
                }
            };

            // ── Footer ──
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.White, Padding = new Padding(10, 8, 10, 8) };
            pnlFooter.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, 0, pnlFooter.Width, 0);

            btnSave = UIHelper.MakeButton("💾 حفظ مسودة", AppTheme.Accent, new Size(130, 40), Point.Empty);
            btnSave.Margin = new Padding(0, 0, 10, 0);
            btnSave.Click += (s, e) => Save(false);

            btnClose = UIHelper.MakeButton("🔒 إقفال الشهر وترحيل", AppTheme.Danger, new Size(200, 40), Point.Empty);
            btnClose.Margin = new Padding(0, 0, 10, 0);
            btnClose.Click += (s, e) =>
            {
                int m = cboMonth.SelectedIndex + 1;
                if (UIHelper.Confirm($"إقفال شهر {m}/{numYear.Value}؟\nسيتم تعديل المخزون ليطابق الرصيد الفعلي ولا يمكن التراجع."))
                    Save(true);
            };

            var note = new Label { Text = "💡 الفرق = الفعلي − الدفتري  •  أخضر: زيادة  •  أحمر: عجز", AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = AppTheme.TextGray, Margin = new Padding(20, 12, 0, 0) };

            var fflow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            fflow.Controls.AddRange(new Control[] { btnClose, btnSave, note });
            pnlFooter.Controls.Add(fflow);

            Controls.Add(UIHelper.WrapGrid(grid));
            Controls.Add(pnlFooter);
            Controls.Add(pnlFilter);
        }

        private void OnCellChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "ActualStock") return;
            var row = grid.Rows[e.RowIndex];
            if (!double.TryParse(row.Cells["ActualStock"].Value?.ToString(), out double actual)) return;
            if (!double.TryParse(row.Cells["SystemStock"].Value?.ToString(), out double system)) return;
            double diff = actual - system;
            row.Cells["Diff"].Value = diff.ToString("+0.##;-0.##;0");
        }

        private void LoadData()
        {
            int year  = (int)numYear.Value;
            int month = cboMonth.SelectedIndex + 1;
            var startDate = new DateTime(year, month, 1);
            var endDate   = startDate.AddMonths(1);

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            bool isClosed = db.MonthlyInventories.Any(mi => mi.Year == year && mi.Month == month && mi.IsClosed);
            lblStatus.Text      = isClosed ? "🔒 مقفل" : "📂 مفتوح";
            lblStatus.ForeColor = isClosed ? AppTheme.Danger : AppTheme.Accent;
            btnSave.Enabled = btnClose.Enabled = !isClosed;

            // أرصدة محفوظة مسبقاً لهذا الشهر (مسودة)
            var saved = db.MonthlyInventories
                .Where(mi => mi.Year == year && mi.Month == month)
                .ToDictionary(mi => mi.GoodId);

            var goods = db.Goods.Include(g => g.Unit).OrderBy(g => g.Name).ToList();

            grid.Rows.Clear();
            int diffCount = 0; double totalDiff = 0;

            foreach (var good in goods)
            {
                // حساب الرصيد الدفتري = كل الواردات - كل الصادرات حتى نهاية الشهر
                double stockIn  = db.Movements.Where(m => m.GoodId == good.Id && !m.Out && m.Date < endDate).Sum(m => (double?)m.Quantity) ?? 0;
                double stockOut = db.Movements.Where(m => m.GoodId == good.Id &&  m.Out && m.Date < endDate).Sum(m => (double?)m.Quantity) ?? 0;
                double system   = stockIn - stockOut;
                if (system < 0) system = 0;

                double actual = saved.TryGetValue(good.Id, out var rec) ? rec.ActualStock : system;
                double diff   = actual - system;

                int idx = grid.Rows.Add();
                var row = grid.Rows[idx];
                row.Cells["GoodId"].Value      = good.Id;
                row.Cells["Code"].Value        = good.Code;
                row.Cells["Name"].Value        = good.Name;
                row.Cells["SystemStock"].Value = system.ToString("N2");
                row.Cells["ActualStock"].Value = actual.ToString("N2");
                row.Cells["Diff"].Value        = diff.ToString("+0.##;-0.##;0");

                if (Math.Abs(diff) > 0.001) { diffCount++; totalDiff += diff; }
            }

            // تطبيق ReadOnly
            foreach (DataGridViewColumn col in grid.Columns)
                col.ReadOnly = isClosed || col.Name != "ActualStock";

            lblSummary.Text = $"أصناف: {goods.Count}  •  فروقات: {diffCount}  •  صافي الفرق: {totalDiff:+0.##;-0.##;0}";
        }

        private void Save(bool closeMonth)
        {
            int year  = (int)numYear.Value;
            int month = cboMonth.SelectedIndex + 1;
            var endDate = new DateTime(year, month, 1).AddMonths(1).AddSeconds(-1);

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            if (db.MonthlyInventories.Any(mi => mi.Year == year && mi.Month == month && mi.IsClosed))
            { UIHelper.ShowError("هذا الشهر مقفل بالفعل ولا يمكن تعديله."); return; }

            int fyId = MainForm.CurrentFiscalYearId
                ?? db.FiscalYears.Where(f => !f.IsClosed).OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();
            if (fyId == 0) fyId = db.FiscalYears.OrderByDescending(f => f.Year).Select(f => f.Id).FirstOrDefault();

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                int    goodId = (int)row.Cells["GoodId"].Value!;
                double system = double.TryParse(row.Cells["SystemStock"].Value?.ToString(), out double sys) ? sys : 0;
                double actual = double.TryParse(row.Cells["ActualStock"].Value?.ToString(), out double act) ? act : system;
                double diff   = actual - system;

                // حفظ / تحديث سجل الجرد
                var rec = db.MonthlyInventories.FirstOrDefault(mi => mi.Year == year && mi.Month == month && mi.GoodId == goodId);
                if (rec == null)
                {
                    rec = new MonthlyInventory { Year = year, Month = month, Date = DateTime.Today, GoodId = goodId };
                    db.MonthlyInventories.Add(rec);
                }
                rec.SystemStock = system;
                rec.ActualStock = actual;
                rec.IsClosed    = closeMonth;

                // عند الإقفال: أضف حركة تسوية لو في فرق
                if (closeMonth && Math.Abs(diff) > 0.001)
                {
                    // احذف أي تسوية قديمة للنفس الشهر
                    string tag = $"تسوية-جرد-{year}-{month:D2}-{goodId}";
                    db.Movements.RemoveRange(db.Movements.Where(m => m.BillNo == tag));

                    db.Movements.Add(new Movement
                    {
                        GoodId       = goodId,
                        Quantity     = Math.Abs(diff),
                        Out          = diff < 0,   // عجز: خروج  |  زيادة: دخول
                        Date         = endDate.Date,
                        Notes        = $"تسوية جرد {month:D2}/{year}",
                        BillNo       = tag,
                        StoreNo      = db.Stores.Select(s => s.Id).FirstOrDefault(),
                        IsBill       = false,
                        FiscalYearId = fyId
                    });
                }
            }

            db.SaveChanges();
            UIHelper.ShowSuccess(closeMonth
                ? $"✅ تم إقفال شهر {month:D2}/{year} وترحيل الفروقات للمخزون"
                : "✅ تم حفظ مسودة الجرد");
            LoadData();
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); LoadData(); }
    }
}
