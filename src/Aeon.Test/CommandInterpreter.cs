using Aeon.Emulator.CommandInterpreter;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test;

[TestClass]
public class CommandInterpreter
{
    [TestMethod]
    public void Call()
    {
        var call = Parse<CallCommand>("call file.bat");
        Assert.AreEqual("file.bat", call.Target);
    }
    [TestMethod]
    public void Cls()
    {
        Parse<ClsCommand>("cls");
    }
    [TestMethod]
    public void Dir()
    {
        var dir = Parse<DirectoryCommand>("dir");
        Assert.AreEqual("", dir.Path);
    }
    [TestMethod]
    public void Echo()
    {
        var echo = Parse<EchoCommand>("echo this is a test");
        Assert.AreEqual("this is a test", echo.Text);
    }
    [TestMethod]
    public void Exit()
    {
        Parse<ExitCommand>("exit");
    }
    [TestMethod]
    public void Goto()
    {
        var gotoCommand = Parse<GotoCommand>("goto label");
        Assert.AreEqual("label", gotoCommand.Label);
    }
    [TestMethod]
    public void Label()
    {
        var label = Parse<LabelStatement>(":label");
        Assert.AreEqual("label", label.Name);
    }
    [TestMethod]
    public void Launch()
    {
        var launch = Parse<LaunchCommand>("program.exe some args");
        Assert.AreEqual("program.exe", launch.Target);
        Assert.AreEqual("some args", launch.Arguments);
    }
    [TestMethod]
    public void PrintCurrentDirectory()
    {
        Parse<PrintCurrentDirectoryCommand>("cd");
    }
    [TestMethod]
    public void PrintEnvironment()
    {
        Parse<PrintEnvironmentCommand>("set");
    }
    [TestMethod]
    public void Rem()
    {
        var rem = Parse<RemCommand>("rem this is a comment");
        Assert.AreEqual("this is a comment", rem.Comment);
    }
    [TestMethod]
    public void Set()
    {
        var setCommand = Parse<SetCommand>("set var=value");
        Assert.AreEqual("var", setCommand.Variable);
        Assert.AreEqual("value", setCommand.Value);
    }
    [TestMethod]
    public void SetCurrentDirectory()
    {
        var setDir = Parse<SetCurrentDirectoryCommand>("cd dir");
        Assert.AreEqual("dir", setDir.Path);
    }
    [TestMethod]
    public void SetCurrentDrive()
    {
        var setDrive = Parse<SetCurrentDriveCommand>("c:");
        Assert.AreEqual(DriveLetter.C, setDrive.Drive);
    }
    [TestMethod]
    public void Type()
    {
        var type = Parse<TypeCommand>("type file.txt");
        Assert.AreEqual("file.txt", type.FileName);
    }

    private static TCommand Parse<TCommand>(string s) where TCommand : CommandStatement
    {
        var cmd = StatementParser.Parse(s);
        Assert.IsInstanceOfType(cmd, typeof(TCommand));
        return (TCommand)cmd;
    }
}
