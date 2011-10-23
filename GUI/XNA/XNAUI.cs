using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System;
using x86CS.Devices;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Collections.Generic;

namespace x86CS.GUI.XNA
{
    public class XNAUI : UI
    {
        private GraphicsDeviceService graphicsService;
        private Control renderControl;
        private SpriteBatch sprites;
        private KeyboardState oldKeyboardState;

        public XNAUI(Form UIForm, VGA device)
            : base(UIForm, device)
        {
            Panel panel = new Panel();

            panel.Location = new System.Drawing.Point(0, 0);
            panel.ClientSize = UIForm.ClientSize;
            UIForm.Controls.Add(panel);
            renderControl = panel;
            oldKeyboardState = Keyboard.GetState();
        }

        public override void Init()
        {
            graphicsService = new GraphicsDeviceService(renderControl);
            graphicsService.CreateDevice();
            sprites = new SpriteBatch(graphicsService.GraphicsDevice);
        }

        private void ProcessInput()
        {
            KeyboardState currentState = Keyboard.GetState();

            Microsoft.Xna.Framework.Input.Keys[] pressedKeys = currentState.GetPressedKeys();
            Microsoft.Xna.Framework.Input.Keys[] oldKeys = oldKeyboardState.GetPressedKeys();

            IEnumerable<Microsoft.Xna.Framework.Input.Keys> nowDown, nowUp;

            nowDown = pressedKeys.Except(oldKeys);
            nowUp = oldKeys.Except(pressedKeys);

            foreach (Microsoft.Xna.Framework.Input.Keys key in nowDown)
            {
                uint scanCode = NativeMethods.MapVirtualKeyEx((uint)key, NativeMethods.MAPVK_VK_TO_VSC, InputLanguage.CurrentInputLanguage.Handle);
                OnKeyDown(scanCode);
            }

            foreach (Microsoft.Xna.Framework.Input.Keys key in nowUp)
            {
                uint scanCode = NativeMethods.MapVirtualKeyEx((uint)key, NativeMethods.MAPVK_VK_TO_VSC, InputLanguage.CurrentInputLanguage.Handle);
                OnKeyUp(scanCode);
            }

            oldKeyboardState = currentState;
        }

        public override void Cycle()
        {
            GraphicsDevice device = graphicsService.GraphicsDevice;

            ProcessInput();

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
    }
}
