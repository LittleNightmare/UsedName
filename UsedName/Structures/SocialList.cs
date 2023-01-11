using System.Runtime.InteropServices;

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
        public byte __padding1;
        public byte __padding2;
        public byte GrandCompanyID;
        public byte Region;
        public byte SelectRegion;
        public byte IsSearchComment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] OnlineStatus;
        //public ulong OnlineStatus;
        //public uint unknown;
        public byte CurrentClassID;
        public byte SelectClassID;
        public ushort CurrentLevel;
        public ushort SelectLevel;
        public byte Identity;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] unknown;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] CharacterName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] FcTag;
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
