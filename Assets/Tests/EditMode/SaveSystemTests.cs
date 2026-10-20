using System;
using System.IO;
using NUnit.Framework;

/// <summary>
/// SaveSystem / GameProgressContributor 的 EditMode 测试.
/// 归属: Tests (EditMode).
/// 不负责: 场景扫描与 MonoBehaviour 生命周期, 那些由 SaveService 承担.
/// </summary>
public sealed class SaveSystemTests
{
    private string directory;
    private string filePath;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(
            Path.GetTempPath(),
            "Basic2DPlatformerTests",
            Guid.NewGuid().ToString("N"));

        filePath = Path.Combine(directory, "GameSave.json");
        GameProgress.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        GameProgress.Reset();

        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }

    [Test]
    public void SaveThenLoad_RestoresContributorSnapshot()
    {
        FakeContributor contributor = new("level", "{\"value\":1}");
        SaveSystem system = CreateSystem(contributor);

        SaveResult result = system.Save();
        Assert.That(result.Succeeded, Is.True, result.Error);
        Assert.That(File.Exists(filePath), Is.True);

        FakeContributor restored = new("level");
        SaveSystem loaded = CreateSystem(restored);

        Assert.That(loaded.TryLoad(), Is.True);
        Assert.That(loaded.PendingCount, Is.EqualTo(1));
        Assert.That(loaded.TryRestore(restored), Is.True);
        Assert.That(restored.RestoredJson, Is.EqualTo("{\"value\":1}"));
        Assert.That(loaded.PendingCount, Is.Zero);
    }

    [Test]
    public void Save_OverwritesExistingFileAtomically()
    {
        FakeContributor contributor = new("progress", "first");
        SaveSystem system = CreateSystem(contributor);

        Assert.That(system.Save().Succeeded, Is.True);

        contributor.Snapshot = "second";
        Assert.That(system.Save().Succeeded, Is.True);

        FakeContributor restored = new("progress");
        SaveSystem loaded = CreateSystem(restored);
        Assert.That(loaded.TryLoad(), Is.True);
        Assert.That(loaded.TryRestore(restored), Is.True);
        Assert.That(restored.RestoredJson, Is.EqualTo("second"));
    }

    [Test]
    public void Save_WhenAnyContributorCannotCapture_DoesNotOverwriteExistingSave()
    {
        FakeContributor first = new("progress", "first");
        SaveSystem system = CreateSystem(first);
        Assert.That(system.Save().Succeeded, Is.True);

        first.CanCaptureValue = false;
        SaveResult failed = system.Save();

        Assert.That(failed.Succeeded, Is.False);

        FakeContributor restored = new("progress");
        SaveSystem loaded = CreateSystem(restored);
        Assert.That(loaded.TryLoad(), Is.True);
        Assert.That(loaded.TryRestore(restored), Is.True);
        Assert.That(restored.RestoredJson, Is.EqualTo("first"));
    }

    [Test]
    public void Save_DuplicateKeys_FailsWithoutWriting()
    {
        SaveSystem system = new(new GameSaveStore(filePath), new[]
        {
            new FakeContributor("same")
        });

        SaveResult result = system.Save(() => new[]
        {
            new FakeContributor("same")
        });

        Assert.That(result.Succeeded, Is.False);
        Assert.That(File.Exists(filePath), Is.False);
    }

    [Test]
    public void NewGame_DeletesSaveAndPendingSections()
    {
        SaveSystem system = CreateSystem(new FakeContributor("progress", "first"));
        Assert.That(system.Save().Succeeded, Is.True);
        Assert.That(system.TryLoad(), Is.True);
        Assert.That(system.PendingCount, Is.EqualTo(1));

        system.NewGame();

        Assert.That(system.HasSave, Is.False);
        Assert.That(system.PendingCount, Is.Zero);
        Assert.That(File.Exists(filePath), Is.False);
    }

    [Test]
    public void Restore_WithUnknownKey_KeepsPendingForLaterScene()
    {
        SaveSystem system = CreateSystem(new FakeContributor("level-a", "{\"open\":true}"));
        Assert.That(system.Save().Succeeded, Is.True);

        SaveSystem loaded = CreateSystem(new FakeContributor("level-b"));
        Assert.That(loaded.TryLoad(), Is.True);

        FakeContributor other = new("level-b");
        Assert.That(loaded.TryRestore(other), Is.False);
        Assert.That(loaded.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void LevelCompletedEvent_FiresWhenNotified()
    {
        int count = 0;
        Action handler = () => count++;

        GameProgress.LevelCompleted += handler;
        GameProgress.NotifyLevelCompleted();
        GameProgress.LevelCompleted -= handler;

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void ProgressRestore_ClampsCompletedLevelsToLevelCount()
    {
        GameProgress.Store.Restore(99, " level-a ");

        Assert.That(GameProgress.Store.CompletedLevels, Is.EqualTo(LevelProgressStore.LevelCount));
        Assert.That(GameProgress.Store.LastLevelId, Is.EqualTo("level-a"));
    }

    [Test]
    public void ProgressContributor_RoundTripsCompletedLevelsAndLastLevelId()
    {
        GameProgress.Store.CompleteLevel(1);
        GameProgress.Store.SetLastLevel("level-a");

        string json = GameProgressContributor.Instance.Capture();

        GameProgress.Reset();
        GameProgressContributor.Instance.Restore(json);

        Assert.That(GameProgress.Store.CompletedLevels, Is.EqualTo(1));
        Assert.That(GameProgress.Store.LastLevelId, Is.EqualTo("level-a"));
    }

    private SaveSystem CreateSystem(params ISaveContributor[] contributors)
        => new(new GameSaveStore(filePath), contributors);

    private sealed class FakeContributor : ISaveContributor
    {
        public string Key { get; }

        public bool CanCaptureValue { get; set; } = true;

        public string Snapshot { get; set; }

        public string RestoredJson { get; private set; }

        public FakeContributor(string key, string snapshot = "{}")
        {
            Key = key;
            Snapshot = snapshot;
        }

        public bool CanCapture()
            => CanCaptureValue;

        public string Capture()
            => Snapshot;

        public void Restore(string json)
            => RestoredJson = json;
    }
}
