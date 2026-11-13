when you copy a value type to a variable it create a copy from the original ,changing the new copied variable doesnt modify the original value ,eg: int, struct

reference type store the memory address instead of object when you copy, so changing it through new variable modify the original object becase both variable points to the same object.Eg: class

When you pass a class to method ,the method parameter get the copy of reference and changing it modifies the original where as when you pass a struct to a method , the method gets the copy of object and changing it does not modify the original.

But reassigning a class parameter inside a method doesnt modify the original object beacuse the method receives a copy of reference and while reassigning the variable inside method changes the methodn copy.So the caller's variable still points to the old object.

C# keyword ref,out and in does allow  pass by reference.
You can create your own value type using struct which java can't.