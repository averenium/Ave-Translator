using AveTranslatorM.Worms.UMH.LanguagePatcher;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveTranslatorM.Worms.UMH.Helpers
{
    public class WormsUMHHelper
    {

        public byte[] ReadLanguageBlock(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                stream.Seek(WormsUMHConstants.LanguageBlockOffset, SeekOrigin.Begin);
                using (var reader = new BinaryReader(stream))
                {
                    return reader.ReadBytes(LanguageBlock.BlockLength);
                }
            }
        }

        public void WriteLanguageBlock(string filePath, byte[] langBlockBytes)
        {
            using (FileStream stream = File.OpenWrite(filePath))
            {
                stream.Seek(WormsUMHConstants.LanguageBlockOffset, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(langBlockBytes);
                }
            }
        }

        public string TryFindWormsPath()
        {
            string path = TryFindWormsSteamPath();

            if (path != null)
            {
                return path;
            }

            return File.Exists(WormsUMHConstants.WormsExeFileName) ? Path.GetFullPath(WormsUMHConstants.WormsExeFileName) : null;
        }

        public string TryFindWormsSteamPath()
        {
            string wormsInstallLocation = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 70600", "InstallLocation", null) as string;

            if (!string.IsNullOrWhiteSpace(wormsInstallLocation))
            {
                string wormsPath = Path.Combine(wormsInstallLocation, WormsUMHConstants.WormsExeFileName);

                if (File.Exists(wormsPath))
                    return wormsPath;
            }

            string steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string ??
                               Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null) as string;

            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                string wormsPath = Path.Combine(steamPath, WormsUMHConstants.WormsSteamDirRelativePath, WormsUMHConstants.WormsExeFileName);

                if (File.Exists(wormsPath))
                    return wormsPath;
            }

            return null;
        }

        public void MakeBackup(string filePath)
        {
            string backupFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Path.GetFileName(filePath)}";
            string backupDirPath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, WormsUMHConstants.BackupDirName);

            Directory.CreateDirectory(backupDirPath);
            File.Copy(filePath, Path.Combine(backupDirPath, backupFileName), true);
        }

        public (bool, string) ExecutePatch(string filePath, WormsLanguage language)
        {
            byte[] langBlockBytes;
            try
            {
                langBlockBytes = ReadLanguageBlock(filePath);
            }
            catch
            {
                return (false, "File read error");
            }

            LanguageBlock langBlock;
            try
            {
                langBlock = new LanguageBlock(langBlockBytes);
            }
            catch
            {
                return (false, "The file is invalid");
            }

            langBlock.SetLanguage(language);

            try
            {
                MakeBackup(filePath);
            }
            catch
            {
                return (false, "Unable to make backup");
            }

            try
            {
                WriteLanguageBlock(filePath, langBlock.GetBytes());
            }
            catch
            {
                return (false, "File write error");
            }

            return (true, "The file has been patched successfully");
        }


    }
}
