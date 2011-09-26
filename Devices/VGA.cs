using System.Drawing;

namespace x86CS.Devices
{
    public class VGA
    {
        private enum SequenceRegister
        {
            Reset,
            ClockingMode,
            MapMask,
            CharacterMap,
            SequencerMemoryMode
        }

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
                    if(attributeControlFlipFlop)
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
                    if(++currColor == 3)
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

    }
}
