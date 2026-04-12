using System.Drawing;
using System.Windows.Forms;

namespace AnonymousEmotionDiary.Views
{
    internal static class UiTheme
    {
        public static readonly Color Background = Color.FromArgb(245, 241, 232);
        public static readonly Color Surface = Color.FromArgb(255, 252, 246);
        public static readonly Color SurfaceStrong = Color.White;
        public static readonly Color Accent = Color.FromArgb(209, 122, 34);
        public static readonly Color AccentMuted = Color.FromArgb(245, 217, 187);
        public static readonly Color Secondary = Color.FromArgb(40, 103, 130);
        public static readonly Color Danger = Color.FromArgb(175, 56, 42);
        public static readonly Color Success = Color.FromArgb(52, 116, 88);
        public static readonly Color Border = Color.FromArgb(228, 220, 206);
        public static readonly Color TextPrimary = Color.FromArgb(53, 44, 37);
        public static readonly Color TextMuted = Color.FromArgb(116, 105, 96);

        public static Font TitleFont(float size = 22f)
        {
            return new Font("Microsoft YaHei UI", size, FontStyle.Bold);
        }

        public static Font BodyFont(float size = 10.5f, FontStyle style = FontStyle.Regular)
        {
            return new Font("Microsoft YaHei UI", size, style);
        }

        public static Panel CreateCard(int x, int y, int width, int height)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = SurfaceStrong,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static void StylePrimaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Accent;
            button.ForeColor = Color.White;
            button.Font = BodyFont(10.5f, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
            button.BackColor = SurfaceStrong;
            button.ForeColor = TextPrimary;
            button.Font = BodyFont(10.5f, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        public static void StyleDangerButton(Button button)
        {
            StylePrimaryButton(button);
            button.BackColor = Danger;
        }

        public static void StyleTextBox(TextBox textBox, bool multiline = false)
        {
            textBox.Font = BodyFont();
            textBox.BackColor = Color.White;
            textBox.ForeColor = TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            if (multiline)
            {
                textBox.Multiline = true;
            }
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = SurfaceStrong;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Secondary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = BodyFont(10f, FontStyle.Bold);
            grid.DefaultCellStyle.BackColor = SurfaceStrong;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = AccentMuted;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.Font = BodyFont();
            grid.RowTemplate.Height = 34;
            grid.GridColor = Border;
        }
    }
}
