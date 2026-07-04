using System.Drawing;

namespace ERP.UI.Helpers
{
    public static class AppTheme
    {
        // Primary: Copper/Bronze warm tone
        public static Color Primary       = Color.FromArgb(180, 120, 60);   // copper
        public static Color PrimaryDark   = Color.FromArgb(150, 95, 40);    // copper-dark
        public static Color PrimaryLight  = Color.FromArgb(210, 155, 90);   // copper-light

        // Accent: Warm teal (Odoo-inspired)
        public static Color Accent        = Color.FromArgb(20, 150, 130);   // teal
        public static Color AccentDark    = Color.FromArgb(14, 115, 100);   // teal-dark

        // Status
        public static Color Danger        = Color.FromArgb(210, 65, 65);
        public static Color Warning       = Color.FromArgb(215, 140, 35);
        public static Color Info          = Color.FromArgb(100, 130, 190);
        public static Color Purple        = Color.FromArgb(140, 100, 200);

        // Neutrals — warm-tinted dark
        public static Color Dark          = Color.FromArgb(28, 22, 18);     // very dark brown
        public static Color TextDark      = Color.FromArgb(42, 34, 28);     // dark warm brown
        public static Color TextGray      = Color.FromArgb(120, 105, 90);   // warm gray
        public static Color TextMuted     = Color.FromArgb(170, 155, 135);  // muted warm
        public static Color Light         = Color.FromArgb(245, 240, 232);  // warm cream
        public static Color Surface       = Color.FromArgb(250, 246, 240);  // lighter cream
        public static Color White         = Color.FromArgb(255, 253, 248);  // warm white
        public static Color Border        = Color.FromArgb(210, 198, 182);  // warm border
        public static Color BorderLight   = Color.FromArgb(232, 224, 210);  // subtle border

        // Sidebar — very dark warm brown (like Odoo dark mode)
        public static Color SidebarBg     = Color.FromArgb(30, 24, 18);     // near-black brown
        public static Color SidebarLogo   = Color.FromArgb(22, 17, 13);     // logo bg
        public static Color SidebarHover  = Color.FromArgb(48, 38, 28);     // hover warm
        public static Color SidebarActive = Color.FromArgb(180, 120, 60);   // copper active
        public static Color SidebarText   = Color.FromArgb(210, 195, 175);  // warm off-white
        public static Color SidebarMuted  = Color.FromArgb(120, 105, 90);   // muted
        public static Color SidebarLogout = Color.FromArgb(18, 14, 10);

        // Grid
        public static Color GridHeader    = Color.FromArgb(52, 40, 28);     // dark warm brown header
        public static Color GridAlt       = Color.FromArgb(248, 244, 238);  // warm alt row
        public static Color GridHover     = Color.FromArgb(240, 228, 210);  // warm hover

        // Layout
        public static int SidebarWidth    = 240;
        public static int TopBarHeight    = 56;
        public static int Radius          = 6;

        // Fonts
        public static Font FontTitle   = new Font("Segoe UI", 14, FontStyle.Bold);
        public static Font FontBold    = new Font("Segoe UI", 10, FontStyle.Bold);
        public static Font FontNormal  = new Font("Segoe UI", 10);
        public static Font FontSmall   = new Font("Segoe UI", 9);
        public static Font FontLarge   = new Font("Segoe UI", 24, FontStyle.Bold);
        public static Font FontMedium  = new Font("Segoe UI", 12);
        public static Font FontMenu    = new Font("Segoe UI", 9.5f);
        public static Font FontSection = new Font("Segoe UI", 7.5f, FontStyle.Bold);
    }
}
