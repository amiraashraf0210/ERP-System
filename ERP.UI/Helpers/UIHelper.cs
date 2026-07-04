using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ERP.UI.Helpers
{
    public static class UIHelper
    {
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d > bounds.Height) d = bounds.Height;
            if (d > bounds.Width) d = bounds.Width;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Button MakeButton(string text, Color bg, Size size, Point loc)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = Color.White,
                Size = size,
                Location = loc,
                FlatStyle = FlatStyle.Flat,
                Font = AppTheme.FontBold,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bg, 0.08f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bg, 0.15f);
            return btn;
        }

        public static Label MakeLabel(string text, Font font, Color color, Point loc, Size size)
        {
            return new Label
            {
                Text = text, Font = font, ForeColor = color,
                Location = loc, Size = size, TextAlign = ContentAlignment.MiddleRight
            };
        }

        public static TextBox MakeTextBox(Point loc, Size size, string? placeholder = null)
        {
            return new TextBox
            {
                Location = loc, Size = size,
                Font = AppTheme.FontNormal,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
        }

        public static Panel MakeInputBox(Point loc, Size size, TextBox inner)
        {
            var box = new Panel
            {
                Location = loc,
                Size = size,
                BackColor = AppTheme.Surface,
                Padding = new Padding(10, 8, 10, 8)
            };
            inner.Dock = DockStyle.Fill;
            inner.BorderStyle = BorderStyle.None;
            inner.BackColor = AppTheme.Surface;
            box.Controls.Add(inner);

            bool focused = false;
            inner.GotFocus += (s, e) => { focused = true; box.Invalidate(); };
            inner.LostFocus += (s, e) => { focused = false; box.Invalidate(); };

            box.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, box.Width - 1, box.Height - 1);
                using var path = RoundedRect(rect, 6);
                using var border = new Pen(focused ? AppTheme.Primary : AppTheme.Border, focused ? 2f : 1f);
                g.DrawPath(border, path);
            };
            return box;
        }

        public static Panel MakeCard(Point loc, Size size, string title = "")
        {
            var card = new Panel
            {
                Location = loc, Size = size,
                BackColor = Color.White,
                Padding = new Padding(15)
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using var path = RoundedRect(rect, AppTheme.Radius);
                using var border = new Pen(AppTheme.Border);
                g.DrawPath(border, path);
                if (!string.IsNullOrEmpty(title))
                {
                    using var brush = new SolidBrush(AppTheme.Primary);
                    g.FillRectangle(brush, 0, 0, card.Width, 44);
                    using var tf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    g.DrawString(title, AppTheme.FontBold, Brushes.White, new RectangleF(0, 0, card.Width - 14, 44), tf);
                }
            };
            return card;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.GridHeader;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(210, 195, 175);
            grid.ColumnHeadersDefaultCellStyle.Font = AppTheme.FontBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
            grid.ColumnHeadersHeight = 44;
            grid.RowTemplate.Height = 38;
            grid.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.GridAlt;
            grid.DefaultCellStyle.Font = AppTheme.FontNormal;
            grid.DefaultCellStyle.ForeColor = AppTheme.TextDark;
            grid.DefaultCellStyle.BackColor = AppTheme.Surface;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 228, 210);
            grid.DefaultCellStyle.SelectionForeColor = AppTheme.TextDark;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = AppTheme.Surface;
            grid.GridColor = AppTheme.BorderLight;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                col.DefaultCellStyle.BackColor = AppTheme.Surface;
                col.DefaultCellStyle.ForeColor = AppTheme.TextDark;
                col.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 228, 210);
                col.DefaultCellStyle.SelectionForeColor = AppTheme.TextDark;
            }
        }

        public static Panel WrapGrid(DataGridView grid)
        {
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(1)
            };
            grid.Dock = DockStyle.Fill;
            wrapper.Controls.Add(grid);
            wrapper.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, wrapper.Width - 1, wrapper.Height - 1);
                using var path = RoundedRect(rect, AppTheme.Radius);
                using var border = new Pen(AppTheme.Border);
                g.DrawPath(border, path);
            };
            return wrapper;
        }

        public static Panel MakeSidebarItem(string icon, string text, Action onClick, ToolTip? tip = null)
        {
            var pnl = new Panel
            {
                Height = 38,
                Width = AppTheme.SidebarWidth - 8,
                Margin = new Padding(4, 1, 4, 1),
                BackColor = AppTheme.SidebarBg,
                Cursor = Cursors.Hand
            };

            var lbl = new Label
            {
                Text = $"{text}  {icon}",
                Dock = DockStyle.Fill,
                Font = AppTheme.FontMenu,
                ForeColor = AppTheme.SidebarText,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(8, 0, 14, 0),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            tip?.SetToolTip(pnl, text);
            tip?.SetToolTip(lbl, text);

            void Activate(bool active)
            {
                pnl.BackColor = active ? AppTheme.SidebarActive : AppTheme.SidebarBg;
                lbl.ForeColor = active ? Color.White : AppTheme.SidebarText;
                lbl.Font = active ? AppTheme.FontBold : AppTheme.FontMenu;
                pnl.Invalidate();
            }

            pnl.Tag = (Action<bool>)Activate;
            pnl.Controls.Add(lbl);

            pnl.Paint += (s, e) =>
            {
                if (pnl.BackColor == AppTheme.SidebarActive)
                {
                    e.Graphics.FillRectangle(Brushes.White, pnl.Width - 3, 8, 3, pnl.Height - 16);
                }
            };

            void Click(object? s, EventArgs e) => onClick();
            pnl.Click += Click;
            lbl.Click += Click;
            pnl.MouseEnter += (s, e) => { if (pnl.BackColor != AppTheme.SidebarActive) pnl.BackColor = AppTheme.SidebarHover; };
            pnl.MouseLeave += (s, e) => { if (pnl.BackColor != AppTheme.SidebarActive) pnl.BackColor = AppTheme.SidebarBg; };

            return pnl;
        }

        public static void SetSidebarItemActive(Panel item, bool active)
        {
            if (item.Tag is Action<bool> setActive) setActive(active);
        }

        public static ComboBox MakeCombo(Point loc, Size size)
        {
            return new ComboBox
            {
                Location = loc, Size = size,
                Font = AppTheme.FontNormal,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
        }

        public static void ShowSuccess(string message) =>
            MessageBox.Show(message, "تم", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void ShowError(string message) =>
            MessageBox.Show(message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static bool Confirm(string message) =>
            MessageBox.Show(message, "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
