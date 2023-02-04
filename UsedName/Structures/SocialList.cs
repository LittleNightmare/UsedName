using System.Runtime.InteropServices;
using System.Text;

namespace UsedName.Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 88, Pack = 1)]
    public unsafe struct PlayerEntry
    {
        public ulong CharacterID;
        // only correct on Friend List
        public uint Timestamp;
        public uint TerritoryID;
        public byte HierarchyStatus;
        public byte HierarchyType;
        public byte HierarchyGroup;
        public byte HierarchyUnk;
        public ushort TerritoryType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] unknown0;
        public byte GrandCompanyID;
        public byte Region;
        public byte SelectRegion;
        public byte IsSearchComment;
        public ushort __padding1;
        public ushort __padding2;
        public long OnlineStatus;
        //public ulong OnlineStatus;
        //public uint unknown;
        public byte CurrentClassID;
        public byte SelectClassID;
        public ushort CurrentLevel;
        public ushort SelectLevel;
        public byte Identity;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] unknown1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] CharacterNameBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] FcTag;

        public string CharacterName
        {
            get => Encoding.UTF8.GetString(CharacterNameBytes).TrimEnd('\0');
        }
    }
    [StructLayout(LayoutKind.Sequential, Size = 896)]
    public struct SocialList
    {   
        public ulong CommunityID;
        public ushort NextIndex;
        public ushort Index;
        public byte ListType;
        public byte RequestKey;
        public byte RequestParam;
        public byte __padding1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public PlayerEntry[] entries;

    }
}
