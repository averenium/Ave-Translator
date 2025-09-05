
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AveTranslatorM.Models
{
    public class WorkingFile
    {
        public GameType LastGameSelected { get; set; }

        public GptSettings GptSettings { get; set; } = new GptSettings();
        
        public Dictionary<GameType, WorkingGameSettings> GameSettings { get; set; } = new()
        {
            { GameType.WormsUMH, new WorkingGameSettings() },
            { GameType.KCD2, new WorkingGameSettings() }
        };
    }

    public class WorkingGameSettings
    {
        public string LastWorkingFile { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public string GamePath { get; set; } = string.Empty;
        public string ExportTemplate { get; set; } = string.Empty;
        public string GptQuery { get; set; } = "Переклади Українською, залиш спецсимволи та розмір строки, без смайлів";
    }

    public class GptSettings
    {
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-4o";

        public int MaxTokens { get; set; } = 1000;

    }
}
