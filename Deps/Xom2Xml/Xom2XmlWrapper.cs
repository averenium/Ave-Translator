using AveTranslatorM.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AveTranslatorM.Deps.Xom2Xml;

public class Xom2XmlWrapper
{
    public const string Xom2XmlExePath  = @".\Deps\Xom2Xml\Xom2Xml.exe";
    public static string DefaultSchemePath = @$"{Directory.GetCurrentDirectory()}\Deps\Xom2Xml\XOMSCHM\WUM\XOMSCHM.dat";
    public string SchemePath { get; set; } = DefaultSchemePath;

    public Xom2XmlWrapper() { }

    /// <summary>
    /// Конвертує XML у XOM.
    /// </summary>
    public int ConvertXmlToXom(string xmlPath, string xomOutPath, LogUserOutput? logStream = null, string? workingDirectory = null)
    {
        var args = $"\"{xmlPath}\" -out \"{xomOutPath}\" -l -schm \"{SchemePath}\"";
        return RunProcess(args, logStream, workingDirectory);
    }

    /// <summary>
    /// Конвертує XOM у XML.
    /// </summary>
    public int ConvertXomToXml(string xomPath, string xmlOutPath, LogUserOutput? logStream = null)
    {
        var args = $"\"{xomPath}\" -out \"{xmlOutPath}\" -l -schm \"{SchemePath}\"";
        return RunProcess(args, logStream);
    }

    /// <summary>
    /// Універсальний запуск з довільними аргументами.
    /// </summary>
    public int Run(string inputFile, string outputFile, bool isXmlToXom, LogUserOutput? logStream = null, string? scheme = null, bool exportXid = false, string? ximgFormat = null, string? ximgDir = null)
    {
        var args = new StringBuilder();
        args.Append($"\"{inputFile}\" -out \"{outputFile}\" -l");
        args.Append($" -schm \"{scheme ?? SchemePath}\"");
        if (exportXid) args.Append(" -id");
        if (!string.IsNullOrWhiteSpace(ximgFormat)) args.Append($" -ximg-file {ximgFormat}");
        if (!string.IsNullOrWhiteSpace(ximgDir)) args.Append($" -ximg-dir \"{ximgDir}\"");
        return RunProcess(args.ToString(), logStream);
    }

    public int RunProcess(string args, LogUserOutput? logBuilder, string? workingDirectory = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = Xom2XmlExePath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (workingDirectory != null)
            psi.WorkingDirectory = workingDirectory;

        using var process = Process.Start(psi);

        if (logBuilder != null)
        {
            // Читання потоків у реальному часі
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                if (line != null)
                    logBuilder.Add(line);
            }
            while (!process.StandardError.EndOfStream)
            {
                var line = process.StandardError.ReadLine();
                if (line != null)
                    logBuilder.Add("[ERR] " + line);
            }
        }

        process.WaitForExit();
        return process.ExitCode;
    }
}
