using AveTranslatorM.Worms.UMH.LanguagePatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveTranslatorM.Worms.UMH.Helpers
{
    internal class WormsUMHConstants
    {
        public const WormsLanguage DefaultChosenLanguage = WormsLanguage.English;
        public const string WormsExeFileName = "WormsMayhem.exe";
        public const string WormsSteamDirRelativePath = @"steamapps\common\WormsXHD";
        public const string BackupDirName = "LanguageToolBackups";
        public const int LanguageBlockOffset = 0x44FC30;
    }
}
