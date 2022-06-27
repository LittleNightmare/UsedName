using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                    foreach (var entry in socialList.entries)
                    {
                        var name = Encoding.UTF8.GetString(entry.name).TrimEnd('\0');
                        if (string.IsNullOrEmpty(name)) continue;
                        result.Add(entry.contentId, name);
                    }
                    break;
                case StructureType.BlackList:
                    blackList = (BlackList)Marshal.PtrToStructure(dataPtr, typeof(BlackList));
                    foreach (var entry in blackList.entry)
                    {
                        var name = Encoding.UTF8.GetString(entry.name).TrimEnd('\0'); ;
                        if (string.IsNullOrEmpty(name)) continue;
                        result.Add(entry.contentId, name);
                    }
                    break;
                default:
                    return null;
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
