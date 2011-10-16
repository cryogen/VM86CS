using System;
using System.Collections.Generic;
using System.Text;

namespace x86Disasm
{
    internal enum ArgumentType
    {
        None,
        Address,
        RegMem,
        RegMemGeneral,
        RegMemMemory,
        RegMemSegment,
        RegMemControl,
        Immediate,
        ImmediateSuppressDefault,
        Relative,
        Offset,
        Memory,
        Constant,
        GeneralRegister,
        SegmentRegister,
    }

    public enum InstructionType
    {
        Prefix,
        Group,
        DataTransfer,
        Arithmetic,
        BCD,
        Logical,
        ShiftRotate,
        BitByte,
        ControlTransfer,
        String,
        InputOutput,
        EnterLeave,
        Flag,
        SegmentRegister,
        Misc,
        System,
        Extended,
        Invalid
    }

    public enum OperandType
    {
        None = 0,
        Register,
        Memory,
        Immediate,
        Address
    }

    public enum RegisterType
    {
        GeneralRegister,
        SegmentRegister,
        ControlRegister
    }

    public enum GeneralRegister
    {
        EAX = 0,
        ECX,
        EDX,
        EBX,
        ESP,
        EBP,
        ESI,
        EDI,
        EIP,
        None
    }

    [Flags]
    public enum OPPrefix
    {
        CSOverride = 0x1,
        DSOverride = 0x2,
        ESOverride = 0x4,
        FSOverride = 0x8,
        GSOverride = 0x10,
        SSOverride = 0x20,
        Repeat = 0x40,
        RepeatNotEqual = 0x80,
        OperandSize = 0x100,
        AddressSize = 0x200,
        Lock=0x400,
    }

    public enum SegmentRegister
    {
        ES = 0,
        CS,
        SS,
        DS,
        FS,
        GS,
        Default
    }

    public enum ControlRegister
    {
        CR0 = 0,
        CR1,
        CR2,
        CR3,
        CR4
    }
}
