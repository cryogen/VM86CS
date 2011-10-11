namespace x86Disasm
{
    public delegate uint ReadCallback(uint offset, int size);
    public delegate void CPUCallbackNoargs();
    public delegate void CPUCallback1args(Operand dest);
    public delegate void CPUCallback2args(Operand dest, Operand source);
    public delegate void CPUCallback3args(Operand dest, Operand source, Operand source2);
}
