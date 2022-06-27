using System.Runtime.InteropServices;

namespace UsedName.Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 40)]
    struct BlEntry
    {
        public ulong contentId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] name;
    }
    [StructLayout(LayoutKind.Sequential, Size = 808)]
    struct BlackList
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public BlEntry[] entry;
        public byte padding;
        public byte padding1;
        public ushort sequence;
        public uint padding2;

    }
}
