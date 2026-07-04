using ERP.Data;
using ERP.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Button btnLogin = null!;
        private Panel pnlLeft = null!, pnlRight = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "نظام ERP - تسجيل الدخول";
            this.Size = new Size(860, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            pnlLeft = new Panel
            {
                Size = new Size(340, 520),
                Location = new Point(520, 0),
                BackColor = AppTheme.PrimaryDark
            };
            pnlLeft.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = new LinearGradientBrush(
                    pnlLeft.ClientRectangle,
                    AppTheme.PrimaryDark, AppTheme.PrimaryLight,
                    LinearGradientMode.Vertical);
                g.FillRectangle(brush, pnlLeft.ClientRectangle);

                var circleRect = new Rectangle(95, 90, 150, 150);
                using var circlePath = UIHelper.RoundedRect(circleRect, 75);
                using var circleBrush = new SolidBrush(Color.FromArgb(35, Color.White));
                g.FillPath(circleBrush, circlePath);

                using var tf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("⚙", new Font("Segoe UI Emoji", 46), Brushes.White, new RectangleF(95, 90, 150, 150), tf);
                g.DrawString("نظام ERP", new Font("Segoe UI", 22, FontStyle.Bold), Brushes.White,
                    new RectangleF(10, 270, 320, 50), tf);
                g.DrawString("نظام إدارة الأعمال المتكامل", new Font("Segoe UI", 10.5f),
                    new SolidBrush(Color.FromArgb(190, 255, 255, 255)), new RectangleF(10, 322, 320, 35), tf);

                var dots = new[] { (120, 400), (160, 400), (200, 400) };
                foreach (var (dx, dy) in dots)
                    g.FillEllipse(new SolidBrush(Color.FromArgb(60, Color.White)), dx, dy, 8, 8);
            };

            pnlRight = new Panel
            {
                Size = new Size(520, 520),
                Location = new Point(0, 0),
                BackColor = Color.White
            };

            var lblWelcome = new Label
            {
                Text = "أهلاً بك",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                Location = new Point(60, 70),
                Size = new Size(400, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblSub = new Label
            {
                Text = "سجّل دخولك للمتابعة",
                Font = AppTheme.FontNormal,
                ForeColor = AppTheme.TextGray,
                Location = new Point(60, 122),
                Size = new Size(400, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblUser = new Label
            {
                Text = "اسم المستخدم",
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location = new Point(70, 178),
                Size = new Size(380, 24),
                TextAlign = ContentAlignment.MiddleRight
            };

            txtUsername = new TextBox { Text = "admin", Font = AppTheme.FontNormal };
            var pnlUserBox = UIHelper.MakeInputBox(new Point(70, 206), new Size(380, 46), txtUsername);

            var lblPass = new Label
            {
                Text = "كلمة المرور",
                Font = AppTheme.FontBold,
                ForeColor = AppTheme.TextDark,
                Location = new Point(70, 268),
                Size = new Size(380, 24),
                TextAlign = ContentAlignment.MiddleRight
            };

            txtPassword = new TextBox { PasswordChar = '●', Text = "admin", Font = AppTheme.FontNormal };
            var pnlPassBox = UIHelper.MakeInputBox(new Point(70, 296), new Size(380, 46), txtPassword);

            btnLogin = new Button
            {
                Text = "  دخول  ←",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(380, 50),
                Location = new Point(70, 370),
                BackColor = AppTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = AppTheme.PrimaryDark;
            btnLogin.FlatAppearance.MouseDownBackColor = AppTheme.PrimaryDark;
            btnLogin.Click += BtnLogin_Click;

            var lblVer = new Label
            {
                Text = "v1.0.0",
                Font = AppTheme.FontSmall,
                ForeColor = AppTheme.TextMuted,
                Location = new Point(70, 440),
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlRight.Controls.AddRange(new Control[]
            { lblWelcome, lblSub, lblUser, pnlUserBox, lblPass, pnlPassBox, btnLogin, lblVer });

            this.Controls.Add(pnlRight);
            this.Controls.Add(pnlLeft);
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                UIHelper.ShowError("يرجى إدخال اسم المستخدم وكلمة المرور");
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "جارٍ التحقق...";

            using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.FirstOrDefault(u =>
                u.Name == txtUsername.Text.Trim() && u.Pass == txtPassword.Text);

            if (user != null)
            {
                var main = new MainForm(user);
                this.Hide();
                main.FormClosed += (s2, args) => this.Close();
                main.Show();
            }
            else
            {
                UIHelper.ShowError("اسم المستخدم أو كلمة المرور غير صحيحة");
                txtPassword.Clear();
                txtPassword.Focus();
                btnLogin.Enabled = true;
                btnLogin.Text = "  دخول  ←";
            }
        }
    }
}
