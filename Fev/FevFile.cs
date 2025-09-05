using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveTranslatorM.Fev
{
    public class FevFile
    {
        public string Magic { get; set; }
        public uint DataBlockSize { get; set; } // не FileSize, а blocksize
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }
        public uint Unknown5 { get; set; }
        public uint Unknown6 { get; set; }

        public List<FevSectionHeader> SectionHeaders { get; set; } = new();
    }


    public class FevSectionHeader
    {
        public uint Offset { get; set; }
        public uint Type { get; set; }
    }
}
