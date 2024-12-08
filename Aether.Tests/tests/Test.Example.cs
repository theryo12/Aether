using Aether.Tests.Core;

public class TEST_Example
{
    private int value;
    // Setup is called before any test.
    [Setup]
    public void Setup()
    {
        value = 10; // Our 'value' will be 10 before any test is called, so even if we  played with it in one of our tests, it will still be 10 in the next one.
    }

    [Test("this test shows how to use Assert.That!")]
    //    ^ a small description of test so we know what's happening here.
    // Test attribute is required for every test so our TestRunner knows what exactly he // needs to run. Description is not required, it's used only for readability. You can // leave it as [Test()].
    public void ThatTest()
    {
        value = 15;
        Assert.That(value == 15, "The value should be 15!");
        // We use `Assert.That` method to validate the condition, in this case,
        // that our value is actually 15. If the condition fails, an `AssertException`
        // is thrown with out message - "The value should be 15!"
    }

    [Test("this test shows how to use Assert.Throws!")]
    public void ThrowsTest()
    {
        Assert.Throws<DivideByZeroException>(() =>
        {
            value /= 0;
        });
        // The `Assert.Throws<TException>` method verifies that the code inside
        // the provided delegate throws the expected exception type `TException`. In our
        // case, we make sure that `DivideByZeroException` is thrown when we divide our
        // value by zero.
    }

    // There's nothing complicated in this system at all, but that's all you need.

    /*
    [Test("an example of test that'll fail")]
    public void ThisTestWillFail()
    {
        // A simple demonstration of a test that'll fail. This is commented so
        // we don't get confused when running real tests.
        value = 15 / 3;
        Assert.That(value != 5, "The value is 5!");
    }
    */

    // Teardown is called after every test, even if it fails, so any changes made 
    // during the test don't affect the next one. It's not required, but useful for 
    // keeping tests independent and avoiding side effects.
    [Teardown]
    public void Teardown()
    {
        value = 0;
    }
}