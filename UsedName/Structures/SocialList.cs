using System.Runtime.InteropServices;

namespace UsedName.Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 88)]
    public struct PlayerEntry
    {
        public ulong contentId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] bytes;
        public ushort zoneId;
        public ushort zoneId1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] bytes1;
        public ulong onlineStatusMask;
        public byte classJob;
        public byte padding;
        public byte level;
        public byte padding1;
        public ushort padding2;
        public byte one;
        // crash when UnmanagedType.LPUTF8Str
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] fcTag;

    }
    [StructLayout(LayoutKind.Sequential, Size = 896)]
    public struct SocialList
    {   
        public uint padding;
        public uint padding1;
        public uint padding2;
        public byte type;
        public byte sequence;
        public ushort padding3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public PlayerEntry[] entries;

    }
}
