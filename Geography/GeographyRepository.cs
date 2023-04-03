using GeographyModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class GeographyRepository : GeographyRouter.IGeoRepository
{
    readonly Action<string> LogAction;
    public GeographyRepository(Action<string> logAction)
    {
        LogAction = logAction;
        Log("Created");
    }

    Dictionary<string, Layer> _layers = new Dictionary<string, Layer>();
    Dictionary<string, LayerElement> _elements = new Dictionary<string, LayerElement>();
    Dictionary<string, Dictionary<string, LayerElement>> _elementsByLayerCode = new Dictionary<string, Dictionary<string, LayerElement>>();
    LayerElementsMatrix ElecricalMatrix;


    protected void Log(string message) => LogAction?.Invoke(message);

    IGeographyRepositoryStorage Storage;
    private void Save(Layer item) => Storage?.Save(item);
    private void Delete(Layer item) => Storage?.Delete(item);
    private void DeleteAllLayers() => Storage?.DeleteAllLayers();
    private void Save(LayerElement item) => Storage?.Save(item);
    private void Delete(LayerElement item) => Storage?.Delete(item);
    private void WaitFlush() => Storage?.WaitFlush();

    public void BeginInitial()
    {
        _layers.Clear();
        _elementsByLayerCode.Clear();
        ElecricalMatrix = new LayerElementsMatrixByPoint(GetElement);
        //---------------------
        _elements.Clear();
        //---------------------
        version = 0;
        versionChangeRequestStopwatch.Restart();
        //---------------------
        Storage = null;
    }
    public void EndInitial(IGeographyRepositoryStorage storage)
    {
        Storage = storage;
        AfterInitialFinished();
    }

    protected virtual void AfterInitialFinished() { }

    #region Version
    long version = 0;
    long versionChangeRequestd = 0;
    Stopwatch versionChangeRequestStopwatch = Stopwatch.StartNew();

    public long Version => ReadByLock(() => version);
    public string VersionAsTimeText => $"{new DateTime(Version):yyyy-MM-dd HH:mm:ss.fff}";
    private void updateVersion(long versionRequestd, bool log = true)
    {
        var newVersion = Math.Max(version, versionRequestd);
        versionChangeRequestd = versionRequestd;
        versionChangeRequestStopwatch.Restart();
        if (version == newVersion) return;
        version = newVersion;
        if (log) Log($"Repository Version Changed: {version} == {new DateTime(version):yyyy-MM-dd HH:mm:ss.fff}");
    }
    public void GetVersion(out long version, out long versionRequested, out long changeElapsedMilliseconds)
    {
        long versionMiror = 0;
        long versionRequestedMiror = 0;
        long changeElapsedMillisecondsMiror = 0;
        ReadByLock(() =>
        {
            versionMiror = this.version;
            versionRequestedMiror = versionChangeRequestd;
            changeElapsedMillisecondsMiror = versionChangeRequestStopwatch.ElapsedMilliseconds;
        });
        version = versionMiror;
        versionRequested = versionRequestedMiror;
        changeElapsedMilliseconds = changeElapsedMillisecondsMiror;
    }
    #endregion Version

    const string StructureLockedErrorMessage = "اطلاعات المان‌ها ثبت شده و امکان تغییر ساختاری وجود ندارد!";

    public bool StructureLocked => ReadByLock(() => isStructureLocked);
    private bool isStructureLocked => _elements.Count > 0;
}
