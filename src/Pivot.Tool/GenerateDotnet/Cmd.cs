using System.CommandLine;

namespace Pivot.Tool.GenerateDotnet;

public static class Cmd
{
    public static Command Get()
    {
        var cmd = new Command("generate-dotnet", "Generate .NET project files")
        {
            
        };
        cmd.SetHandler(async (file) =>
        {
            
        }, new Option<string>("sdf"));

        return cmd;
    }
}