using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ERP.Sales
{
    public class SalesForm : Form
    {
        private TextBox txtCompany = null!, txtResult = null!;
        private NumericUpDown nudUsers;
        private Label lblInfo = null!;

        public SalesForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "🔑 أداة توليد ترخيص ERP — خاص بالمورد";
            this.Size = new Size(560, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.FromArgb(37, 99, 235) };
            header.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var tf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
                g.DrawString("أداة توليد ترخيص ERP", new Font("Segoe UI", 15, FontStyle.Bold),
                    Brushes.White, new RectangleF(0, 12, header.Width - 20, 36), tf);
                g.DrawString("هذه الأداة خاصة بالمورد فقط — لا تشاركها مع العملاء",
                    new Font("Segoe UI", 9), new SolidBrush(Color.FromArgb(200, 255, 255, 255)),
                    new RectangleF(0, 44, header.Width - 20, 22), tf);
            };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 20, 28, 16), BackColor = Color.White };

            int y = 0;

            body.Controls.Add(new Label { Text = "اسم الشركة / العميل *:", Left = 0, Top = y, Width = 500, Height = 24, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight });
            y += 28;
            txtCompany = new TextBox { Left = 0, Top = y, Width = 500, Height = 32, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "مثال: شركة النيل للتجارة" };
            body.Controls.Add(txtCompany);
            y += 44;

            body.Controls.Add(new Label { Text = "عدد المستخدمين المسموح بهم:", Left = 0, Top = y, Width = 500, Height = 24, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight });
            y += 28;
            nudUsers = new NumericUpDown { Left = 0, Top = y, Width = 500, Height = 32, Font = new Font("Segoe UI", 11), Minimum = 1, Maximum = 9999, Value = 5, BorderStyle = BorderStyle.FixedSingle };
            body.Controls.Add(nudUsers);
            y += 44;

            // Note
            var note = new Panel { Left = 0, Top = y, Width = 500, Height = 38, BackColor = Color.FromArgb(239, 246, 255) };
            note.Controls.Add(new Label
            {
                Text = "✅  الترخيص دائم — بدون تاريخ انتهاء",
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129), TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 12, 0)
            });
            body.Controls.Add(note);
            y += 50;

            // Generate button
            var btnGen = new Button
            {
                Text = "⚡ توليد كود الترخيص", Left = 0, Top = y, Width = 500, Height = 46,
                BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnGen.FlatAppearance.BorderSize = 0;
            btnGen.Click += BtnGen_Click;
            body.Controls.Add(btnGen);
            y += 58;

            // Result box
            body.Controls.Add(new Label { Text = "الكود المولّد (ابعتيه للعميل):", Left = 0, Top = y, Width = 500, Height = 24, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight });
            y += 28;
            txtResult = new TextBox
            {
                Left = 0, Top = y, Width = 420, Height = 34,
                Font = new Font("Courier New", 9), BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true, BackColor = Color.FromArgb(248, 250, 252),
                RightToLeft = RightToLeft.No
            };

            var btnCopy = new Button
            {
                Text = "📋 نسخ", Left = 428, Top = y, Width = 72, Height = 34,
                BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtResult.Text))
                {
                    Clipboard.SetText(txtResult.Text);
                    btnCopy.Text = "✅ تم";
                    Task.Delay(1500).ContinueWith(_ => this.Invoke(() => btnCopy.Text = "📋 نسخ"));
                }
            };
            body.Controls.Add(txtResult);
            body.Controls.Add(btnCopy);
            y += 44;

            lblInfo = new Label { Left = 0, Top = y, Width = 500, Height = 28, Font = new Font("Segoe UI", 9), TextAlign = ContentAlignment.MiddleRight };
            body.Controls.Add(lblInfo);

            this.Controls.Add(body);
            this.Controls.Add(header);
        }

        private void BtnGen_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompany.Text))
            {
                MessageBox.Show("أدخل اسم الشركة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string key = GenerateKey(txtCompany.Text.Trim(), (int)nudUsers.Value);
            txtResult.Text = key;
            lblInfo.Text      = $"✅ تم توليد ترخيص دائم لـ: {txtCompany.Text.Trim().ToUpper()}  |  {nudUsers.Value} مستخدم";
            lblInfo.ForeColor = Color.FromArgb(16, 185, 129);
        }

        // ── نفس خوارزمية LicenseManager ──
        private static string GenerateKey(string companyName, int maxUsers)
        {
            const string SecretKey = "ERP@L1c3ns3#S3cr3t$2025!";
            string expStr  = "99991231"; // دائمة
            string payload = $"{companyName.ToUpper()}|{expStr}|{maxUsers}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            string hash    = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLower();
            string shortH  = hash[..16].ToUpper();
            string key     = $"{shortH[..4]}-{shortH[4..8]}-{shortH[8..12]}-{shortH[12..16]}";
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
                             .Replace("+", "-").Replace("/", "_").Replace("=", "");
            return $"{key}#{encoded}";
        }
    }
}
