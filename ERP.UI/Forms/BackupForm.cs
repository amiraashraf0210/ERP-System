using ERP.UI.Helpers;
using ERP.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;

namespace ERP.UI.Forms
{
    public class BackupForm : Form
    {
        /// <summary>
        /// اسم قاعدة البيانات يُستخرج من connection string المحفوظة.
        /// يُرجع "ERPSystem" كقيمة افتراضية لو ما أمكن استخراجها.
        /// </summary>
        private static string DbName
        {
            get
            {
                var cs = ConnectionManager.Load();
                if (string.IsNullOrEmpty(cs)) return "ERPSystem";
                // ابحث عن Database= أو Initial Catalog=
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs);
                return string.IsNullOrEmpty(builder.InitialCatalog) ? "ERPSystem" : builder.InitialCatalog;
            }
        }

        /// <summary>
        /// Connection string لـ master يُبنى من الـ connection string المحفوظة
        /// حتى يتصل بنفس السيرفر الصحيح (مش localhost ثابت).
        /// </summary>
        private static string MasterConnStr
        {
            get
            {
                var cs = ConnectionManager.Load();
                if (string.IsNullOrEmpty(cs))
                    return "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs)
                {
                    InitialCatalog = "master"
                };
                return builder.ConnectionString;
            }
        }

        private static string BackupFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ERP_Backups");

        private DataGridView grid = null!;

        public BackupForm()
        {
            BuildUI();
            RefreshList();
        }

        private void BuildUI()
        {
            this.Text = "💾 النسخ الاحتياطي والاسترداد";
            this.BackColor = AppTheme.Light;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // شريط الأدوات
            var toolbar = new Panel
            {
                Dock = DockStyle.Top, Height = 60,
                BackColor = Color.White, Padding = new Padding(12, 10, 12, 10)
            };
            toolbar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(AppTheme.Border), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, BackColor = Color.Transparent
            };

            var btnBackup  = MakeBtn("💾 نسخة احتياطية الآن", AppTheme.Accent);
            var btnBackupFY = MakeBtn("🗂 أرشفة السنة المالية", AppTheme.Primary);
            var btnRestore = MakeBtn("🔄 استرداد نسخة",       AppTheme.Warning);
            var btnDelete  = MakeBtn("🗑 حذف نسخة",           AppTheme.Danger);
            var btnOpenDir = MakeBtn("📂 فتح المجلد",         AppTheme.TextGray);

            btnBackup.Click  += (s, e) => DoBackup();
            btnBackupFY.Click += (s, e) => DoFiscalYearBackup();
            btnRestore.Click += (s, e) => DoRestore();
            btnDelete.Click  += (s, e) => DoDelete();
            btnOpenDir.Click += (s, e) =>
            {
                Directory.CreateDirectory(BackupFolder);
                System.Diagnostics.Process.Start("explorer.exe", BackupFolder);
            };

            flow.Controls.AddRange(new Control[] { btnBackup, btnBackupFY, btnRestore, btnDelete, btnOpenDir });
            toolbar.Controls.Add(flow);

            // معلومات
            var infoBar = new Panel
            {
                Dock = DockStyle.Top, Height = 36,
                BackColor = Color.FromArgb(239, 246, 255), Padding = new Padding(12, 0, 12, 0)
            };
            var lblInfo = new Label
            {
                Dock = DockStyle.Fill, Font = AppTheme.FontSmall,
                ForeColor = AppTheme.Primary, TextAlign = ContentAlignment.MiddleRight,
                Text = $"📁 مجلد النسخ الاحتياطية: {BackupFolder}"
            };
            infoBar.Controls.Add(lblInfo);

