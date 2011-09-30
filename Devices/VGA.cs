using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace x86CS.Devices
{
    public class VGA : IDevice, INeedsClock, INeedsMMIO
    {
        private enum SequenceRegister
        {
            Reset,
            ClockingMode,
            MapMask,
            CharacterMap,
            SequencerMemoryMode
        }

        private readonly int[] portsUsed = {
                                               0x3b4, 0x3b5, 0x3ba, 0x3c0, 0x3c1, 0x3c2, 0x3c4, 0x3c5, 0x3c7, 0x3c8, 0x3c9,
                                               0x3ca, 0x3cc, 0x3d4, 0x3d5, 0x3da
                                           };

        private readonly MemoryMapRegion[] memoryMap;
        private readonly byte[] sequencer;
        private readonly Color[] dacPalette;
        private readonly byte[] dacColour;
        private readonly byte[] attributeControl;
        private readonly byte[] crtControl;
        private byte miscOutputRegister;
        private byte featureControl;
        private SequenceRegister sequencerAddress;
        private byte dacAddress;
        private byte attributeControlAddress;
        private byte crtControlAddress;
        private byte currColor;
        private bool attributeControlFlipFlop;

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public MemoryMapRegion[] MemoryMap
        {
            get { return memoryMap; }
        }

        public VGA()
        {
            sequencer = new byte[5];
            dacPalette = new Color[256];
            dacColour = new byte[3];
            dacAddress = 0;
            attributeControlFlipFlop = false;
            attributeControl = new byte[0x15];
            crtControl = new byte[0x19];
        }

        public void Cycle(double frequency, ulong tickCount)
        {
            
        }

        public ushort Read(ushort addr)
        {
            switch (addr)
            {
                case 0x3da:
                    attributeControlFlipFlop = false;
                    return 0;
                case 0x3ba:
                case 0x3c2:
                    return 0;
                case 0x3ca:
                    return featureControl;
                case 0x3cc:
                    return miscOutputRegister;
                default:
                    break;
            }

            return 0;
        }

        public void Write(ushort addr, ushort value)
        {
            switch (addr)
            {
                case 0x3b4:
                case 0x3d4:
                    crtControlAddress = (byte)value;
                    break;
                case 0x3b5:
                case 0x3d5:
                    crtControl[crtControlAddress] = (byte)value;
                    break;
                case 0x3ba:
                case 0x3da:
                    featureControl = (byte)value;
                    break;
                case 0x3c2:
                    miscOutputRegister = (byte)value;
                    break;
                case 0x3c4:
                    sequencerAddress = (SequenceRegister)value;
                    break;
                case 0x3c5:
                    sequencer[(int)sequencerAddress] = (byte)value;
                    break;
                case 0x3c0:
                    if (attributeControlFlipFlop)
                    {
                        attributeControl[attributeControlAddress] = (byte)value;
                        attributeControlFlipFlop = false;
                    }
                    else
                    {
                        attributeControlAddress = (byte)value;
                        attributeControlFlipFlop = true;
                    }
                    break;
                case 0x3c8:
                    dacAddress = (byte)value;
                    currColor = 0;
                    break;
                case 0x3c9:
                    dacColour[currColor] = (byte)(value & 0x3f);
                    if (++currColor == 3)
                    {
                        currColor = 0;
                        dacPalette[dacAddress] = Color.FromArgb(dacColour[0], dacColour[1], dacColour[2]);
                        dacAddress++;
                    }
                    break;
                default:
                    break;
            }
        }

        public void GDIDraw(Graphics g)
        {
            var screenBitmap = new Bitmap(720, 420, PixelFormat.Format32bppRgb);

            var fontBuffer = new byte[0x1000];
            var displayBuffer = new byte[0xfa0];

            Memory.BlockRead(0xa0000, fontBuffer, fontBuffer.Length);
            Memory.BlockRead(0xb8000, displayBuffer, displayBuffer.Length);

            for (var i = 0; i < displayBuffer.Length; i += 2)
            {
                int currChar = displayBuffer[i];
                int fontOffset = currChar * 32;
                byte attribute = displayBuffer[i + 1];
                int y = i / 160 * 16;

                Color foreColour = dacPalette[attribute & 0xf];
                Color backColour = dacPalette[(attribute >> 4) & 0xf];

                for (var f = fontOffset; f < fontOffset + 16; f++)
                {
                    int x = ((i % 160) / 2) * 8;

                    for (var j = 7; j > 0; j--)
                    {
                        screenBitmap.SetPixel(x++, y, ((fontBuffer[f] >> j) & 0x1) != 0 ? foreColour : backColour);
                    }
                    y++;
                }
            }

            g.DrawImage(screenBitmap, 0, 0);
        }
    }
}