using System.CommandLine;

var rootCommand = new RootCommand("various tools for pivot");
rootCommand.AddCommand(Pivot.Tool.GenerateDotnet.Cmd.Get());

return await rootCommand.InvokeAsync(args);