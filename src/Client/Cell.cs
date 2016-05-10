using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Client.Properties;

namespace Client
{
    public class Cell
    {
        public uint ID;
        public Color Color;
        public string Nick;
        public PointF Pos;
        public float Radius;
        public float ShownRadius;
        public double ShownMass;
        public bool IsVirus;
        public bool IsMine = false;

        public double GetScore()
        {
            return Math.Pow(this.Radius, 2) / 100;
        }

        public Cell(uint id, Color rgb, string nick, PointF pos,
            float radius, float shownRadius, bool isVirus = false)
        {
            this.ID = id;
            this.Color = rgb;
            this.Nick = nick;
            this.Pos = pos;
            this.Radius = radius;
            this.ShownRadius = shownRadius;
            this.IsVirus = isVirus;
        }

        public void DrawMe(Graphics g, bool seeName, bool seeMass)
        {
            // Draw cell
            if (!this.IsVirus)
            {
                if (this.Radius > 32)
                {
                    // Draw outer part
                    Pen a = new Pen(Color.FromArgb(this.Color.R / 2,
                        this.Color.G / 2, this.Color.B / 2)
                        , 10);
                    g.DrawEllipse(
                        a,
                        new RectangleF(Pos.X - ShownRadius, Pos.Y - ShownRadius,
                            ShownRadius * 2, ShownRadius * 2)
                    );
                    a.Dispose();
                }

                // Draw inner part
                g.FillEllipse(
                    new SolidBrush(this.Color),
                    new RectangleF(Pos.X - ShownRadius, Pos.Y - ShownRadius,
                        ShownRadius * 2, ShownRadius * 2)
                );
            }
            else
            {
                // Draw a virus
                g.DrawImage(
                    Resources.virus,
                    new RectangleF(Pos.X - ShownRadius, Pos.Y - ShownRadius,
                        ShownRadius * 2, ShownRadius * 2)
                );
            }

            // See if name & mass shouldn't be drawn
            if (!seeMass && !seeName) return;

            // Draw text
            float sizeInc = 0.265625F;
            float textSize = this.Radius * sizeInc;

            PointF massPoint;
            SizeF massSize;

            double mass = Math.Ceiling(this.ShownMass);

            // Measure size & name
            Font massFont = new Font("Arial", textSize / 1.5F);
            massSize = g.MeasureString(mass.ToString("F0"), massFont);

            if (seeName)
            {
                massPoint = new PointF(
                    x: this.Pos.X - massSize.Width / 2,
                    y: this.Pos.Y + massSize.Height / 1.3F
                );
            }
            else
            {
                massPoint = new PointF(
                   x: this.Pos.X - massSize.Width / 2,
                   y: this.Pos.Y - massSize.Height / 2
                );
            }

            if (seeMass && this.IsMine && this.Radius > 32)
            {
                // Draw mass
                g.DrawString(
                    mass.ToString(),
                    massFont,
                    Brushes.White,
                    massPoint
                );
            }

            if (seeName && this.Radius > 32)
            {
                Font f = new Font("Arial", textSize);
                SizeF textSz = g.MeasureString(Nick, f);

                // Draw name
                g.DrawString(
                    Nick,
                    f,
                    Brushes.White,
                    new PointF(
                        x: this.Pos.X - textSz.Width / 2,
                        y: this.Pos.Y - textSz.Height / 2
                    )
                );
                f.Dispose();
            }
        }
        public override string ToString()
        {
            return this.Nick + " " + this.Pos.ToString() + " " + this.Radius + " " + this.ID;
        }
    }
}
