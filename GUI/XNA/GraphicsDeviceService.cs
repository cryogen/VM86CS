using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using System;

namespace x86CS.GUI.XNA
{
    class GraphicsDeviceService : IGraphicsDeviceService, IGraphicsDeviceManager
    {
        private GraphicsDevice m_graphicsDevice;
        private Control m_renderControl;

        public GraphicsDeviceService(Control control)
        {
            this.m_renderControl = control;
        }

        #region IGraphicsDeviceService Members

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return this.m_graphicsDevice; }
        }

        #endregion

        #region IGraphicsDeviceManager Members

        public bool BeginDraw()
        {
            return false;
        }

        public void CreateDevice()
        {
            PresentationParameters pp = new PresentationParameters();
            pp.IsFullScreen = false;
            pp.BackBufferHeight = this.m_renderControl.Height;
            pp.BackBufferWidth = this.m_renderControl.Width;
            pp.DeviceWindowHandle = this.m_renderControl.Handle;

            this.m_graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);
        }

        public void EndDraw()
        {
        }
        #endregion
    }
}
