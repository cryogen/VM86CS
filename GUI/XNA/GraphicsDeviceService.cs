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

        public event EventHandler DeviceCreated;
        public event EventHandler DeviceDisposing;
        public event EventHandler DeviceReset;
        public event EventHandler DeviceResetting;

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
            pp.BackBufferCount = 1;
            pp.BackBufferHeight = this.m_renderControl.Height;
            pp.BackBufferWidth = this.m_renderControl.Width;

            this.m_graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.Hardware,
                                                       this.m_renderControl.Handle, pp);
        }

        public void EndDraw()
        {
        }
        #endregion
    }
}