            // الجدول
            grid = new DataGridView { Dock = DockStyle.Fill, RightToLeft = RightToLeft.Yes };
            UIHelper.StyleGrid(grid);
            grid.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "اسم الملف",   Width = 280 },
                new DataGridViewTextBoxColumn { Name = "Date",     HeaderText = "التاريخ",      Width = 140 },
                new DataGridViewTextBoxColumn { Name = "Size",     HeaderText = "الحجم",        Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Path",     HeaderText = "المسار الكامل", Width = 0 }
            );
            if (grid.Columns["Path"] is DataGridViewColumn col) col.Visible = false;

            var gridWrap = UIHelper.WrapGrid(grid);

            this.Controls.Add(gridWrap);
            this.Controls.Add(infoBar);
            this.Controls.Add(toolbar);
        }

        // ── إنشاء نسخة احتياطية ──
        private void DoBackup()
        {
            var connStr = MasterConnStr;
            var dbName  = DbName;

            // تحقق إن فيه connection string محفوظ
            if (!ConnectionManager.IsConfigured)
            {
                UIHelper.ShowError("لا يوجد اتصال بقاعدة البيانات. يرجى إعداد الاتصال أولاً.");
                return;
            }

            try
            {
                Directory.CreateDirectory(BackupFolder);
                string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string file  = Path.Combine(BackupFolder, $"ERP_Backup_{stamp}.bak");

                using var conn = new SqlConnection(connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = N'{file}' WITH FORMAT, STATS = 10";
                cmd.ExecuteNonQuery();

                RefreshList();
                UIHelper.ShowSuccess($"✅ تم حفظ النسخة الاحتياطية\n{Path.GetFileName(file)}");

                // احذف النسخ اللي أكبر من 30 يوم تلقائياً
                AutoCleanOldBackups();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"خطأ في النسخ الاحتياطي:\n{ex.Message}");
            }
        }

        // ── أرشفة السنة المالية ──
        private void DoFiscalYearBackup()
        {
            if (!ConnectionManager.IsConfigured)
            {
                UIHelper.ShowError("لا يوجد اتصال بقاعدة البيانات. يرجى إعداد الاتصال أولاً.");
                return;
            }

            // Get current fiscal year
            int? currentFyId = MainForm.CurrentFiscalYearId;
            if (!currentFyId.HasValue)
            {
                UIHelper.ShowError("لا توجد سنة مالية محددة حالياً");
                return;
            }

            try
            {
                using var db = Program.ServiceProvider.GetRequiredService<AppDbContext>();
                var fiscalYear = db.FiscalYears.Find(currentFyId.Value);
                if (fiscalYear == null)
                {
                    UIHelper.ShowError("السنة المالية المحددة غير موجودة");
                    return;
                }

                if (!UIHelper.Confirm($"إنشاء نسخة احتياطية أرشيف للسنة المالية {fiscalYear.Year}؟\nسيتم حفظ الملف باسم: Archive_{fiscalYear.Year}.bak"))
                    return;

                Directory.CreateDirectory(BackupFolder);
                string file = Path.Combine(BackupFolder, $"Archive_{fiscalYear.Year}.bak");

                var connStr = MasterConnStr;
                var dbName = DbName;

                using var conn = new SqlConnection(connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = N'{file}' WITH FORMAT, STATS = 10";
                cmd.ExecuteNonQuery();

                RefreshList();
                UIHelper.ShowSuccess($"✅ تم أرشفة السنة المالية {fiscalYear.Year}\nالملف: {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"خطأ في الأرشفة:\n{ex.Message}");
            }
        }

        // ── استرداد نسخة ──
        private void DoRestore()
        {
            if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("اختر نسخة أولاً"); return; }
            string file = grid.SelectedRows[0].Cells["Path"].Value?.ToString() ?? "";
            if (!File.Exists(file)) { UIHelper.ShowError("الملف غير موجود"); return; }

            if (!ConnectionManager.IsConfigured)
            {
                UIHelper.ShowError("لا يوجد اتصال بقاعدة البيانات. يرجى إعداد الاتصال أولاً.");
                return;
            }

            if (!UIHelper.Confirm(
                $"⚠️ تحذير: سيتم استبدال قاعدة البيانات الحالية بالنسخة المختارة.\n" +
                $"النسخة: {Path.GetFileName(file)}\n\n" +
                "هل أنت متأكد؟ هذا الإجراء لا يمكن التراجع عنه!"))
                return;

            var connStr = MasterConnStr;
            var dbName  = DbName;

            try
            {
                using var conn = new SqlConnection(connStr);
                conn.Open();

                // إغلاق كل الاتصالات الأخرى بالـ DB
                using var killCmd = conn.CreateCommand();
                killCmd.CommandText = $@"
                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                killCmd.ExecuteNonQuery();

                // استرداد النسخة
                using var restoreCmd = conn.CreateCommand();
                restoreCmd.CommandTimeout = 300;
                restoreCmd.CommandText = $@"
                    RESTORE DATABASE [{dbName}]
                    FROM DISK = N'{file}'
                    WITH REPLACE, STATS = 10;
                    ALTER DATABASE [{dbName}] SET MULTI_USER;";
                restoreCmd.ExecuteNonQuery();

                UIHelper.ShowSuccess("✅ تم استرداد النسخة الاحتياطية بنجاح!\nسيتم إعادة تشغيل البرنامج.");
                Application.Restart();
            }
            catch (Exception ex)
            {
                // تأكد إن الـ DB رجع لـ multi user لو فيه خطأ
                try
                {
                    using var conn2 = new SqlConnection(connStr);
                    conn2.Open();
                    using var cmd2 = conn2.CreateCommand();
                    cmd2.CommandText = $"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                    cmd2.ExecuteNonQuery();
                }
                catch { /* ignore */ }

                UIHelper.ShowError($"خطأ في الاسترداد:\n{ex.Message}");
            }
        }

        // ── حذف نسخة ──
        private void DoDelete()
        {
            if (grid.SelectedRows.Count == 0) { UIHelper.ShowError("اختر نسخة أولاً"); return; }
            string file = grid.SelectedRows[0].Cells["Path"].Value?.ToString() ?? "";
            if (!UIHelper.Confirm($"حذف هذه النسخة؟\n{Path.GetFileName(file)}")) return;

            try
            {
                File.Delete(file);
                RefreshList();
                UIHelper.ShowSuccess("تم حذف النسخة");
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"خطأ: {ex.Message}");
            }
        }

        // ── تحديث القائمة ──
        private void RefreshList()
        {
            grid.Rows.Clear();
            if (!Directory.Exists(BackupFolder)) return;

            var files = Directory.GetFiles(BackupFolder, "*.bak")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            foreach (var fi in files)
            {
                double sizeMb = fi.Length / 1024.0 / 1024.0;
                grid.Rows.Add(
                    fi.Name,
                    fi.CreationTime.ToString("yyyy/MM/dd  HH:mm"),
                    $"{sizeMb:N1} MB",
                    fi.FullName
                );
            }

            if (files.Count == 0)
            {
                // row بيان
                grid.Rows.Add("لا توجد نسخ احتياطية بعد", "", "", "");
            }
        }

        // ── حذف النسخ القديمة تلقائياً (+30 يوم) ──
        private static void AutoCleanOldBackups()
        {
            if (!Directory.Exists(BackupFolder)) return;
            foreach (var f in Directory.GetFiles(BackupFolder, "*.bak"))
            {
                if (File.GetCreationTime(f) < DateTime.Now.AddDays(-30))
                    try { File.Delete(f); } catch { /* ignore */ }
            }
        }

        private static Button MakeBtn(string text, Color color)
        {
            var b = new Button
            {
                Text = text, BackColor = color, ForeColor = Color.White,
                Size = new Size(160, 38), FlatStyle = FlatStyle.Flat,
                Font = AppTheme.FontNormal, Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(color, 0.08f);
            return b;
        }
    }
}
