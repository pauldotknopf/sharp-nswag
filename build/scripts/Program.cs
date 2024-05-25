using static Bullseye.Targets;
using static Build.Buildary.Directory;
using static Build.Buildary.Path;
using static Build.Buildary.Shell;
using static Build.Buildary.Runner;
using static Build.Buildary.Log;
using static Build.Buildary.File;
using static Build.Buildary.GitVersion;

namespace Build
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = ParseOptions<RunnerOptions>(args);
            
            var gitVersion = GetGitVersion(ExpandPath("./"));
            var commandBuildArgs = $"--configuration {options.Config} ";
            
            Info($"Version: {gitVersion.FullVersion}");
            
            Target("clean", () =>
            {
                CleanDirectory(ExpandPath("./output"));
            });
            
            Target("update-version", () =>
            {
                if (FileExists("./build/version.props"))
                {
                    DeleteFile("./build/version.props");
                }
                
                WriteFile("./build/version.props",
                    $@"<Project>
    <PropertyGroup>
        <VersionPrefix>{gitVersion.Version}</VersionPrefix>
    </PropertyGroup>
</Project>");
            });
            
            Target("build", () =>
            {
                RunShell($"dotnet build {commandBuildArgs} ./Pivotte.sln");
            });
            
            Target("publish", () =>
            {
                RunShell($"dotnet pack {commandBuildArgs} --output {ExpandPath("./output")} {ExpandPath("./Pivotte.sln")}");
            });
            
            Target("default", DependsOn("clean", "update-version", "build", "publish"));

            Execute(options);
        }
    }
}