using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using System.Data;
using System.Drawing;

namespace Client
{
    public class Client
    {
        public WebSocket WS { get; set; }
        public List<Cell> VisibleCells { get; protected set; }
        public List<uint> MyCells { get; protected set; }
        public List<string> Leaderboard { get; protected set; }
        public float Xpos = 0, Ypos = 0, Zoom = 0;
        private float _zoom;
        public double Score = 0;
        public PointF MousePos { get; set; }
        public volatile bool ShouldUpdate = false;

        public Client(string connectTo)
        {
            // Connect
            WS = new WebSocket("ws://" + connectTo);
            WS.Connect();

            VisibleCells = new List<Cell>();
            MyCells = new List<uint>();
            Leaderboard = new List<string>();
            ShouldUpdate = false;

            WS.OnMessage += new EventHandler<MessageEventArgs>(WS_OnMessage);
        }

        void WS_OnMessage(object sender, MessageEventArgs e)
        {
            byte[] data = e.RawData;

            switch (data[0])
            {
                case 16:
                    int offset = 1;
                    // Update nodes packet

                    // Eat events
                    uint deaths = Packet.ReadUint16(data, ref offset);

                    for (uint i = 0; i < deaths; i++)
                    {
                        uint eaterID = Packet.ReadUint32(data, ref offset);
                        uint eatenID = Packet.ReadUint32(data, ref offset);

                        int index = VisibleCells.FindIndex((v) => v.ID == eatenID);

                        if (index != -1)
                            VisibleCells.RemoveAt(index);
                    }

                    // Main events
                    while (true)
                    {
                        uint ID = Packet.ReadUint32(data, ref offset);

                        if (ID == 0) break;

                        int posX = Packet.Readint32(data, ref offset);
                        int posY = Packet.Readint32(data, ref offset);
                        short size = Packet.Readint16(data, ref offset);

                        int colorR = Packet.ReadUint8(data, ref offset);
                        int colorG = Packet.ReadUint8(data, ref offset);
                        int colorB = Packet.ReadUint8(data, ref offset);

                        var opt = Packet.ReadUint8(data, ref offset);
                        bool isVirus = (opt & 1) != 0;

                        // Reserved for future use?
                        if ((opt & 2) != 0)
                            offset += 4;

                        if ((opt & 4) != 0)
                            offset += 8;

                        if ((opt & 8) != 0)
                            offset += 16;

                        string nick = "";
                        while (true)
                        {
                            ushort ch = Packet.ReadUint16(data, ref offset);
                            if (ch == 0)
                                break;
                            nick += (char)ch;
                        }

                        // Got all the info
                        // Check if it's visible or not
                        int index = VisibleCells.FindIndex((v) => v.ID == ID);
                        if (index != -1)
                        {
                            float shSz = this.VisibleCells[index].ShownRadius;
                            double shM = this.VisibleCells[index].ShownMass;
                            
                            // Update it
                            this.VisibleCells[index] = new Cell(
                                ID,
                                Color.FromArgb(colorR, colorG, colorB),
                                nick,
                                new PointF(posX, posY),
                                size,
                                shSz,
                                isVirus
                            )
                            {
                                ShownMass = shM
                            };
                        }
                        else
                        {
                            this.VisibleCells.Add(new Cell(
                                ID,
                                Color.FromArgb(colorR, colorG, colorB),
                                nick,
                                new PointF(posX, posY),
                                size,
                                size,
                                isVirus
                            ));
                        }
                    }

                    // Disappearance
                    uint disappears = Packet.ReadUint32(data, ref offset);

                    for (int i = 0; i < disappears; i++)
                    {
                        uint ID = Packet.ReadUint32(data, ref offset);

                        int index = VisibleCells.FindIndex((v) => v.ID == ID);
                        if (index != -1)
                        {
                            this.VisibleCells.RemoveAt(index);
                        }
                    }

                    // First animate cells
                    this.UpdateAnimation();
                    // Update my position
                    this.UpdatePosition();
                    // Send info about my mouse position
                    this.SendMouseInfo();
                    // Update has come, signal the form to invalidate
                    this.ShouldUpdate = true;
                    break;

                case 32:
                    // A cell is mine
                    offset = 1;
                    this.MyCells.Add(Packet.ReadUint32(data, ref offset));
                    break;

                case 17:
                    // Spectate location change
                    offset = 1;
                    this.Xpos = Packet.ReadFloat(data, ref offset);
                    this.Ypos = Packet.ReadFloat(data, ref offset);
                    this.Zoom = Packet.ReadFloat(data, ref offset);
                    break;
                case 49:
                    // Leaderboard change
                    offset = 1;
                    uint lbLength = Packet.ReadUint32(data, ref offset);
                    this.Leaderboard.Clear();

                    for (int i = 0; i < lbLength; i++)
                    {
                        // Read strings
                        string user = "";
                        while (true)
                        {
                            ushort chr = Packet.ReadUint16(data, ref offset);
                            if (chr == 0) break;

                            user += (char)chr;
                        }

                        // Add user
                        this.Leaderboard.Add((i + 1) + ". " + user);
                    }
                    break;
                    
            }
        }

        public void SendMouseInfo()
        {
            if (this.MyCells.Count == 0) return;

            List<byte> packet = new List<byte>() { 16 }; // Packet id
            packet.AddRange(BitConverter.GetBytes((int)MousePos.X));
            packet.AddRange(BitConverter.GetBytes((int)MousePos.Y));
            packet.AddRange(BitConverter.GetBytes((uint)0));
            this.WS.Send(packet.ToArray());
        }

        public void EmptyPacket(byte id)
        {
            this.WS.Send(new byte[] { id });
        }

        public void UpdatePosition()
        {
            try
            {
                if (this.MyCells.Count == 0) return;
                float x = 0, y = 0;
                double score = 0;
                int count = 0;

                foreach (var ID in this.MyCells.ToList())
                {
                    int index = VisibleCells.FindIndex((v) => v.ID == ID);
                    if (index != -1)
                    {
                        this.VisibleCells[index].IsMine = true;
                        x += this.VisibleCells[index].Pos.X;
                        y += this.VisibleCells[index].Pos.Y;
                        score += Math.Ceiling(this.VisibleCells[index].GetScore());
                        count++;
                    }
                    else
                    {
                        // Not visible here
                        this.MyCells.Remove(ID);
                    }
                }

                x /= count;
                y /= count;
                this.Xpos = x;
                this.Ypos = y;

                // Calculate zoom
                double specZoom = Math.Sqrt(100 * score);
                specZoom = Math.Pow(Math.Min(40.5 / specZoom, 1.0), 0.4) * 0.8;
                specZoom *= 1D / (1D + (this.MyCells.Count / 10D - 0.1D) / 2D);
                this._zoom = (float)specZoom;

                this.Score = score;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        public void UpdateAnimation()
        {
            // Animate zoom, radius and cell mass
            Zoom += (_zoom - Zoom) / 8;
            foreach (Cell c in this.VisibleCells)
            {
                double mass = Math.Ceiling(Math.Pow(c.Radius, 2) / 100);
                c.ShownRadius += (c.Radius - c.ShownRadius) / 5;
                c.ShownMass += (mass - c.ShownMass) / 3;
            }
        }

        public void Spawn(string nick)
        {
            List<byte> packet = new List<byte>() { 0 }; // Packet id
            packet.AddRange(Encoding.Unicode.GetBytes(nick + "\0").AsEnumerable());
            this.WS.Send(packet.ToArray());
        }
    }
}
