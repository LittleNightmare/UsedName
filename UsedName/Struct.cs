using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsedName.Structs;



[StructLayout(LayoutKind.Sequential, Size = 0x380)]
public unsafe struct SocialListResult
{
    public ulong CommunityID;
    public ushort NextIndex;
    public ushort Index;
    public byte ListType;
    public byte RequestKey;
    public byte RequestParam;
    private byte __padding1;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public CharacterEntry[] CharacterEntries;
}
[StructLayout(LayoutKind.Explicit, Size = 0x58, Pack = 1)]
public unsafe struct CharacterEntry
{
    [FieldOffset(0x00)] public ulong CharacterID;
    [FieldOffset(0x08)] public ulong TerritoryID;
    [FieldOffset(0x08)] public ulong TimeStamp;
    [FieldOffset(0x10)] public uint HierarchyID;
    [FieldOffset(0x14)] public ushort TerritoryTypeID;
    [FieldOffset(0x16)] public ushort ContentFinderConditionID;
    [FieldOffset(0x18)] public byte GrandCompanyID;
    [FieldOffset(0x19)] public byte Region;
    [FieldOffset(0x1A)] public byte SelectRegion;
    [FieldOffset(0x1B)] public byte IsSearchComment;
    [FieldOffset(0x20)] public ulong OnlineStatusBytes;
    [FieldOffset(0x28)] public byte CurrentClassID;
    [FieldOffset(0x29)] public byte SelectClassID;
    [FieldOffset(0x2A)] public ushort CurrentLevel;
    [FieldOffset(0x2C)] public ushort SelectLevel;
    [FieldOffset(0x1E)] public byte Identity;
    [FieldOffset(0x31)] private fixed byte CharacterNameBytes[32];
    [FieldOffset(0x51)] private fixed byte FcTagBytes[7];

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

    public ExcelResolver<ClassJob> CurrntClassJob => new(this.CurrentClassID);
    public ExcelResolver<ContentFinderCondition> ContentFinderCondition => new(this.ContentFinderConditionID);
    public ExcelResolver<TerritoryType> TerritoryType
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
                return DateTimeOffset.FromUnixTimeSeconds((long)this.Struct.TimeStamp).LocalDateTime;
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
}