namespace SafeEject
{
    public sealed class DeviceInfo
    {
        public int Index { get; set; }
        public string Model { get; set; }
        public string DeviceId { get; set; }
        public string Letters { get; set; }
        public long Size { get; set; }
        public bool Removable { get; set; }

        public override string ToString()
        {
            var gb = Size > 0 ? (Size / 1024d / 1024d / 1024d).ToString("0.##") + " GB" : "容量未知";
            var letters = string.IsNullOrEmpty(Letters) ? "无盘符" : Letters;
            return string.Format("{0}  |  {1}  |  {2}", letters, Model, gb);
        }
    }
}
