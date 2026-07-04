using ERP.Core.Models;
using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.UI.Forms
{
    public class MonthlyReportsForm : Form
    {
        private DataGridView grid = null!;
        private ComboBox cboYear = null!;
        private ComboBox cboMonth = null!;
        private Label lblSummary = null!;

        public MonthlyReportsForm()
        {
            BuildUI();
            LoadYears();
        }

        private void BuildUI()
        {
            this.Text = "📊 التقارير الشهرية";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Filter bar
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top, Height = 60, BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };
            pnlFilter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, BackColor = Color.Transparent
            };

            var lblYear = new Label
            {
                Text = "السنة:", Font = AppTheme.FontBold,
                Width = 55, Height = 36, TextAlign = ContentAlignment.MiddleRight
            };
            cboYear = new ComboBox
            {
                Width = 100, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontNormal, Margin = new Padding(0, 0, 12, 0)
            };
            cboYear.SelectedIndexChanged += (s, e) => LoadReport();

            var lblMonth = new Label
            {
                Text = "الشهر:", Font = AppTheme.FontBold,
                Width = 55, Height = 36, TextAlign = ContentAlignment.MiddleRight
            };
            cboMonth = new ComboBox
            {
                Width = 140, Height = 36, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontNormal, Margin = new Padding(0, 0, 12, 0)
            };
            var months = new[]
            {
                "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
            };
            for (int i = 0; i < months.Length; i++)
                cboMonth.Items.Add(new { Name = months[i], Value = i + 1 });
            cboMonth.DisplayMember = "Name";
            cboMonth.ValueMember = "Value";
            cboMonth.SelectedIndex = DateTime.Today.Month - 1;
            cboMonth.SelectedIndexChanged += (s, e) => LoadReport();

            var btnRefresh = UIHelper.MakeButton("🔄 تحديث", AppTheme.Primary, new Size(110, 36), Point.Empty);
            btnRefresh.Margin = new Padding(0, 0, 0, 0);
            btnRefresh.Click += (s, e) => LoadReport();

            // RightToLeft FlowPanel: أضيف بالعكس عشان يظهر صح (السنة أقصى يمين)
            flow.Controls.AddRange(new Control[] { btnRefresh, cboMonth, lblMonth, cboYear, lblYear });
            pnlFilter.Controls.Add(flow);

            // Summary bar
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(239, 246, 255),
                Padding = new Padding(16, 0, 16, 0)
            };
            pnlSummary.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, pnlSummary.Height - 1, pnlSummary.Width, pnlSummary.Height - 1);
            lblSummary = new Label
            {
                Dock = DockStyle.Fill, Font = AppTheme.FontBold,
                ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight
            };
            pnlSummary.Controls.Add(lblSummary);

            // Grid
            grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", Width = 150, SortMode = DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name = "Count", HeaderText = "العدد", Width = 100, SortMode = DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 150, SortMode = DataGridViewColumnSortMode.NotSortable }
            );

            var gridWrap = UIHelper.WrapGrid(grid);

            this.Controls.Add(gridWrap);
            this.Controls.Add(pnlSummary);
            this.Controls.Add(pnlFilter);
        }

        private void LoadYears()
        {
            try
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var fiscalYears = db.FiscalYears.OrderByDescending(f => f.Year).ToList();
                
                if (fiscalYears.Any())
                {
                    cboYear.DisplayMember = "Year";
                    cboYear.ValueMember = "Id";
                    cboYear.DataSource = fiscalYears;
                    
                    var currentYear = fiscalYears.FirstOrDefault(f => f.Year == DateTime.Today.Year && !f.IsClosed)
                        ?? fiscalYears.FirstOrDefault(f => !f.IsClosed)
                        ?? fiscalYears.First();
                    cboYear.SelectedItem = currentYear;
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"خطأ في تحميل السنوات: {ex.Message}");
            }
        }

        private void LoadReport()
        {
            if (cboYear.SelectedItem == null || cboMonth.SelectedItem == null) return;

            var fiscalYear = cboYear.SelectedItem as FiscalYear;
            if (fiscalYear == null) return;

            var monthData = cboMonth.SelectedItem as dynamic;
            int month = monthData?.Value ?? DateTime.Today.Month;

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();

            // Filter by fiscal year and month
            var sellBills = db.SellBills
                .Where(b => b.FiscalYearId == fiscalYear.Id && b.Date.Month == month && b.Date.Year == fiscalYear.Year)
                .ToList();

            var buyBills = db.BuyBills
                .Where(b => b.FiscalYearId == fiscalYear.Id && b.Date.Month == month && b.Date.Year == fiscalYear.Year)
                .ToList();

            var expenses = db.Expenses
                .Where(e => e.FiscalYearId == fiscalYear.Id && e.Date.Month == month && e.Date.Year == fiscalYear.Year)
                .ToList();

            var incomes = db.Incomes
                .Where(i => i.FiscalYearId == fiscalYear.Id && i.Date.Month == month && i.Date.Year == fiscalYear.Year)
                .ToList();

            var boxQuery = db.BoxTransactions
                .Where(bt => bt.FiscalYearId == fiscalYear.Id && bt.Date.Month == month && bt.Date.Year == fiscalYear.Year);

            grid.Rows.Clear();

            // Sales
            grid.Rows.Add("فواتير المبيعات", sellBills.Count.ToString(), sellBills.Sum(b => b.Asked).ToString("N2"));
            // Purchases
            grid.Rows.Add("فواتير المشتريات", buyBills.Count.ToString(), buyBills.Sum(b => b.Asked).ToString("N2"));
            // Expenses
            grid.Rows.Add("المصروفات", expenses.Count.ToString(), expenses.Sum(e => e.Value).ToString("N2"));
            // Incomes
            grid.Rows.Add("الإيرادات", incomes.Count.ToString(), incomes.Sum(i => i.Value).ToString("N2"));
            // Net Profit
            double totalSales = sellBills.Sum(b => b.Asked);
            double totalPurchases = buyBills.Sum(b => b.Asked);
            double totalExpenses = expenses.Sum(e => e.Value);
            double totalIncomes = incomes.Sum(i => i.Value);
            double netProfit = totalSales - totalPurchases - totalExpenses + totalIncomes;
            grid.Rows.Add("صافي الربح", "", netProfit.ToString("N2"));

            // Update summary
            lblSummary.Text = $"تقرير شهر {monthData?.Name} {fiscalYear.Year}  |  صافي الربح: {netProfit:N2}";
        }
    }
}
