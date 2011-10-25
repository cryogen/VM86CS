using System.Runtime.InteropServices;
using System;

namespace x86CS.ATADevice
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Footer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Cookie;
        public uint Features;
        public uint FormatVersion;
        public ulong DataOffset;
        public uint TimeStamp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CreatorApplication;
        public uint CreatorVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CreatorHostOS;
        public ulong OriginalSize;
        public ulong CurrentSize;
        public ushort Cylinders;
        public byte Heads;
        public byte SectorsPerCylinder;
        public uint Type;
        public uint Checksum;
        public Guid UniqueId;
        public byte SavedState;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 427)]
        public byte[] Reserved;
    }

    [Flags]
    public enum ImageFeatures : uint
    {
        None = 0,
        Temporary = 0x1,
        Reserved = 0x2
    }

    [Flags]
    public enum DiskType : uint
    {
        None,
        Reserved,
        Fixed,
        Dynamic,
        Differencing,
        Reserved2,
        Reserved3
    }
}
