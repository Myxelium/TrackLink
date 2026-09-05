using api.Services;
using Xunit;

namespace api.Tests;

public class DriveFolderScopeTests
{
    [Fact]
    public void File_directly_in_root_is_inside()
    {
        var parents = new Dictionary<string, IReadOnlyList<string>>
        {
            ["take"] = ["root"]
        };

        Assert.True(DriveFolderScope.IsInsideRoot("take", "root", parents));
    }

    [Fact]
    public void Nested_file_is_inside()
    {
        var parents = new Dictionary<string, IReadOnlyList<string>>
        {
            ["take"] = ["session"],
            ["session"] = ["root"]
        };

        Assert.True(DriveFolderScope.IsInsideRoot("take", "root", parents));
    }

    [Fact]
    public void File_in_another_tree_is_outside()
    {
        var parents = new Dictionary<string, IReadOnlyList<string>>
        {
            ["take"] = ["other"]
        };

        Assert.False(DriveFolderScope.IsInsideRoot("take", "root", parents));
    }
}
