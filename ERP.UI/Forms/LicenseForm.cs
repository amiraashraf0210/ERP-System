using ERP.UI.Helpers;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    // ── شاشة إدخال الـ License (عند العميل) ──
    public class LicenseActivationForm : Form
    {
        private TextBox txtKey = null!;
        private Label lblInfo = null!;

        public LicenseActivationForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "تفعيل البرنامج";
            this.Size = new Size(480, 360);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = AppTheme.Primary };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var tf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
                g.DrawString("🔑 تفعيل البرنامج", new Font("Segoe UI", 16, FontStyle.Bold),
                    Brushes.White, new RectangleF(0, 14, header.Width - 20, 36), tf);
                g.DrawString("أدخل كود التفعيل الذي حصلت عليه", AppTheme.FontSmall,
                    new SolidBrush(Color.FromArgb(190, 255, 255, 255)),
                    new RectangleF(0, 50, header.Width - 20, 26), tf);
            };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 20), BackColor = Color.White };

            body.Controls.Add(new Label
            {
                Text = "كود التفعيل:", Left = 22, Top = 0, Width = 420, Height = 24,
                Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes
            });

            txtKey = new TextBox
            {
                Left = 22, Top = 28, Width = 420, Height = 36,
                Font = new Font("Courier New", 11),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "XXXX-XXXX-XXXX-XXXX#...",
                RightToLeft = RightToLeft.No  // الـ key لاتيني
            };
            body.Controls.Add(txtKey);

            lblInfo = new Label
            {
                Left = 22, Top = 78, Width = 420, Height = 52,
                Font = AppTheme.FontNormal, TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes
            };
            body.Controls.Add(lblInfo);

            var btnActivate = new Button
            {
                Text = "✅ تفعيل", Left = 22, Top = 140, Width = 200, Height = 44,
                BackColor = AppTheme.Accent, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = AppTheme.FontBold, Cursor = Cursors.Hand
            };
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Click += (s, e) => ActivateLicense();

            var btnContact = new Button
            {
                Text = "📞 تواصل مع الدعم", Left = 242, Top = 140, Width = 200, Height = 44,
                BackColor = AppTheme.TextGray, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = AppTheme.FontNormal, Cursor = Cursors.Hand
            };
            btnContact.FlatAppearance.BorderSize = 0;
            btnContact.Click += (s, e) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = "mailto:support@erpsystem.com", UseShellExecute = true });

            body.Controls.Add(btnActivate);
            body.Controls.Add(btnContact);

            this.Controls.Add(body);
            this.Controls.Add(header);
        }

        private void ActivateLicense()
        {
            string key = txtKey.Text.Trim();
            if (string.IsNullOrEmpty(key)) { lblInfo.Text = "❌ أدخل كود التفعيل"; lblInfo.ForeColor = AppTheme.Danger; return; }

            var info = LicenseManager.ValidateKey(key);
            if (info == null)
            { lblInfo.Text = "❌ كود التفعيل غير صحيح"; lblInfo.ForeColor = AppTheme.Danger; return; }

            if (info.IsExpired)
            { lblInfo.Text = $"❌ انتهت صلاحية هذا الكود في {info.ExpiryDate:dd/MM/yyyy}"; lblInfo.ForeColor = AppTheme.Danger; return; }

            LicenseManager.SaveLicense(key);
            lblInfo.Text      = $"✅ تم التفعيل بنجاح!\n{info.CompanyName}  |  صالح حتى {info.ExpiryDate:dd/MM/yyyy}";
            lblInfo.ForeColor = AppTheme.Accent;

            Task.Delay(1500).ContinueWith(_ => this.Invoke(() => { this.DialogResult = DialogResult.OK; this.Close(); }));
        }
    }

    // ── شاشة توليد License (عندك إنتي — للمبيعات) ──
    public class LicenseGeneratorForm : Form
    {
        public LicenseGeneratorForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "🔑 توليد كود ترخيص جديد";
            this.Size = new Size(520, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6,
                Padding = new Padding(20), BackColor = Color.White
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            void AddRow(int r, string lbl, Control ctrl)
            {
                tbl.Controls.Add(new Label { Text = lbl, Dock = DockStyle.Fill, Font = AppTheme.FontBold, TextAlign = ContentAlignment.MiddleRight }, 0, r);
                tbl.Controls.Add(ctrl, 1, r);
            }

            var txtCompany  = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 8, 0, 8) };
            var dtpExpiry   = new DateTimePicker { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddYears(1), Margin = new Padding(0, 8, 0, 8) };
            var nudUsers    = new NumericUpDown { Dock = DockStyle.Fill, Font = AppTheme.FontNormal, Minimum = 1, Maximum = 999, Value = 5, Margin = new Padding(0, 8, 0, 8) };
            var txtResult   = new TextBox { Dock = DockStyle.Fill, Font = new Font("Courier New", 8), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true, Multiline = true, Margin = new Padding(0, 4, 0, 4), BackColor = Color.FromArgb(248, 250, 252) };

            AddRow(0, "اسم الشركة *:", txtCompany);
            AddRow(1, "تاريخ الانتهاء:", dtpExpiry);
            AddRow(2, "عدد المستخدمين:", nudUsers);
            AddRow(3, "الكود المولّد:", txtResult);

            var btnGen = new Button
            {
                Text = "⚡ توليد الكود", Dock = DockStyle.Fill,
                BackColor = AppTheme.Primary, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = AppTheme.FontBold, Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 4, 8)
            };
            btnGen.FlatAppearance.BorderSize = 0;
            btnGen.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtCompany.Text)) { UIHelper.ShowError("أدخل اسم الشركة"); return; }
                string key = LicenseManager.GenerateKey(txtCompany.Text, dtpExpiry.Value, (int)nudUsers.Value);
                txtResult.Text = key;
            };

            var btnCopy = new Button
            {
                Text = "📋 نسخ", Dock = DockStyle.Fill,
                BackColor = AppTheme.Accent, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = AppTheme.FontBold, Cursor = Cursors.Hand,
                Margin = new Padding(4, 8, 0, 8)
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtResult.Text))
                { Clipboard.SetText(txtResult.Text); UIHelper.ShowSuccess("تم النسخ ✅"); }
            };

            var btnRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            btnRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            btnRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            btnRow.Controls.Add(btnGen, 0, 0);
            btnRow.Controls.Add(btnCopy, 1, 0);

            tbl.Controls.Add(new Label(), 0, 4);
            tbl.Controls.Add(btnRow, 1, 4);

            var lblWarn = new Label { Text = "⚠ احتفظي بالكود — لا يمكن استعادته", Dock = DockStyle.Fill, Font = AppTheme.FontSmall, ForeColor = AppTheme.Warning, TextAlign = ContentAlignment.MiddleRight };
            tbl.Controls.Add(lblWarn, 0, 5);
            tbl.SetColumnSpan(lblWarn, 2);

            this.Controls.Add(tbl);
        }
    }
}
