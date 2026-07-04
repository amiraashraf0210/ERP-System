using ERP.UI.Helpers;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    public class SetupConnectionForm : Form
    {
        private TextBox txtServer = null!, txtDatabase = null!, txtUser = null!, txtPass = null!;
        private RadioButton rbWindows = null!, rbSQL = null!;
        private Panel pnlSQL = null!;
        private Label lblStatus = null!;

        public SetupConnectionForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text                = "إعداد الاتصال بقاعدة البيانات";
            this.Size                = new Size(500, 540);
            this.StartPosition       = FormStartPosition.CenterScreen;
            this.FormBorderStyle     = FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.BackColor           = Color.White;
            this.RightToLeft         = RightToLeft.Yes;
            this.RightToLeftLayout   = true;

            // ── Header ──
            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = AppTheme.Primary };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var tf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
                g.DrawString("إعداد قاعدة البيانات", new Font("Segoe UI", 16, FontStyle.Bold),
                    Brushes.White, new RectangleF(0, 12, header.Width - 20, 36), tf);
                g.DrawString("أدخل بيانات الاتصال بالسيرفر", AppTheme.FontSmall,
                    new SolidBrush(Color.FromArgb(190, 255, 255, 255)),
                    new RectangleF(0, 48, header.Width - 20, 26), tf);
            };

            // ── Body using TableLayoutPanel for proper RTL alignment ──
            var body = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 8,
                BackColor   = Color.White,
                Padding     = new Padding(24, 16, 24, 16),
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // lbl server
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // txt server
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // lbl database
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // txt database
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));  // lbl auth + radios
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // sql auth panel
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));  // status
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // buttons

            // Server label + textbox
            body.Controls.Add(MakeLbl("اسم السيرفر:"), 0, 0);
            txtServer = MakeTxt("(localdb)\\MSSQLLocalDB");
            body.Controls.Add(txtServer, 0, 1);

            // Database label + textbox
            body.Controls.Add(MakeLbl("اسم قاعدة البيانات:"), 0, 2);
            txtDatabase = MakeTxt("ERPSystem");
            body.Controls.Add(txtDatabase, 0, 3);

            // Auth type row: label + two radio buttons in a FlowPanel
            var pnlAuth = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor     = Color.White,
                WrapContents  = false,
            };
            pnlAuth.Controls.Add(MakeLbl("نوع المصادقة:"));

            rbWindows = new RadioButton
            {
                Text         = "Windows Authentication",
                AutoSize     = true,
                Checked      = true,
                Font         = AppTheme.FontNormal,
                CheckAlign   = ContentAlignment.MiddleRight,
                TextAlign    = ContentAlignment.MiddleRight,
                RightToLeft  = RightToLeft.Yes,
                Margin       = new Padding(16, 4, 4, 0),
            };
            rbSQL = new RadioButton
            {
                Text         = "SQL Server Authentication",
                AutoSize     = true,
                Font         = AppTheme.FontNormal,
                CheckAlign   = ContentAlignment.MiddleRight,
                TextAlign    = ContentAlignment.MiddleRight,
                RightToLeft  = RightToLeft.Yes,
                Margin       = new Padding(16, 4, 4, 0),
            };
            pnlAuth.Controls.Add(rbWindows);
            pnlAuth.Controls.Add(rbSQL);
            body.Controls.Add(pnlAuth, 0, 4);

            // SQL auth credentials panel
            pnlSQL = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Visible   = false,
                Padding   = new Padding(8),
            };
            pnlSQL.Paint += (s, e) =>
            {
                using var pen  = new Pen(AppTheme.Border);
                using var path = UIHelper.RoundedRect(new Rectangle(0, 0, pnlSQL.Width - 1, pnlSQL.Height - 1), 6);
                e.Graphics.DrawPath(pen, path);
            };

            var sqlLayout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2,
                BackColor   = Color.Transparent,
            };
            sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            sqlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            sqlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            sqlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            sqlLayout.Controls.Add(MakeLbl("اسم المستخدم:"), 0, 0);
            sqlLayout.Controls.Add(MakeLbl("كلمة المرور:"),  1, 0);

            txtUser = MakeTxt("sa");
            txtPass = MakeTxt(""); txtPass.PasswordChar = '●';
            sqlLayout.Controls.Add(txtUser, 0, 1);
            sqlLayout.Controls.Add(txtPass, 1, 1);

            pnlSQL.Controls.Add(sqlLayout);
            body.Controls.Add(pnlSQL, 0, 5);

            rbWindows.CheckedChanged += (s, e) => pnlSQL.Visible = !rbWindows.Checked;
            rbSQL.CheckedChanged     += (s, e) => pnlSQL.Visible =  rbSQL.Checked;

            // Status label
            lblStatus = new Label
            {
                Dock      = DockStyle.Fill,
                Font      = AppTheme.FontNormal,
                TextAlign = ContentAlignment.MiddleRight,
            };
            body.Controls.Add(lblStatus, 0, 6);

            // Buttons row
            var pnlBtns = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.White,
            };
            pnlBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var btnTest = MakeBtn("🔌 اختبار الاتصال", AppTheme.Primary);
            var btnSave = MakeBtn("💾 حفظ والمتابعة",  AppTheme.Accent);
            btnTest.Click += BtnTest_Click;
            btnSave.Click += BtnSave_Click;

            // Right-to-left: save on right, test on left
            pnlBtns.Controls.Add(btnTest, 1, 0);
            pnlBtns.Controls.Add(btnSave, 0, 0);
            body.Controls.Add(pnlBtns, 0, 7);

            this.Controls.Add(body);
            this.Controls.Add(header);
        }

        // ── Helpers ──
        private static Label MakeLbl(string text) => new Label
        {
            Text      = text,
            Dock      = DockStyle.Fill,
            Font      = AppTheme.FontBold,
            ForeColor = AppTheme.TextDark,
            TextAlign = ContentAlignment.MiddleRight,
        };

        private static TextBox MakeTxt(string placeholder) => new TextBox
        {
            Dock            = DockStyle.Fill,
            Font            = AppTheme.FontNormal,
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = placeholder,
            RightToLeft     = RightToLeft.No,  // connection strings are LTR
            Margin          = new Padding(0, 4, 0, 4),
        };

        private static Button MakeBtn(string text, Color color)
        {
            var btn = new Button
            {
                Text      = text,
                Dock      = DockStyle.Fill,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = AppTheme.FontBold,
                Cursor    = Cursors.Hand,
                Height    = 44,
                Margin    = new Padding(4),
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private string BuildConnStr() =>
            ConnectionManager.Build(
                txtServer.Text.Trim(), txtDatabase.Text.Trim(),
                rbWindows.Checked, txtUser.Text.Trim(), txtPass.Text);

        private void BtnTest_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            lblStatus.Text      = "⏳ جاري الاختبار...";
            lblStatus.ForeColor = AppTheme.TextGray;
            Application.DoEvents();

            var (ok, msg) = ConnectionManager.TestConnection(BuildConnStr());
            lblStatus.Text      = msg;
            lblStatus.ForeColor = ok ? AppTheme.Accent : AppTheme.Danger;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            var connStr   = BuildConnStr();
            var (ok, msg) = ConnectionManager.TestConnection(connStr);
            if (!ok) { lblStatus.Text = msg; lblStatus.ForeColor = AppTheme.Danger; return; }
            ConnectionManager.Save(connStr);
            UIHelper.ShowSuccess("✅ تم حفظ الإعدادات بنجاح!\nسيتم إعادة تشغيل البرنامج.");
            Application.Restart();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            { UIHelper.ShowError("أدخل اسم السيرفر"); return false; }
            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            { UIHelper.ShowError("أدخل اسم قاعدة البيانات"); return false; }
            if (rbSQL.Checked && string.IsNullOrWhiteSpace(txtUser.Text))
            { UIHelper.ShowError("أدخل اسم المستخدم"); return false; }
            return true;
        }
    }
}
