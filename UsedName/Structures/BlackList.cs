using System.Runtime.InteropServices;
using System.Text;

namespace UsedName.Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 40)]
    struct BlEntry
    {
        public ulong contentId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] nameBytes;

        public string name
        {
            get => Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');
        }
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
