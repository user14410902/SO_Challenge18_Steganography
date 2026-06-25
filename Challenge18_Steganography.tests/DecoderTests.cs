using Challenge18_Steganography.lib;

namespace Challenge18_Steganography.tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("sample_images/1RtT7h3L.png", "Test Test Test Test Test Test Test Test Test T")]
    [TestCase("sample_images/EKD3bBZP.png", "Test Test Test Test Test Test Test Test Test T")]
    [TestCase("sample_images/bmDwolWU.png", "Test Test Test Test Test Test Test Test Test T")]
    [TestCase("sample_images/MBlXyTSp.png", "The first part of the sentence is \"Three may keep a secret, ...\".\n\n### Task 2: D")]
    [TestCase("sample_images/mCeETXDs.png", "Test Test Test Test Test Test Test Test Test T")]
    public void TestSamples(string filename, string expected_message)
    {
        var actual = Decoder.Decode(filename);
        Assert.That(actual, Is.EqualTo(expected_message));
    }
}
