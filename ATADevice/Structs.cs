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

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DiskHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Cookie;
        public ulong DataOffset;
        public ulong TableOffset;
        public uint HeaderVersion;
        public uint MaxTableEntries;
        public uint BlockSize;
        public uint Checksum;
        public Guid ParentID;
        public uint ParentTimestamp;
        public uint Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ParentUnicodeName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator5;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator6;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator7;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] ParentLocator8;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] Reserved2;
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
