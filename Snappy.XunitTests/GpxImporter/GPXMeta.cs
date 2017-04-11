using System;

namespace Snappy.XunitTests.GpxImporter
{
    public class GPXMeta
    {
        public String Version { get; set; }
        public String Creator { get; set; }
        public String FileName { get; set; }

        public GPXMeta(String creator, String version)
        {
            Version = version;
            Creator = creator;
        }
    }
}
