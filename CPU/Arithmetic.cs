using x86Disasm;
namespace x86CS.CPU
{
    public enum ShiftType
    {
        Left,
        ArithmaticLeft,
        Right,
        ArithmaticRight
    }

    public enum RotateType
    {
        LeftWithCarry,
        Left,
        Right,
        RightWithCarry
    }

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
            int overFlow = (int)((dest.Value & ~source.Value & ~result.Value) | (~dest.Value & source.Value & result.Value));
            OF = overFlow < 0;
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
            int overFlow = (int)((dest.Value & source.Value & ~result.Value) | (~dest.Value & ~source.Value & result.Value));
            OF = overFlow < 0;
            CF = result.Value < dest.Value;

            WriteOperand(result);
        }

        [CPUFunction(OpCode = 0x10, Count = 6)]
        [CPUFunction(OpCode = 0x8002)]
        [CPUFunction(OpCode = 0x8102)]
        [CPUFunction(OpCode = 0x8302)]
        public void AddWithCarry(Operand dest, Operand source)
        {
            Operand result = dest;

            if (CF)
                source.Value++;

            result.Value = dest.Value + source.Value;
            SetCPUFlags(result);
            int overFlow = (int)((dest.Value & source.Value & ~result.Value) | (~dest.Value & ~source.Value & result.Value));
            OF = overFlow < 0;
            CF = result.Value < dest.Value;

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
            int overFlow = (int)((dest.Value & 1 & ~result.Value) | (~dest.Value & ~1 & result.Value));
            OF = overFlow < 0;

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
            int overFlow = (int)((dest.Value & ~source.Value & ~result.Value) | (~dest.Value & source.Value & result.Value));
            OF = overFlow < 0;
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

            if (CF)
                source.Value++;

            result.Value = dest.Value - source.Value;
            SetCPUFlags(result);
            int overFlow = (int)((dest.Value & ~source.Value & ~result.Value) | (~dest.Value & source.Value & result.Value));
            OF = overFlow < 0;
            CF = dest.Value < source.Value;

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
            int overFlow = (int)((dest.Value & ~1 & ~result.Value) | (~dest.Value & 1 & result.Value));
            OF = overFlow < 0;

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
            ulong temp = dest.Value * source.Value;
            bool setflags = false;

            switch (source.Size)
            {
                case 8:
                    dest.Value = (byte)temp;
                    if ((dest.Value & 0xff00) == 0)
                        setflags = true;
                    break;
                case 16:
                    dest.Value = (ushort)temp;
                    DX = (ushort)(temp >> 16);
                    if (dest.Value == 0)
                        setflags = true;
                    break;
                default:
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
            ulong temp = dest.Value * source.Value;
            bool setflags = false;

            switch (source.Size)
            {
                case 8:
                    dest.Value = (byte)temp;
                    if ((dest.Value & 0xff00) == 0)
                        setflags = true;
                    break;
                case 16:
                    dest.Value = (ushort)temp;
                    DX = (ushort)(temp >> 16);
                    if (dest.Value == 0)
                        setflags = true;
                    break;
                default:
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

        private byte Rotate(byte dest, byte count, RotateType type)
        {
            return (byte) DoRotate(dest, count, type, 8);
        }

        private ushort Rotate(ushort dest, ushort count, RotateType type)
        {
            return (ushort) DoRotate(dest, count, type, 16);
        }

        private uint Rotate(uint dest, uint count, RotateType type)
        {
            return DoRotate(dest, count, type, 32);
        }

        private uint DoRotate(uint dest, uint count, RotateType type, int size)
        {
            long tempCount;
            bool tempCF;
            uint ret = dest;

            if (type == RotateType.LeftWithCarry || type == RotateType.RightWithCarry)
            {
                if (size == 8)
                    tempCount = ((count & 0x1f)%9);
                else if (size == 16)
                    tempCount = ((count & 0x1f)%17);
                else
                    tempCount = (count & 0x1f);
            }
            else
            {
                tempCount = count%size;
            }
            if (type == RotateType.LeftWithCarry)
            {
                while (tempCount != 0)
                {
                    if (size == 8)
                        tempCF = ((ret & 0x80) == 0x80);
                    else if (size == 16)
                        tempCF = ((ret & 0x8000) == 0x8000);
                    else
                        tempCF = ((ret & 0x80000000) == 0x80000000);

                    ret = (ushort) ((ret*2) + (CF ? 1 : 0));

                    CF = tempCF;
                    tempCount--;
                }

                if (count == 1)
                {
                    if (size == 8)
                        OF = ((ret & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else if (size == 16)
                        OF = ((ret & 0x8000) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((ret & 0x80000000) ^ (CF ? 1 : 0)) != 0;
                }
            }
            else if (type == RotateType.RightWithCarry)
            {
                if (count == 1)
                {
                    if (size == 8)
                        OF = ((dest & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else if (size == 16)
                        OF = ((dest & 0x8000) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((dest & 0x80000000) ^ (CF ? 1 : 0)) != 0;
                }

                while (tempCount != 0)
                {
                    tempCF = ((dest & 0x01) == 0x01);
                    ret /= 2;

                    if (size == 8)
                        ret += (uint) (CF ? 256 : 0);
                    else if (size == 16)
                        ret += (uint) (CF ? 65536 : 0);
                    else
                        ret += (uint) (CF ? 4294967296 : 0);

                    CF = tempCF;

                    tempCount--;
                }
            }
            else if (type == RotateType.Left)
            {
                while (tempCount != 0)
                {
                    if (size == 8)
                        tempCF = ((ret & 0x80) == 0x80);
                    else if (size == 16)
                        tempCF = ((ret & 0x8000) == 0x8000);
                    else
                        tempCF = ((ret & 0x80000000) == 0x80000000);

                    ret = (ushort) ((ret*2) + (CF ? 1 : 0));
                    tempCount--;
                }
                CF = ((ret & 0x01) == 0x01);
                if (count == 1)
                {
                    if (size == 8)
                        OF = ((dest & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else if (size == 16)
                        OF = ((dest & 0x8000) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((dest & 0x80000000) ^ (CF ? 1 : 0)) != 0;
                }
            }
            else
            {
                while (tempCount != 0)
                {
                    tempCF = ((dest & 0x01) == 0x01);
                    ret /= 2;
                    switch (size)
                    {
                        case 8:
                            ret += (uint) (CF ? byte.MaxValue + 1 : 0);
                            break;
                        case 16:
                            ret += (uint) (CF ? ushort.MaxValue + 1 : 0);
                            break;
                        default:
                            ret += (uint) (CF ? 4294967296 : 0);
                            break;
                    }

                    tempCount--;
                }
                switch (size)
                {
                    case 8:
                        CF = ((ret & 0x80) == 0x80);
                        break;
                    case 32:
                        CF = ((ret & 0x8000) == 0x8000);
                        break;
                    default:
                        CF = ((ret & 0x80000000) == 0x80000000);
                        break;
                }

                if (count == 1)
                {
                    switch (size)
                    {
                        case 8:
                            OF = ((((ret & 0x80) == 0x80) ^ ((ret & 0x40) == 0x40)));
                            break;
                        case 16:
                            OF = ((((ret & 0x8000) == 0x8000) ^ ((ret & 0x4000) == 0x4000)));
                            break;
                        default:
                            OF = ((((ret & 0x80000000) == 0x80000000) ^ ((ret & 0x40000000) == 0x40000000)));
                            break;
                    }
                }
            }

            return ret;
        }

        private byte Shift(byte source, byte count, ShiftType type)
        {
            var dest = source;
            var tempCount = (byte) (count & 0x1f);
            var tempDest = dest;

            while (tempCount != 0)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    CF = ((dest & 0x80) == 0x80);
                    dest *= 2;
                }
                else
                {
                    CF = ((dest & 0x01) == 0x01);
                    if (type == ShiftType.ArithmaticRight)
                        dest = (byte) ((sbyte) dest/2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    var tmp = (byte) ((dest & 0x80) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x80) == 0x80);
            }

            return dest;
        }

        private ushort Shift(ushort source, ushort count, ShiftType type)
        {
            var dest = source;
            var tempCount = (ushort) (count & 0x1f);
            var tempDest = dest;

            while (tempCount != 0)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    CF = ((dest & 0x8000) == 0x8000);
                    dest *= 2;
                }
                else
                {
                    CF = ((dest & 0x0001) == 0x0001);
                    if (type == ShiftType.ArithmaticRight)
                        dest = (ushort) ((short) dest/2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    var tmp = (ushort) ((dest & 0x8000) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x8000) == 0x8000);
            }

            return dest;
        }

        private uint Shift(uint source, uint count, ShiftType type)
        {
            var dest = source;
            var tempCount = count & 0x1f;
            var tempDest = dest;

            while (tempCount != 0)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    CF = ((dest & 0x80000000) == 0x8000000);
                    dest *= 2;
                }
                else
                {
                    CF = ((dest & 0x1) == 0x1);
                    if (type == ShiftType.ArithmaticRight)
                        dest = (uint) ((int) dest/2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    var tmp = (uint) ((dest & 0x80000000) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x80000000) == 0x80000000);
            }

            return dest;
        }
    }
}