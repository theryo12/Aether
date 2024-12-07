using Aether.Core;
using Aether.Tests.Core;


var runner = new TestRunner();
runner.RunAll();

public class TESTS_ComponentMask
{
    private ComponentMask _mask;

    [Setup]
    public void Setup()
    {
        _mask = new ComponentMask(128);
    }

    [Test("SetBit and HasBit functionality test")]
    public void SetBit_HasBit()
    {
        _mask.SetBit(10);
        _mask.SetBit(64);

        Assert.That(_mask.HasBit(10), "Bit at index 10 should be set.");
        Assert.That(_mask.HasBit(64), "Bit at index 64 should be set.");
        Assert.That(!_mask.HasBit(11), "Bit at index 11 should not be set.");
    }

    [Test("ClearBit functionality test")]
    public void ClearBit()
    {
        _mask.SetBit(20);
        Assert.That(_mask.HasBit(20), "Bit at index 20 should be set.");

        _mask.ClearBit(20);
        Assert.That(!_mask.HasBit(20), "Bit at index 20 should be cleared.");
    }

    [Test("EnsureCapacity dynamically resizes array")]
    public void EnsureCapacity()
    {
        _mask.SetBit(200);
        Assert.That(_mask.HasBit(200), "Bit at index 200 should be set after dynamic resizing.");
    }

    [Test("GetBitIndex<T> unique ID generation")]
    public void GetBitIndex()
    {
        int index1 = ComponentMask.GetBitIndex<int>();
        int index2 = ComponentMask.GetBitIndex<double>();

        Assert.That(index1 != index2, "Different types should have different bit indices.");

        int index3 = ComponentMask.GetBitIndex<int>();
        Assert.That(index1 == index3, "Same type should have the same bit index.");
    }

    [Test("GetBitIndex<T> throws on exceeding limit")]
    public void GetBitIndexLimit()
    {
        for (int i = 0; i < 1024; i++)
        {
            ComponentMask.GetBitIndex<(int, int)>();
        }

        Assert.Throws<InvalidOperationException>(() =>
        {
            ComponentMask.GetBitIndex<(int, int)>();
        }, "Exceeded the maximum limit of 1024 unique bit indices.");
    }

    [Test("Equals and GetHashCode equality")]
    public void TestEqualsAndHashCode()
    {
        var mask1 = new ComponentMask(64);
        var mask2 = new ComponentMask(64);

        mask1.SetBit(15);
        mask2.SetBit(15);

        Assert.That(mask1.Equals(mask2), "Masks with the same bits set should be equal.");
        Assert.That(mask1.GetHashCode() == mask2.GetHashCode(), "Equal masks should have the same hash code.");
    }

    [Test("ToString provides binary representation")]
    public void ToString()
    {
        _mask.SetBit(2);
        _mask.SetBit(32);

        string result = _mask.ToString();
        Assert.That(result.Contains("00000000000000000000000000000100"), "Binary representation should include index 2.");
        Assert.That(result.Contains("00000000000000000000000000000001"), "Binary representation should include index 32.");
    }
}