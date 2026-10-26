using GrignardReagent.LayeredId;
using NUnit.Framework;

public sealed class AudioIdTests
{
    [Test]
    public void Equality_IsOrdinal()
    {
        AudioId upper = AudioId.From("Sfx/UI/MouseClick");
        AudioId lower = AudioId.From("sfx/ui/mouseclick");

        Assert.AreNotEqual(upper, lower);
        Assert.AreEqual(upper, AudioId.From("Sfx/UI/MouseClick"));
    }

    [Test]
    public void None_IsInvalidAndEmpty()
    {
        Assert.IsFalse(AudioId.None.IsValid);
        Assert.AreEqual(string.Empty, AudioId.None.Path);
        Assert.AreEqual("None", AudioId.None.ToString());
    }

    [Test]
    public void Catalog_ContainsEveryRegisteredPath()
    {
        Assert.IsTrue(LayeredIdCatalog.Contains(typeof(AudioCatalog), AudioCatalog.MouseClick));
        Assert.IsTrue(LayeredIdCatalog.Contains(typeof(AudioCatalog), AudioCatalog.GenericSfx));
        Assert.IsTrue(LayeredIdCatalog.Contains(typeof(AudioCatalog), AudioCatalog.GenericOst));
    }

    [Test]
    public void Catalog_HasNoIssues()
    {
        Assert.IsEmpty(LayeredIdCatalog.Validate(typeof(AudioCatalog)));
    }
}
