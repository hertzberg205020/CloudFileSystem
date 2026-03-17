using CloudFileSystem.ConsoleApp;
using FluentAssertions;
using Xunit;

namespace CloudFileSystem.Tests.Cli;

public class TokenizerTests
{
    public static TheoryData<string, string[]> TokenizeCases =>
        new()
        {
            // 基本分割
            { "tag README.txt Urgent", new[] { "tag", "README.txt", "Urgent" } },
            // 引號保留空白
            { "tag \"Not Urgent\" Work", new[] { "tag", "Not Urgent", "Work" } },
            // 多重空白忽略
            { "tag  README.txt   Urgent", new[] { "tag", "README.txt", "Urgent" } },
            // 空輸入
            { "", Array.Empty<string>() },
            // 純空白
            { "   ", Array.Empty<string>() },
            // 未閉合引號
            { "tag \"Not Urgent", new[] { "tag", "Not Urgent" } },
            // 引號路徑
            {
                "delete \"個人筆記 (PN)/file.txt\"",
                new[] { "delete", "個人筆記 (PN)/file.txt" }
            },
            // 無引號向後相容
            {
                "tag 個人筆記 (PN)/待辦.txt Urgent",
                new[] { "tag", "個人筆記", "(PN)/待辦.txt", "Urgent" }
            },
            // 空引號被丟棄
            { "tag \"\" Urgent", new[] { "tag", "Urgent" } },
        };

    [Theory]
    [MemberData(nameof(TokenizeCases))]
    public void Tokenize_GivenInput_ReturnsExpectedTokens(string input, string[] expected)
    {
        var result = CloudFileSystemCli.Tokenize(input);

        result.Should().Equal(expected);
    }
}
