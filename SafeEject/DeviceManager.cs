using System;
using System.Collections.Generic;
using System.Management;

namespace SafeEject
{
    internal static class DeviceManager
    {
        public static List<DeviceInfo> GetUsbDisks()
        {
            var result = new List<DeviceInfo>();
            using (var searcher = new ManagementObjectSearcher(
                "SELECT Index,Model,DeviceID,PNPDeviceID,Size,InterfaceType FROM Win32_DiskDrive WHERE InterfaceType='USB'"))
            using (var items = searcher.Get())
            {
                foreach (ManagementObject d in items)
                {
                    var info = new DeviceInfo
                    {
                        Index = Convert.ToInt32(d["Index"] ?? -1),
                        Model = Convert.ToString(d["Model"]) ?? "USB Storage",
                        DeviceId = Convert.ToString(d["PNPDeviceID"]) ?? "",
                        Size = d["Size"] == null ? 0 : Convert.ToInt64(d["Size"]),
                        Removable = true,
                        Letters = ""
                    };

                    using (var parts = d.GetRelated("Win32_DiskPartition"))
                    {
                        foreach (ManagementObject part in parts)
                        using (var vols = part.GetRelated("Win32_LogicalDisk"))
                        {
                            foreach (ManagementObject vol in vols)
                            {
                                var letter = Convert.ToString(vol["DeviceID"]);
                                if (!string.IsNullOrEmpty(letter))
                                    info.Letters += (info.Letters.Length == 0 ? "" : ", ") + letter;
                            }
                        }
                    }
                    result.Add(info);
                }
            }
            return result;
        }
    }
}
