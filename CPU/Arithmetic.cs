using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x38, Count = 6)]
        [CPUFunction(OpCode = 0x8007)]
        [CPUFunction(OpCode = 0x8107)]
        [CPUFunction(OpCode = 0x8307)]
        public void Compare(Operand dest, Operand source)
        {
            Operand result = dest;

            result.Value = dest.Value - source.Value;
            SetCPUFlags(result);

            if ((source.SignedValue > 0 && dest.SignedValue < dest.SignedMin + source.SignedValue) ||
                (source.SignedValue < 0 && dest.SignedValue > dest.SignedMax + source.SignedValue))
                OF = true;
            else
                OF = false;

            CF = dest.Value < source.Value;
        }

        [CPUFunction(OpCode = 0x00, Count = 6)]
        [CPUFunction(OpCode = 0x8000)]
        [CPUFunction(OpCode = 0x8100)]
        [CPUFunction(OpCode = 0x8300)]
        public void Add(Operand dest, Operand source)
        {
            Operand result = dest;

            result.Value = dest.Value + source.Value;
            SetCPUFlags(result);
            CF = result.Value < dest.Value;
            if (((source.SignedValue > 0) && (dest.SignedValue > (dest.SignedMax - source.SignedValue))) || 
                ((source.SignedValue < 0) && (dest.SignedValue < (dest.SignedMin - source.SignedValue))))
                OF = true;
            else
                OF = false;
            
            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0x10, Count = 6)]
        [CPUFunction(OpCode = 0x8002)]
        [CPUFunction(OpCode = 0x8102)]
        [CPUFunction(OpCode = 0x8302)]
        public void AddWithCarry(Operand dest, Operand source)
        {
            Operand result = dest;
            ulong tmp;

            tmp = dest.Value + source.Value;
            if (CF)
                tmp++;

            result.Value = (uint)tmp;
            SetCPUFlags(result);
            if (((source.SignedValue > 0) && (dest.SignedValue > (dest.SignedMax - source.SignedValue))) ||
                ((source.SignedValue < 0) && (dest.SignedValue < (dest.SignedMin - source.SignedValue))))
                OF = true;
            else
                OF = false;
            CF = result.Value < tmp;

            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0x40, Count = 8)]
        [CPUFunction(OpCode = 0xfe00)]
        [CPUFunction(OpCode = 0xff00)]
        public void Increment(Operand dest)
        {
            Operand result = dest;

            result.Value++;
            SetCPUFlags(result);
            if (((1 > 0) && (dest.SignedValue > (dest.SignedMax - 1))))
                OF = true;
            else
                OF = false;

            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0x28, Count = 6)]
        [CPUFunction(OpCode = 0x8005)]
        [CPUFunction(OpCode = 0x8105)]
        [CPUFunction(OpCode = 0x8305)]
        public void Subtract(Operand dest, Operand source)
        {
            Operand result = dest;

            result.Value = dest.Value - source.Value;
            SetCPUFlags(result);

            if ((source.SignedValue > 0 && dest.SignedValue < dest.SignedMin + source.SignedValue) ||
                (source.SignedValue < 0 && dest.SignedValue > dest.SignedMax + source.SignedValue))
                OF = true;
            else
                OF = false;
            CF = dest.Value < source.Value;

            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0x18, Count = 6)]
        [CPUFunction(OpCode = 0x8003)]
        [CPUFunction(OpCode = 0x8103)]
        [CPUFunction(OpCode = 0x8303)]
        public void SubtractWithBorrow(Operand dest, Operand source)
        {
            Operand result = dest;
            bool oldCF = CF;

            result.Value = dest.Value - source.Value;
            if (CF)
                result.Value--;

            SetCPUFlags(result);

            if (oldCF)
                CF = dest.Value < (source.Value + 1);
            else
                CF = dest.Value < source.Value;

            if (oldCF)
                source.Value++;

            if ((source.SignedValue > 0 && dest.SignedValue < dest.SignedMin + source.SignedValue) ||
                (source.SignedValue < 0 && dest.SignedValue > dest.SignedMax + source.SignedValue))
                OF = true;
            else
                OF = false;

            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0x48, Count = 8)]
        [CPUFunction(OpCode = 0xfe01)]
        [CPUFunction(OpCode = 0xff01)]
        public void Decrement(Operand dest)
        {
            Operand result = dest;

            result.Value--;
            SetCPUFlags(result);
            if ((1 > 0 && dest.SignedValue < dest.SignedMin + 1))
                OF = true;
            else
                OF = false;

            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0xf607)]
        [CPUFunction(OpCode = 0xf707)]
        public void SignedDivide(Operand dest, Operand source)
        {
            ulong dividend;
            uint result, remainder;

            switch (source.Size)
            {
                case 8:
                    dividend = AX;
                    break;
                case 16:
                    dividend = (ulong)((DX << 16) + AX);
                    break;
                default:
                    dividend = (ulong)((EDX << 32) + EAX);
                    break;
            }

            result = (uint)(dividend / source.Value);
            remainder = (uint)(dividend % source.Value);

            switch (source.Size)
            {
                case 8:
                    AL = (byte)result;
                    AH = (byte)remainder;
                    break;
                case 16:
                    AX = (ushort)result;
                    DX = (ushort)remainder;
                    break;
                default:
                    EAX = result;
                    EDX = remainder;
                    break;
            }
        }

        [CPUFunction(OpCode = 0xf606)]
        [CPUFunction(OpCode = 0xf706)]
        public void UnSignedDivide(Operand dest, Operand source)
        {
            ulong dividend;
            uint result, remainder;

            switch (source.Size)
            {
                case 8:
                    dividend = AX;
                    break;
                case 16:
                    dividend = (ulong)((DX << 16) + AX);
                    break;
                default:
                    dividend = (ulong)((EDX << 32) + EAX);
                    break;
            }

            result = (uint)(dividend / source.Value);
            remainder = (uint)(dividend % source.Value);

            switch (source.Size)
            {
                case 8:
                    AL = (byte)result;
                    AH = (byte)remainder;
                    break;
                case 16:
                    AX = (ushort)result;
                    DX = (ushort)remainder;
                    break;
                default:
                    EAX = result;
                    EDX = remainder;
                    break;
            }
        }

        [CPUFunction(OpCode = 0xf604)]
        [CPUFunction(OpCode = 0xf704)]
        public void UnsignedMultiply(Operand dest, Operand source)
        {
            ulong temp;
            bool setflags = false;

            switch (source.Size)
            {
                case 8:
                    temp = AL * source.Value;
                    dest.Value = (uint)temp;
                    if ((dest.Value & 0xff00) == 0)
                        setflags = true;
                    break;
                case 16:
                    temp = AX * source.Value;
                    dest.Value = (ushort)temp;
                    DX = (ushort)(temp >> 16);
                    if (dest.Value == 0)
                        setflags = true;
                    break;
                default:
                    temp = EAX * source.Value;
                    dest.Value = (uint)temp;
                    EDX = (uint)(temp >> 32);
                    if (dest.Value == 0)
                        setflags = true;
                    break;
            }

            if (setflags)
            {
                OF = CF = true;
            }

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xf605)]
        [CPUFunction(OpCode = 0xf705)]
        public void SignedMultiply(Operand dest, Operand source)
        {
            long temp;
            bool setflags = false;

            switch (source.Size)
            {
                case 8:
                    temp = AL * source.SignedValue;
                    dest.SignedValue = (int)temp;
                    if ((dest.Value & 0xff00) == 0)
                        setflags = true;
                    break;
                case 16:
                    temp = (short)AX * source.SignedValue;
                    dest.SignedValue = (int)temp;
                    DX = (ushort)(short)(temp >> 16);
                    if (dest.Value == 0)
                        setflags = true;
                    break;
                default:
                    temp = (int)EAX * source.SignedValue;
                    dest.SignedValue = (int)temp;
                    EDX = (uint)(temp >> 32);
                    if (dest.Value == 0)
                        setflags = true;
                    break;
            }

            if (setflags)
            {
                OF = CF = true;
            }

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0faf)]
        public void SignedMultiply2(Operand dest, Operand source)
        {
            long temp;
            bool setflags = false;

            switch (source.Size)
            {
                case 8:
                    temp = dest.SignedValue * source.SignedValue;
                    dest.SignedValue = (int)temp;
                    if (temp > 0xff)
                        setflags = true;
                    break;
                case 16:
                    temp = dest.SignedValue * source.SignedValue;
                    dest.SignedValue = (int)temp;
                    if (temp > 0xffff)
                        setflags = true;
                    break;
                default:
                    temp = dest.SignedValue * source.SignedValue;
                    dest.SignedValue = (int)temp;
                    if (temp > 0xffffffff)
                        setflags = true;
                    break;
            }

            if (setflags)
            {
                OF = CF = true;
            }

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x69)]
        [CPUFunction(OpCode = 0x6b)]
        public void SignedMultiply(Operand dest, Operand source, Operand source2)
        {
            long temp;
            bool setflags = false;

            switch (source.Size)
            {
                case 8:
                    temp = source.SignedValue * source2.SignedValue;
                    dest.SignedValue = (int)temp;
                    if (temp > 0xff)
                        setflags = true;
                    break;
                case 16:
                    temp = source.SignedValue * source2.SignedValue;
                    dest.SignedValue = (int)temp;
                    if (temp > 0xffff)
                        setflags = true;
                    break;
                default:
                    temp = source.SignedValue * source2.SignedValue;
                    dest.SignedValue = (int)temp;
                    if (temp > 0xffffffff)
                        setflags = true;
                    break;
            }

            if (setflags)
            {
                OF = CF = true;
            }

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xf602)]
        [CPUFunction(OpCode = 0xf702)]
        public void Not(Operand dest)
        {
            dest.Value = ~dest.Value;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xf603)]
        [CPUFunction(OpCode = 0xf703)]
        public void Neg(Operand dest)
        {
            if (dest.Value == 0)
                CF = false;
            else
                CF = true;

            dest.Value = (uint)-dest.Value;

            WriteOperand(dest);
        }
    }
}