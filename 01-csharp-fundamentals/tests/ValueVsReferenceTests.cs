namespace CSharpFundamentals.Tests;

public class ValueVsReferenceTests
{

  [Fact]
  public void Copying_a_struct_does_not_change_the_original()
  {
    var s = new PointStruct { X = 1 };
    var b = s;
    b.X = 100;

    Assert.Equal(1, s.X);
    Assert.Equal(100, b.X);

  }

  [Fact]
  public void Copying_a_class_changes_the_original()
  {
    var c = new PointClass { X = 1 };
    var b = c;
    b.X = 100;
    Assert.Equal(100, c.X);
    Assert.Equal(100, b.X);
  }

  [Fact]
  public void Passing_a_struct_to_a_method_does_not_change_the_original()
  {
    var s = new PointStruct { X = 1 };
    ChangeStruct(s);
    Assert.Equal(1, s.X);
    void ChangeStruct(PointStruct p) { p.X = 100; }
  }
  
  [Fact]
  public void Passing_a_class_to_a_method_changes_the_original()
  {
    var c = new PointClass { X = 1 };
    ChangeClass(c);
    Assert.Equal(100, c.X);
    void ChangeClass(PointClass p) { p.X = 100; }
  }

  [Fact]
  public void Passing_a_class_to_a_method_that_reassigns_does_not_changes_the_original()
  {
    var c = new PointClass { X = 1 };
    Replace(c);
    Assert.Equal(1, c.X);
    void Replace(PointClass p) { p = new PointClass { X = 100 }; }
  }
}
