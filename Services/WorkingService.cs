using AveTranslatorM.Components.Pages;
using AveTranslatorM.Models;
using Microsoft.Maui.Storage;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;


public class WorkingService
{
    public const string WorkingSubdir = "Working";

    public static readonly string WorkingDir = Path.Combine(Environment.CurrentDirectory, WorkingSubdir);

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
    private readonly GameSelectionService _gameSelectionService;

    public WorkingFile WorkingFile { get; private set; } = new WorkingFile();

    public WorkingGameSettings CurrentGameSettings => 
        WorkingFile.GameSettings.TryGetValue(_gameSelectionService.SelectedGame, out WorkingGameSettings? value)
        ? value : new WorkingGameSettings();

    public bool HasCurrentGameSettings =>
      WorkingFile.GameSettings.ContainsKey(_gameSelectionService.SelectedGame);

    public WorkingService(GameSelectionService gameSelectionService)
    {
        _gameSelectionService = gameSelectionService;
    }

    public async Task ReadWorkingFile()
    {
        try
        {
            Directory.CreateDirectory(WorkingDir);

            var workingJsonPath = Path.Combine(WorkingDir, "Working.json");
            if (File.Exists(workingJsonPath))
            {
                var file = await File.ReadAllBytesAsync(workingJsonPath);
                var workingFile = JsonSerializer.Deserialize<WorkingFile>(file, _jsonOptions );

                if (workingFile != null)
                {
                    WorkingFile = workingFile;
                    if (WorkingFile.LastGameSelected != GameType.Unselected)
                    {
                        _gameSelectionService.SetGame(workingFile.LastGameSelected);
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async Task SaveWorkingFile()
    {
        WorkingFile.LastGameSelected = _gameSelectionService.SelectedGame;
        var workingJsonPath = Path.Combine(WorkingDir, "Working.json");
        var file = JsonSerializer.SerializeToUtf8Bytes(WorkingFile, _jsonOptions);
        await File.WriteAllBytesAsync(workingJsonPath, file);
    }

    public string ShortenWorkingDirPath(int maxLines = 2)
    {
        var path = CurrentGameSettings.WorkingDirectory;
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (parts.Length <= maxLines)
            return path;

        // Показати перший, ..., останній
        return string.Join(Path.DirectorySeparatorChar, parts.Take(1)) +
               Path.DirectorySeparatorChar + "..." + Path.DirectorySeparatorChar +
               string.Join(Path.DirectorySeparatorChar, parts.Skip(parts.Length - (maxLines - 1)));
    }

    public string ShortenPath(string? path, int maxLines = 2)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (parts.Length <= maxLines)
            return path;

        // Показати перший, ..., останній
        return string.Join(Path.DirectorySeparatorChar, parts.Take(1)) +
               Path.DirectorySeparatorChar + "..." + Path.DirectorySeparatorChar +
               string.Join(Path.DirectorySeparatorChar, parts.Skip(parts.Length - (maxLines - 1)));
    }
}