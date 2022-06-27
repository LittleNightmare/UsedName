using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace UsedName.Structures
{
    internal class StructureReader
    {
        public enum StructureType { 
            SocialList,
            BlackList
        }
        public static unsafe IDictionary<ulong, string> Read(IntPtr dataPtr, StructureType type)
        {
            var result = new Dictionary<ulong, string>();
            SocialList socialList;
            BlackList blackList;
            switch (type)
            {
                case StructureType.SocialList:
                    socialList = (SocialList)Marshal.PtrToStructure(dataPtr, typeof(SocialList));
#if DEBUG
                    PluginLog.Log($"Read {socialList.entries.Length} entries from SocialList, and type is {socialList.type}");
#endif
                    // type: 1 = Party List; 2 = Friend List; 4 = Player Search; 3=????
                    //if (socialList.type != 2) break;
                    foreach (var entry in socialList.entries)
                    {
                        var name = Encoding.UTF8.GetString(entry.name).TrimEnd('\0');
                        if (string.IsNullOrEmpty(name)) continue;
                        result.Add(entry.contentId, name);
                    }
                    result.Add(0, socialList.type.ToString());
                    break;
                case StructureType.BlackList:
                    // not work, i cannot find any player's name, it is always empty. CN opcode is 0x0250
                    blackList = (BlackList)Marshal.PtrToStructure(dataPtr, typeof(BlackList));
                    foreach (var entry in blackList.entry)
                    {
                        var name = Encoding.UTF8.GetString(entry.name).TrimEnd('\0'); ;
                        if (string.IsNullOrEmpty(name)) continue;
                        result.Add(entry.contentId, name);
                    }
                    break;
                default:
                    return result;
            }
            return result;
        }

        public static unsafe SocialList SocialListRead(IntPtr dataPtr)
        {
            SocialList socialList = new SocialList();
            using (var unmanagedMemoryStream = new UnmanagedMemoryStream((byte*)dataPtr.ToPointer(), 896L))
            {
                using (var binaryReader = new BinaryReader(unmanagedMemoryStream, System.Text.Encoding.UTF8))
                {
                    socialList.padding = binaryReader.ReadUInt32();
                    socialList.padding1 = binaryReader.ReadUInt32();
                    socialList.padding2 = binaryReader.ReadUInt32();
                    socialList.type = binaryReader.ReadByte();
                    socialList.sequence = binaryReader.ReadByte();
                    socialList.padding3 = binaryReader.ReadUInt16();
                    socialList.entries = new PlayerEntry[10];
                    for (var i = 0; i < 10; i++)
                    {
                        var readBuffer = binaryReader.ReadBytes(Marshal.SizeOf(typeof(PlayerEntry)));
                        GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
                        socialList.entries[i] = (PlayerEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PlayerEntry));
                        handle.Free();
                    }
                }
            }


            return socialList;
        }
    }
}
