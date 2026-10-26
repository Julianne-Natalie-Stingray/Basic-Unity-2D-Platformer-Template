using NUnit.Framework;
using UnityEngine;

public sealed class TimeScaleManagerTests
{
    private GameObject first;
    private GameObject second;

    [SetUp]
    public void SetUp()
    {
        TimeScaleManager.ResetAll();
        first = new GameObject("time-scale-first");
        second = new GameObject("time-scale-second");
    }

    [TearDown]
    public void TearDown()
    {
        TimeScaleManager.ResetAll();
        Object.DestroyImmediate(first);
        Object.DestroyImmediate(second);
    }

    [Test]
    public void PauseOverridesSpeed()
    {
        TimeScaleManager.PushPause(first);
        TimeScaleManager.SetSpeed(first, 2f);

        Assert.IsTrue(TimeScaleManager.IsPaused);
        Assert.AreEqual(0f, Time.timeScale, 0.0001f);

        TimeScaleManager.PopPause(first);

        Assert.IsFalse(TimeScaleManager.IsPaused);
        Assert.AreEqual(2f, Time.timeScale, 0.0001f);
    }

    [Test]
    public void TwoOwnersRequireBothToRelease()
    {
        TimeScaleManager.PushPause(first);
        TimeScaleManager.PushPause(second);

        TimeScaleManager.PopPause(first);
        Assert.IsTrue(TimeScaleManager.IsPaused);
        Assert.AreEqual(0f, Time.timeScale, 0.0001f);

        TimeScaleManager.PopPause(second);
        Assert.IsFalse(TimeScaleManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale, 0.0001f);
    }

    [Test]
    public void SpeedRequestsMultiply()
    {
        TimeScaleManager.SetSpeed(first, 2f);
        TimeScaleManager.SetSpeed(second, 0.5f);

        Assert.AreEqual(1f, Time.timeScale, 0.0001f);

        TimeScaleManager.ClearSpeed(first);
        Assert.AreEqual(0.5f, Time.timeScale, 0.0001f);
    }

    [Test]
    public void DestroyedOwnerIsPurged()
    {
        var owner = new GameObject("time-scale-destroyed");
        TimeScaleManager.PushPause(owner);
        Object.DestroyImmediate(owner);

        Assert.IsFalse(TimeScaleManager.IsPaused);
        Assert.AreEqual(1f, Time.timeScale, 0.0001f);
    }

    [Test]
    public void ResetAllRestoresScale()
    {
        TimeScaleManager.PushPause(first);
        TimeScaleManager.SetSpeed(second, 2f);

        TimeScaleManager.ResetAll();

        Assert.AreEqual(1f, Time.timeScale, 0.0001f);
        Assert.IsFalse(TimeScaleManager.IsPaused);
    }
}
