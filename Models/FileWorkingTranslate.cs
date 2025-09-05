using AveTranslator.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AveTranslatorM.Models
{
    public class FileWorkingTranslate
    {

        [JsonPropertyOrder(0)]
        public string Game { get; set; }
        [JsonPropertyOrder(1)]
        public string? ModName { get; set; }

        [JsonPropertyOrder(1)]
        public string OrigName { get; set; }

        [JsonPropertyOrder(1)]
        public string? OrigPath { get; set; }

        [JsonPropertyOrder(2)]
        public string TranslatedName { get; set; }

        [JsonPropertyOrder(3)]
        public string InsideGameExportPath { get; set; }

        [JsonPropertyOrder(10)]
        public bool? MultipleFileInPak{ get; set; } 

        [JsonPropertyOrder(99)]
        public string? Template { get; set; }


        [JsonPropertyOrder(20)]
        public List<TranslationEntry> Entries { get; set; } = new List<TranslationEntry>();
    }
}
