using System;
using System.Collections.Generic;
using ARC.FileRouting;

class Program
{
    static void Main(string[] args)
    {
        // 간단한 예제: 커맨드라인 인자로 사용 가능
        // Usage: dotnet run -- <sourceFile> <destRoot> [move|copy]
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: <sourceFile> <destRoot> [move|copy]");
            return;
        }

        var src = args[0];
        var destRoot = args[1];
        var move = true;
        if (args.Length >= 3 && args[2].Equals("copy", StringComparison.OrdinalIgnoreCase))
            move = false;

        var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Setup", "Setup" },
            { "Log", "Logs" },
            { "Config", "Configs" }
        };

        try
        {
            var result = TagRouter.RouteFile(src, destRoot, tagMap, defaultFolder: "Unsorted", move: move);
            Console.WriteLine($"파일이 이동되었습니다: {result}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"에러: {ex.Message}");
        }
    }
}