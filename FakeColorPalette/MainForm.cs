using System;
using System.Diagnostics;
using System.Security.Policy;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace FakeColorPalette
{
    public partial class MainForm : Form
    {
        // TODO:
        // 1. Get the complimentary color and "negative by hue".
        // 2. Color scheme helper (like https://colorscheme.ru/ or something).
        // 3. Global color picker.
        // 4. Add a toolbar (it would be quite useful to have one).
        // 5. Add more color modes like color wheel, color "rectangle", etc. Not just random.
        // 6. CLEAN THIS UP FOR ONCE.

        // Global color to pick or to place. Just one at a time, I'm too lazy to allow multiple color selection.
        Color SelectedColor { get; set; } = Color.FromKnownColor(KnownColor.Control);

        public MainForm()
        {
            InitializeComponent();
            CreateButtons(7, 5);
        }

        /// <summary>
        /// Creates a rectangle full of buttons of random colors.
        /// </summary>
        /// <param name="x">(int) x - the width of said square (in buttons, 35x35 px).</param>
        /// <param name="y">(int) y - the height of said square (in buttons, 35x35 px).</param>
        private void CreateButtons(int x, int y)
        {
            int buttonWidth = Convert.ToInt32(buttonWidthMTB.Text);
            int buttonHeight = Convert.ToInt32(buttonHeightMTB.Text);

            int counter = 1;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    Button btn = new Button();

                    btn.Text = "Color" + counter;

                    btn.Location = new Point(i * buttonWidth, j * buttonHeight);
                    btn.Height = buttonHeight;
                    btn.Width = buttonWidth;
                    btn.Name = "ColorBtn" + counter;
                    btn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorBtn_MouseUp);

                    this.Controls.Add(btn);
                    counter++;
                }
            }
            this.Refresh();
            RandomizeColors();
        }

        /// <summary>
        /// Destroys the buttons created by "CreateButtons(int x, int y)" method.
        /// </summary>
        private void DestroyButtons()
        {
            foreach (Button btn in this.Controls.OfType<Button>().Where(x => x.Name.Contains("ColorBtn")).ToList<Button>())
            {
                this.Controls.Remove(btn);
                btn.Dispose();
            }

            this.Refresh();
        }

        /// <summary>
        /// Returns the inverted color (negative).
        /// </summary>
        /// <param name="input">(Color) The color needed to be inverted.</param>
        /// <returns></returns>
        private static Color InvertColor(Color input)
        {
            return Color.FromArgb(255 - input.R,
                                  255 - input.G,
                                  255 - input.B);
        }

        /// <summary>
        /// Randomizes the colors for all the buttons. Sets foreground color as random ad background color as negative color of that.
        /// </summary>
        private void RandomizeColors()
        {
            Random rand = new Random();

            foreach (Button btn in this.Controls.OfType<Button>().Where(x => x.Name.Contains("ColorBtn")))
            {
                KnownColor[] colorNames = (KnownColor[])Enum.GetValues(typeof(KnownColor));
                KnownColor randomColor = colorNames[rand.Next(colorNames.Length)];
                Color foreColor = Color.FromKnownColor(randomColor);
                Color backColor = InvertColor(foreColor);

                btn.BackColor = backColor;
                btn.ForeColor = foreColor;
            }
            this.Refresh();
        }

        /// <summary>
        /// Occurs when the "Randomize colors" button is pressed. Invokes the RandomizeColors() method.
        /// </summary>
        /// <param name="sender">Default param (Button).</param>
        /// <param name="e">Default param.</param>
        private void RandomizeColorsButton_Click(object sender, EventArgs e)
        {
            RandomizeColors();
        }

        /// <summary>
        /// Universal handler for MaskedTextBoxes. Whenever the palette or button width or height changes, redraws the buttons accordingly.
        /// </summary>
        /// <param name="sender">Default param (MaskedTextBox).</param>
        /// <param name="e">Default param.</param>
        private void MTB_TextChanged(object sender, EventArgs e)
        {
            DestroyButtons();

            int paletteWidth = Convert.ToInt32(paletteWidthMTB.Text);
            int paletteHeight = Convert.ToInt32(paletteHeightMTB.Text);
            int buttonWidth = Convert.ToInt32(buttonWidthMTB.Text);
            int buttonHeight = Convert.ToInt32(buttonHeightMTB.Text);

            // To fit all the buttons properly:
            // Margin (3 px) * a number of buttons (X) + button width * palette width.
            int requiredWindowWidth = 3 * paletteWidth + buttonWidth * paletteWidth;

            // 150 (fixed button on the bottom) + Margin (3 px) * a number of buttons (Y) + button height * palette height.
            int requiredWindowHeight = 150 + 3 * paletteHeight + buttonHeight * paletteHeight;

            if (this.ClientSize.Width >= requiredWindowWidth &&
                this.ClientSize.Height >= requiredWindowHeight)
            {
                CreateButtons(paletteWidth, paletteHeight);
            }
            else
            {
                MessageBox.Show("You cannot stack this much buttons in this window.");
            }
        }

        /// <summary>
        /// Handles "Left click" as "Take color". Handles "Right click" as "Place color".
        /// </summary>
        /// <param name="sender">Default param (Button).</param>
        /// <param name="e">Default param.</param>
        private void ColorBtn_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender == null)
            {
                throw new ApplicationException("Something went terribly wrong. Please, consider contacting the author of this application at @gCyanide / GitHub");
            }
            Button btn = (Button) sender;

            if (e.Button == MouseButtons.Left)
            {
                SelectedColor = btn.BackColor;
                hexValueTB.Text = "HEX:\r\n" + SelectedColor.Name.ToUpper()[2..8];
                colorDisplayPanel.BackColor = SelectedColor;
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (SelectedColor == Color.FromKnownColor(KnownColor.Control))
                {
                    MessageBox.Show("Please, pick the color first.");
                }
                else
                {
                    btn.BackColor = SelectedColor;
                    btn.ForeColor = InvertColor(SelectedColor);
                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Clears the "YourColor" button and the color display panel.
        /// </summary>
        /// <param name="sender">Default param (Button).</param>
        /// <param name="e">Default param.</param>
        private void clearSavedColorButton_Click(object sender, EventArgs e)
        {
            foreach (Button btn in this.Controls.OfType<Button>().Where(x => x.Name.Contains("yourColor")))
            {
                btn.BackColor = Color.FromArgb(255, 255, 255);
                btn.ForeColor = Color.FromArgb(0, 0, 0);
            }
            colorDisplayPanel.BackColor = Color.FromArgb(255, 255, 255);

            this.Refresh();
        }

        /// <summary>
        /// Opens the link to my GitHub profile if infoLinkLabel is clicked.
        /// </summary>
        /// <param name="sender">Default param (LinkLabel).</param>
        /// <param name="e">Default param.</param>
        private void infoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("https://github.com/gCyanide")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(processInfo);
        }
    }
}