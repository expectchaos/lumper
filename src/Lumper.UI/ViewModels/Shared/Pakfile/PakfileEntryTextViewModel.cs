namespace Lumper.UI.ViewModels.Shared.Pakfile;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lumper.Lib.Bsp;
using Lumper.Lib.Bsp.Struct;
using Lumper.UI.Views.Shared.Pakfile;
using NLog;
using ReactiveUI.Fody.Helpers;

public class PakfileEntryTextViewModel : PakfileEntryViewModel
{
    [Reactive]
    public string? Content { get; set; } = "";

    [Reactive]
    public bool IsContentLoaded { get; set; }

    private static readonly string[] KnownFileTypes = [".txt", ".vbsp", ".vmt"];

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public PakfileEntryTextViewModel(PakfileEntry entry, BspNode parent)
        : base(entry, parent) => RegisterView<PakfileEntryTextViewModel, PakfileEntryTextView>();

    public override void Load(CancellationTokenSource? cts = null)
    {
        if (!IsContentLoaded && KnownFileTypes.Contains(Extension))
            LoadContent();
    }

    public void LoadContent()
    {
        try
        {
            Content = BspFile.Encoding.GetString(GetData());
        }
        catch
        {
            LogManager.GetCurrentClassLogger().Warn("Failed to load pakfile entry!");
            Content = null;
        }

        IsContentLoaded = true;
    }

    public async Task OpenExternal()
    {
        string fileName = Path.Combine(Path.GetTempPath(), $"{Name}-{Guid.NewGuid()}{Extension}");

        try
        {
            await using var fileStream = new FileStream(fileName, FileMode.CreateNew);
            fileStream.Write(GetData());
            fileStream.Flush();
            await Program.MainWindow.Launcher.LaunchUriAsync(new Uri(fileName));
        }
        catch (IOException ex)
        {
            Logger.Error(ex, "Failed to create temporary file");
        }
    }

    public override void UpdateModel()
    {
        if (!IsModified)
            return;

        UpdateData(BspFile.Encoding.GetBytes(Content ?? ""));
    }
}
