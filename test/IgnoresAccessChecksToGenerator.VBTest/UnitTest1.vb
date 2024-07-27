Imports IgnoresAccessChecksToGenerator.VBTest.SourceAssembly
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UnitTest1
    <TestMethod>
    Sub TestSub()
        Dim temp = GetType(System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute)
        Assert.AreEqual("This is a secret.", SecretClass.SecretFunction)
    End Sub
End Class
