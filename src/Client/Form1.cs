using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            drawThread = new Thread(new ThreadStart(UpdateDraw));
            drawThread.Start();
        }

        Client c = new Client("ogar-luka967.c9users.io:8080");
        bool isClosing;
        Thread drawThread;
        bool spacePressed = false;

        // FPS counter
        int refPerSec = 0;

        private void UpdateDraw()
        {
            while (!isClosing && !this.IsDisposed)
            {
                try
                {
                    this.Invoke((Action)delegate()
                    {
                        if (this.IsDisposed) return;
                        if (c.ShouldUpdate)
                        {
                            this.Refresh();
                        }

                        if (c.MyCells.Count == 0)
                        {
                            label1.Text = "Enter to spawn, " + 
                                "S to spectate. Connected to: " + c.WS.Url.ToString();
                        }
                        else
                        {
                            label1.Text = "Score: " + c.Score.ToString("F0");
                        }
                    });
                }
                catch (Exception) { }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int i = 10;
            try
            {
                // Anti-aliasing
                if (fastRenderToolStripMenuItem.Checked)
                {
                    e.Graphics.SmoothingMode =
                        System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    e.Graphics.PixelOffsetMode =
                        System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
                }
                else
                {
                    e.Graphics.SmoothingMode =
                        System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode =
                        System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                }

                if (c.Zoom != 0)
                {
                    e.Graphics.TranslateTransform(
                        e.ClipRectangle.Width / 2 - c.Xpos * c.Zoom,
                        e.ClipRectangle.Height / 2 - c.Ypos * c.Zoom
                    );
                    e.Graphics.ScaleTransform(c.Zoom, c.Zoom);
                }

                List<Cell> drawingSort = c.VisibleCells.ToList();
                drawingSort.Sort(new Comparison<Cell>(checkCells));
                for (int a = 0; a < drawingSort.Count; a++)
                {
                    Cell cel = drawingSort[a];
                    if (cel == null) continue;

                    cel.DrawMe(e.Graphics, showNamesToolStripMenuItem.Checked,
                        showMassToolStripMenuItem.Checked);
                    i++;
                }

                // Set mouse pos
                float posX = c.Xpos + (-ClientSize.Width / 2 + MousePosition.X) / c.Zoom;
                float posY = c.Ypos + (-ClientSize.Height / 2 + MousePosition.Y) / c.Zoom;
                c.MousePos = new PointF(posX, posY);

                refPerSec++;
            }
            catch (Exception ex)
            {
                
            }
        }

        // Used for comparison
        int checkCells(Cell a, Cell b)
        {
            if (a == null) return -1;
            if (b == null) return 1;
            return a.Radius.CompareTo(b.Radius);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isClosing = true;
            c.WS.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            c.Spawn("Test");
        }

        string lastName = "";

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    c.EmptyPacket(21);
                    break;
                case Keys.Space:
                    c.EmptyPacket(17);
                    spacePressed = true;
                    break;
                case Keys.Q:
                    c.EmptyPacket(18);
                    break;
                case Keys.S:
                    // Spectate
                    c.EmptyPacket(1);
                    break;
                case Keys.Return:
                    NameDialog d = new NameDialog();
                    d.textBox1.Text = lastName;
                    if (d.ShowDialog() == DialogResult.OK)
                    {
                        lastName = d.textBox1.Text;
                        c.Spawn(d.textBox1.Text);
                    }
                    d.Dispose();
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Q:
                    c.EmptyPacket(19);
                    break;
                case Keys.Space:
                    spacePressed = false;
                    break;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            panel1.Location = new Point(
                ClientSize.Width - panel1.Width - 50,
                50
            );
            string a = "";
            foreach (string s in c.Leaderboard)
                a += s + "\n";
            label3.Text = a;
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionDialog f = new ConnectionDialog();
            f.textBox1.Text = c.WS.Url.ToString();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Close current socket and connect to new IP
                c.WS.Close();
                c = new Client(f.textBox1.Text);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        void updateChecked(ToolStripMenuItem item)
        {
            if (item.Checked) item.Checked = false;
            else item.Checked = true;
        }

        private void showMassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateChecked(showMassToolStripMenuItem);
        }

        private void showNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateChecked(showNamesToolStripMenuItem);
        }

        private void darkThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateChecked(darkThemeToolStripMenuItem);
            if (darkThemeToolStripMenuItem.Checked)
            {
                this.BackColor = Color.Black;
                panel1.BackColor = Color.FromArgb(15, 15, 15);
                label1.ForeColor = Color.White;
                label2.ForeColor = Color.White;
                label3.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = Color.White;
                panel1.BackColor = Color.FromArgb(240, 240, 240);
                label1.ForeColor = Color.Black;
                label2.ForeColor = Color.Black;
                label3.ForeColor = Color.Black;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.Show();
        }

        private void fastRenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateChecked(fastRenderToolStripMenuItem);
        }
    }
}
