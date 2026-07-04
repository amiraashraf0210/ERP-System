using ERP.UI.Helpers;

namespace ERP.UI.Forms
{
    public class BaseListForm : Form
    {
        protected DataGridView grid = null!;
        protected Button btnAdd = null!, btnEdit = null!, btnDelete = null!, btnRefresh = null!;
        protected TextBox txtSearch = null!;

        protected BaseListForm(string title)
        {
            this.Text = title;
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            BuildLayout();
        }

        private void BuildLayout()
        {
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Color.Transparent, Padding = new Padding(0) };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var toolbar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Surface, Padding = new Padding(10, 8, 10, 8) };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            var toolLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            toolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtSearch = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "بحث...", RightToLeft = RightToLeft.Yes, Margin = new Padding(0, 0, 8, 0) };
            txtSearch.TextChanged += (s, e) => OnSearch(txtSearch.Text);

            var flowBtns = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = false, BackColor = Color.Transparent, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };

            btnAdd     = Btn("+ إضافة",   AppTheme.Accent);
            btnEdit    = Btn("/ تعديل",    AppTheme.Primary);
            btnDelete  = Btn("- حذف",     AppTheme.Danger);
            btnRefresh = Btn("تحديث",     Color.FromArgb(90, 75, 60));

            btnAdd.Click     += (s, e) => OnAdd();
            btnEdit.Click    += (s, e) => OnEdit();
            btnDelete.Click  += (s, e) => OnDelete();
            btnRefresh.Click += (s, e) => LoadData();

            flowBtns.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });
            AddExtraButtons(flowBtns);

            toolLayout.Controls.Add(txtSearch, 0, 0);
            toolLayout.Controls.Add(flowBtns, 1, 0);
            toolbar.Controls.Add(toolLayout);

            grid = new DataGridView { RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.DataBindingComplete += (s, e) => { if (grid.Columns["Id"] is DataGridViewColumn idCol) idCol.Visible = false; };
            grid.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex >= 0)
                {
                    ev.CellStyle.BackColor = Color.White;
                    ev.CellStyle.ForeColor = AppTheme.TextDark;
                }
            };
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OnEdit(); };

            tbl.Controls.Add(toolbar, 0, 0);
            tbl.Controls.Add(UIHelper.WrapGrid(grid), 0, 1);
            this.Controls.Add(tbl);
        }

        protected static Button Btn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text, BackColor = bg, ForeColor = Color.White,
                Size = new Size(100, 36), FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bg, 0.1f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bg, 0.18f);
            return b;
        }

        protected virtual void LoadData() { }
        protected virtual void OnAdd() { }
        protected virtual void OnEdit() { }
        protected virtual void OnDelete() { }
        protected virtual void OnSearch(string keyword) { }
        protected virtual void AddExtraButtons(FlowLayoutPanel toolbar) { }

        protected int? GetSelectedId(string col = "Id")
        {
            if (grid.SelectedRows.Count == 0)
            { UIHelper.ShowError("اختار سطراً أولاً"); return null; }
            var row = grid.SelectedRows[0];
            if (grid.Columns.Contains(col) && row.Cells[col].Value is int id) return id;
            if (row.Cells[0].Value is int fid) return fid;
            return null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }
    }
}
