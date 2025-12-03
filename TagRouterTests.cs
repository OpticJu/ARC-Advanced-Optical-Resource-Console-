using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using ARC.FileRouting;

public class TagRouterTests : IDisposable
{
    private readonly string _tmpRoot;
    public TagRouterTests()
    {
        _tmpRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tmpRoot, true); } catch { }
    }

    private string MakeTempFile(string name, byte[] contents)
    {
        var path = Path.Combine(_tmpRoot, name);
        File.WriteAllBytes(path, contents);
        return path;
    }

    [Fact]
    public void Route_WithTag_GoesToMappedFolder()
    {
        var src = MakeTempFile("[Setup]dodo.txt", new byte[] { 1, 2, 3 });
        var destRoot = Path.Combine(_tmpRoot, "dest");
        Directory.CreateDirectory(destRoot);
        var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { "Setup", "SetupFolder" } };

        var result = TagRouter.RouteFile(src, destRoot, tagMap);

        Assert.True(File.Exists(result));
        Assert.Contains(Path.Combine(destRoot, "SetupFolder"), result);
    }

    [Fact]
    public void Route_WithoutTag_GoesToDefault()
    {
        var src = MakeTempFile("no_tag_file.txt", new byte[] { 1 });
        var destRoot = Path.Combine(_tmpRoot, "dest");
        Directory.CreateDirectory(destRoot);

        var result = TagRouter.RouteFile(src, destRoot, defaultFolder: "NoTag");

        Assert.True(File.Exists(result));
        Assert.Equal("NoTag", new DirectoryInfo(Path.GetDirectoryName(result)!).Name);
    }

    [Fact]
    public void Route_Collision_AppendsCounter()
    {
        var src1 = MakeTempFile("[A]file.txt", new byte[] { 1 });
        var src2 = MakeTempFile("[A]file.txt", new byte[] { 2 });
        var destRoot = Path.Combine(_tmpRoot, "dest");
        Directory.CreateDirectory(destRoot);

        var r1 = TagRouter.RouteFile(src1, destRoot, null, "Unsorted");
        var r2 = TagRouter.RouteFile(src2, destRoot, null, "Unsorted");

        Assert.True(File.Exists(r1));
        Assert.True(File.Exists(r2));
        Assert.NotEqual(r1, r2);
        Assert.Matches(@"\(\d+\)\.txt$", r2);
    }
}