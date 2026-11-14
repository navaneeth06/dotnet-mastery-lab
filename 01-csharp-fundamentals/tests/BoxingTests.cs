

namespace CSharpFundamentals.Tests;

public class BoxingTests
{
    [Fact]
    public void Boxing_copies_the_value_so_changing_the_original_does_not_change_the_box()
    {
        int a = 5;
        object b = a;
        a = 10;
        Assert.Equal(5, (int)b);
    }

    [Fact]
    public void Two_boxes_holding_the_same_number_are_not_equal_with_equality_operator()
    {
        object a = 5;
        object b = 5;
        Assert.False(a == b);
    }

    [Fact]
    public void Two_boxes_holding_the_same_number_are_equal_with_equals()
    {
        object a = 5;
        object b = 5;
        Assert.True(a.Equals(b));
    }
}