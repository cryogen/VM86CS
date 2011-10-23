using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using x86CS.Devices;
using Microsoft.Xna.Framework.Input;
namespace x86CS.GUI.XNA
{
    public class XNAUI : UI
    {
        private GraphicsDeviceService graphicsService;
        private Control renderControl;
        private SpriteBatch sprites;
        private VGA vgaDevice;

        private bool AppStillIdle
        {
            get
            {
                Message msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        public XNAUI(Form UIForm, VGA device)
            : base(UIForm)
        {
            Panel panel = new Panel();

            panel.Location = new System.Drawing.Point(0, 0);
            panel.ClientSize = UIForm.ClientSize;
            UIForm.Controls.Add(panel);
            renderControl = panel;
            vgaDevice = device;
        }

        public override void Init()
        {
            Application.Idle += new System.EventHandler(ApplicationIdle);
            graphicsService = new GraphicsDeviceService(renderControl);
            graphicsService.CreateDevice();
            sprites = new SpriteBatch(graphicsService.GraphicsDevice);
        }

        void ApplicationIdle(object sender, System.EventArgs e)
        {
            while (AppStillIdle)
            {
                Cycle();
            }
        }

        public override void Cycle()
        {
            GraphicsDevice device = graphicsService.GraphicsDevice;

            device.Clear(Color.Black);

            Texture2D screenBitmap = new Texture2D(device, renderControl.ClientSize.Width, renderControl.ClientSize.Height, 1, TextureUsage.Linear, SurfaceFormat.Color);

            var fontBuffer = new byte[0x2000];
            var displayBuffer = new byte[0xfa0];
            Color[] data = new Color[screenBitmap.Width * screenBitmap.Height];

            Memory.BlockRead(0xa0000, fontBuffer, fontBuffer.Length);
            Memory.BlockRead(0xb8000, displayBuffer, displayBuffer.Length);

            for (var i = 0; i < displayBuffer.Length; i += 2)
            {
                int currChar = displayBuffer[i];
                int fontOffset = currChar * 32;
                byte attribute = displayBuffer[i + 1];
                int y = i / 160 * 16;

                System.Drawing.Color fore, back;

                fore = vgaDevice.GetColour(attribute & 0xf);
                back = vgaDevice.GetColour((attribute >> 4) & 0xf);
                Color foreColour = new Color(fore.R, fore.G, fore.B);
                Color backColour = new Color(back.R, back.G, back.B);

                for (var f = fontOffset; f < fontOffset + 16; f++)
                {
                    int x = ((i % 160) / 2) * 8;

                    for (var j = 7; j >= 0; j--)
                    {
                        if (((fontBuffer[f] >> j) & 0x1) != 0)
                            data[y * screenBitmap.Width + x] = foreColour;
                        else
                            data[y * screenBitmap.Width + x] = backColour;
                        x++;
                    }
                    y++;
                }
            }

            screenBitmap.SetData<Color>(data);

            sprites.Begin();
            sprites.Draw(screenBitmap, new Vector2(0, 0), Color.White);
            sprites.End();

            device.Present();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hWnd;
            public IntPtr msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
    }
}
