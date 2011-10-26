using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xc004)]
        [CPUFunction(OpCode = 0xc104)]
        [CPUFunction(OpCode = 0xd004)]
        [CPUFunction(OpCode = 0xd104)]
        [CPUFunction(OpCode = 0xd204)]
        [CPUFunction(OpCode = 0xd304)]
        public void ShiftLeft(Operand dest, Operand source)
        {
            byte count = (byte)(source.Value & 0x1f);
            byte msb = (byte)(dest.Size - count);
            ulong val = dest.Value;

            val = val << count;

            CF = ((val & (ulong)(1 << (byte)(dest.Size))) != 0);
            dest.Value = dest.Value << count;

            if (count == 1)
            {
                if (CF && dest.MSB != 0 || !CF && dest.MSB == 0)
                    OF = true;
                else
                    OF = false;
            }

            SetCPUFlags(dest);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc005)]
        [CPUFunction(OpCode = 0xc105)]
        [CPUFunction(OpCode = 0xd005)]
        [CPUFunction(OpCode = 0xd105)]
        [CPUFunction(OpCode = 0xd205)]
        [CPUFunction(OpCode = 0xd305)]
        public void ShiftRight(Operand dest, Operand source)
        {
            byte count = (byte)(source.Value & 0x1f);

            CF = (((dest.Value) & (1 << count - 1)) == 1);
            dest.Value = dest.Value >> count;

            if (count == 1)
                OF = dest.MSB != 0;

            SetCPUFlags(dest);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc007)]
        [CPUFunction(OpCode = 0xc107)]
        [CPUFunction(OpCode = 0xd007)]
        [CPUFunction(OpCode = 0xd107)]
        [CPUFunction(OpCode = 0xd207)]
        [CPUFunction(OpCode = 0xd307)]
        public void ShiftArithRight(Operand dest, Operand source)
        {
            byte count = (byte)(source.Value & 0x1f);

            CF = (((dest.SignedValue) & (1 << count - 1)) == 1);
            dest.SignedValue = dest.SignedValue >> count;

            if (count == 1)
                OF = dest.MSB != 0;

            SetCPUFlags(dest);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fac)]
        [CPUFunction(OpCode = 0x0fad)]
        public void ShiftRightDP(Operand dest, Operand source, Operand count)
        {
            int c = (int)(count.Value % 32);

            dest.Value = (source.Value << c) | ((uint)(dest.Value >> c));

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc000)]
        [CPUFunction(OpCode = 0xc100)]
        [CPUFunction(OpCode = 0xd000)]
        [CPUFunction(OpCode = 0xd100)]
        [CPUFunction(OpCode = 0xd200)]
        [CPUFunction(OpCode = 0xd300)]
        public void RotateLeft(Operand dest, Operand source)
        {
            byte s = (byte)((dest.Size - source.Value) & (0x1f));
            uint d;

            d = (dest.Value << (byte)(dest.Size - s)) | (dest.Value >> s);
            dest.Value = d;

            SetCPUFlags(dest);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc001)]
        [CPUFunction(OpCode = 0xc101)]
        [CPUFunction(OpCode = 0xd001)]
        [CPUFunction(OpCode = 0xd101)]
        [CPUFunction(OpCode = 0xd201)]
        [CPUFunction(OpCode = 0xd301)]
        public void RotateRight(Operand dest, Operand source)
        {
            byte count = (byte)(source.Value & 0x1f);
            uint tmp = dest.Value;

            dest.Value = ((tmp >> count) | (tmp << (byte)((dest.Size - count))));
            CF = dest.MSB != 0;

            SetCPUFlags(dest);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc003)]
        [CPUFunction(OpCode = 0xc103)]
        [CPUFunction(OpCode = 0xd003)]
        [CPUFunction(OpCode = 0xd103)]
        [CPUFunction(OpCode = 0xd203)]
        [CPUFunction(OpCode = 0xd303)]
        public void RotateCarryRight(Operand dest, Operand source)
        {
            byte tempCount;
            byte tempCF;

            switch (dest.Size)
            {
                case 8:
                    tempCount = (byte)((source.Value & 0x1f) % 9);
                    break;
                case 16:
                    tempCount = (byte)((source.Value & 0x1f) % 17);
                    break;
                default:
                    tempCount = (byte)((source.Value & 0x1f));
                    break;
            }

            if(source.Value == 1)
                OF = (dest.MSB ^ (CF ? 1 : 0)) != 0;

            while (tempCount != 0)
            {
                tempCF = (byte)(dest.Value & 0x1);
                dest.Value = (uint)((dest.Value / 2) + ((CF ? 1 : 0) << (int)source.Size));
                CF = tempCF != 0;
                tempCount--;
            }

            SetCPUFlags(dest);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc002)]
        [CPUFunction(OpCode = 0xc102)]
        [CPUFunction(OpCode = 0xd002)]
        [CPUFunction(OpCode = 0xd102)]
        [CPUFunction(OpCode = 0xd202)]
        [CPUFunction(OpCode = 0xd302)]
        public void RotateCarryLeft(Operand dest, Operand source)
        {
            byte tempCount;
            byte tempCF;

            switch (dest.Size)
            {
                case 8:
                    tempCount = (byte)((source.Value & 0x1f) % 9);
                    break;
                case 16:
                    tempCount = (byte)((source.Value & 0x1f) % 17);
                    break;
                default:
                    tempCount = (byte)((source.Value & 0x1f));
                    break;
            }

            while (tempCount != 0)
            {
                tempCF = dest.MSB;
                dest.Value = (uint)((dest.Value * 2) + (CF ? 1 : 0));
                CF = tempCF != 0;
                tempCount--;
            }

            if (source.Value == 1)
                OF = (dest.MSB ^ (CF ? 1 : 0)) != 0;

            SetCPUFlags(dest);
            WriteOperand(dest);
        }
    }
}
