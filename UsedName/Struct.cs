using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsedName.Structs;

public enum ListType : byte
{
    PartyList = 1,
    FriendList = 2,
    LinkShell = 3,
    PlayerSearch = 4,
    [Display(ShortName = "MembersOnline")]
    MembersOnlineAndOnHomeWorld = 5,
    CompanyMember = 6,
    ApplicationOfCompany = 7,
    Mentor = 10,
    NewAdventurer = 11
}

[StructLayout(LayoutKind.Explicit, Size = 0x420)]
public unsafe struct SocialListResult
{
    [FieldOffset(0x00)] public ulong CommunityID;
    [FieldOffset(0x08)] public ushort NextIndex;
    [FieldOffset(0x0A)] public ushort Index;
    [FieldOffset(0x0C)] public byte ListTypeByte;
    [FieldOffset(0x0D)] public byte RequestKey;
    [FieldOffset(0x0E)] public byte RequestParam;
    [FieldOffset(0x0F)] private byte __padding1;
    [FieldOffset(0x10)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public CharacterEntry[] CharacterEntries;

    public ListType? ListType
    {
        get
        {
            if (Enum.IsDefined(typeof(ListType), ListTypeByte))
            {
                return (ListType)ListTypeByte;
            }
            return null;
        }
        set {}
    }
}
[StructLayout(LayoutKind.Explicit, Size = 0x68, Pack = 1)]
public unsafe struct CharacterEntry
{
    [FieldOffset(0x00)] public ulong CharacterID;
    // [FieldOffset(0x08)] public uint Timestamp;
    // [FieldOffset(0x0C)] public uint TerritoryID;
    // [FieldOffset(0x10)] public byte HierarchyStatus;
    // [FieldOffset(0x11)] public byte HierarchyType;
    // [FieldOffset(0x12)] public byte HierarchyGroup;
    // [FieldOffset(0x13)] public byte IsDeleted;
    // [FieldOffset(0x14)] public ushort TerritoryTypeID;
    // [FieldOffset(0x16)] public byte GrandCompanyID;
    // [FieldOffset(0x17)] public byte Region;
    // [FieldOffset(0x18)] public byte SelectRegion;
    // [FieldOffset(0x19)] public byte IsSearchComment;
    // [FieldOffset(0x1A)] public byte __padding1;
    // [FieldOffset(0x1B)] public byte __padding2;
    // [FieldOffset(0x1C)] public ulong OnlineStatusBytes;
    // [FieldOffset(0x24)] public byte CurrentClassID;
    // [FieldOffset(0x25)] public byte SelectClassID;
    // [FieldOffset(0x26)] public ushort CurrentLevel;
    // [FieldOffset(0x28)] public ushort SelectLevel;
    // [FieldOffset(0x2A)] public byte Identity;
    [FieldOffset(0x3C)] private fixed byte CharacterNameBytes[32];
    [FieldOffset(0x5C)] private fixed byte FcTagBytes[7];

    public unsafe string CharacterName
    {
        get
        {
            fixed (byte* ptr = this.CharacterNameBytes) {
                return Encoding.UTF8.GetString(ptr, 31).TrimEnd('\0');
            }
        }
        set { }
    }

    public string FcTag
    {
        get
        {
            fixed (byte* ptr = this.CharacterNameBytes) {
                return Encoding.UTF8.GetString(ptr, 6).TrimEnd('\0');
            }
        }
        set { }
    }
    /*
        public ExcelResolver<ClassJob> CurrntClassJob => new(this.CurrentClassID);
        // public ExcelResolver<ContentFinderCondition> ContentFinderCondition => new(this.ContentFinderConditionID);
        public ExcelResolver<TerritoryType>? TerritoryType
        {
            get
            {
                if (this.TerritoryTypeID != 0) {
                    return new ExcelResolver<TerritoryType>(this.TerritoryTypeID);
                }
                else {
                    return null;
                }
            }
            set { }
        }

        public List<ExcelResolver<OnlineStatus>> OnlineStatus
        {
            get
            {
                BitArray os = new BitArray(BitConverter.GetBytes(this.OnlineStatusBytes));
                var list = new List<ExcelResolver<OnlineStatus>>();
                foreach (var i in os)
                {
                    list.Add(new ExcelResolver<OnlineStatus>((uint)i));
                }
                return list;
            }
            set { }
        }
    }
    public sealed class Character
    {
        protected internal CharacterEntry Struct;
        public byte ListType;
        internal Character(CharacterEntry entry, byte listType)
        {
            this.Struct = entry;
            this.ListType = listType;
        }

        public DateTime? LocalDateTime
        {
            get
            {
                if (this.ListType == 6 || this.ListType == 2) {
                    //PluginLog.Log($"{this.Struct.TimeStamp}");
                    return DateTimeOffset.FromUnixTimeSeconds((uint)this.Struct.Timestamp).LocalDateTime;
                    //return null;
                }
                else return null;
            }
            set { }
        }
        public unsafe void DumpBytes()
        {
            var mem = stackalloc byte[0x58];
            Marshal.StructureToPtr(this.Struct, (IntPtr)mem, false);
            Util.DumpMemory((IntPtr)mem, 0x58);
        }
    */
}
