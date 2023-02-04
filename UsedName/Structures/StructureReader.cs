using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace UsedName.Structures
{
    internal class StructureReader
    {
        public enum StructureType { 
            SocialList,
            BlackList
        }
        public static unsafe IDictionary<ulong, string> Read <T>(byte[] data)
            where T : struct
        {
            var result = new Dictionary<ulong, string>();
            T list;
            // transfer data to InPtr
            int size = Marshal.SizeOf(typeof(T));
            IntPtr buffer = buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(data, 0, buffer, size);
                list = (T)Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            
            // Adjust data
            switch (list)
            {
                case SocialList socialList:
#if DEBUG
                    PluginLog.LogDebug($"Read {socialList.entries.Length} entries from SocialList, and type is {socialList.ListType}");;
#endif
                    // type: 1 = Party List; 2 = Friend List; 4 = Player Search; 3=????
                    //if (socialList.ListType != 2) break;
                    foreach (var entry in socialList.entries)
                    {
                        var name = entry.CharacterName;
                        // some player would not get name. some are Unable to Retrieve, some from other world. are they really save on local? 
                        if (string.IsNullOrEmpty(name)) 
                        {
#if DEBUG
                            // entry.bytes is only different part. it seems to consist of 3 parts. At the middle 4 bytes always 00-00-00-00
                            PluginLog.LogDebug($"name IsNullOrEmpty {entry.CharacterID}:{name}\n");
#endif
                            continue;
                        }
                        
                        if(!result.TryAdd(entry.CharacterID, name))
                        {
                            PluginLog.LogWarning($"Duplicate entry {entry.CharacterID} {name}");
                        }
#if DEBUG
                        PluginLog.LogDebug($"{name}:{String.Join(", ", GetOnlineStatus(entry.OnlineStatus))}");
                        PluginLog.Debug($"{entry.__padding2}");
#endif
                    }
                    result.Add(0, socialList.ListType.ToString());
                    break;
                case BlackList blackList:
                    // not work, i cannot find any player's name, it is always empty. CN opcode is 0x0250
                    foreach (var entry in blackList.entry)
                    {
                        if (string.IsNullOrEmpty(entry.name)) continue;
                        result.Add(entry.contentId, entry.name);
                    }
                    break;
                default:
                    return result;
            }
            return result;
        }

        public static List<string>? GetOnlineStatus(long onlineStatus)
        {

            BitArray os = new BitArray(BitConverter.GetBytes(onlineStatus));
            var result = Service.DataManager.GetExcelSheet<OnlineStatus>()?
                .Where(s => os.Get((int)s.RowId))
                .OrderBy(s => s.Priority)
                .Select(s => s.Name.ToString());
            
            return result.ToList();
        }

        //public static unsafe SocialList SocialListRead(IntPtr dataPtr)
        //{
        //    SocialList socialList = new SocialList();
        //    using (var unmanagedMemoryStream = new UnmanagedMemoryStream((byte*)dataPtr.ToPointer(), 896L))
        //    {
        //        using (var binaryReader = new BinaryReader(unmanagedMemoryStream, System.Text.Encoding.UTF8))
        //        {
        //            socialList.padding = binaryReader.ReadUInt32();
        //            socialList.padding1 = binaryReader.ReadUInt32();
        //            socialList.padding2 = binaryReader.ReadUInt32();
        //            socialList.type = binaryReader.ReadByte();
        //            socialList.sequence = binaryReader.ReadByte();
        //            socialList.padding3 = binaryReader.ReadUInt16();
        //            socialList.entries = new PlayerEntry[10];
        //            for (var i = 0; i < 10; i++)
        //            {
        //                var readBuffer = binaryReader.ReadBytes(Marshal.SizeOf(typeof(PlayerEntry)));
        //                GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
        //                socialList.entries[i] = (PlayerEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PlayerEntry));
        //                handle.Free();
        //            }
        //        }
        //    }


        //    return socialList;
        //}
    }
}
