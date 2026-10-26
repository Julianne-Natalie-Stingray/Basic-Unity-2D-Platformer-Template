using System;
using NUnit.Framework;
using UnityEngine;

public sealed class LevelManagerTests
{
    private GameObject levelObject;
    private GameObject goalObject;
    private LevelManager manager;
    private TestGoal goal;

    [SetUp]
    public void SetUp()
    {
        levelObject = new GameObject("level");
        goalObject = new GameObject("goal");

        goal = goalObject.AddComponent<TestGoal>();
        manager = levelObject.AddComponent<LevelManager>();

        typeof(LevelManager)
            .GetField("rootCondition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, goal);

        typeof(LevelManager)
            .GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(manager, null);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(levelObject);
        UnityEngine.Object.DestroyImmediate(goalObject);
    }

    [Test]
    public void NotifyFailed_FiresOnce()
    {
        int failed = 0;
        manager.LevelFailed += () => failed++;

        manager.NotifyFailed();
        manager.NotifyFailed();

        Assert.AreEqual(1, failed);
        Assert.IsTrue(manager.IsFailed);
        Assert.IsFalse(manager.IsCleared);
        Assert.IsTrue(manager.IsResolved);
    }

    [Test]
    public void ClearedThenFailed_KeepsCleared()
    {
        int cleared = 0;
        int failed = 0;
        manager.LevelCleared += () => cleared++;
        manager.LevelFailed += () => failed++;

        goal.Fulfill();
        manager.NotifyFailed();

        Assert.AreEqual(1, cleared);
        Assert.AreEqual(0, failed);
        Assert.IsTrue(manager.IsCleared);
        Assert.IsFalse(manager.IsFailed);
    }

    [Test]
    public void FailedThenCleared_KeepsFailed()
    {
        int cleared = 0;
        int failed = 0;
        manager.LevelCleared += () => cleared++;
        manager.LevelFailed += () => failed++;

        manager.NotifyFailed();
        goal.Fulfill();

        Assert.AreEqual(1, failed);
        Assert.AreEqual(0, cleared);
        Assert.IsTrue(manager.IsFailed);
        Assert.IsFalse(manager.IsCleared);
    }

    private sealed class TestGoal : MonoBehaviour, ILevelGoal
    {
        public bool Fulfilled { get; private set; }

        public event Action Changed;

        public void Fulfill()
        {
            Fulfilled = true;
            Changed?.Invoke();
        }
    }
}
