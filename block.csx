using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System;

var sourceFile = Directory.EnumerateFiles(Path.GetDirectoryName(
    Environment.CommandLine.Substring(Environment.CommandLine.IndexOf(Path.DirectorySeparatorChar))),
    "*.bs").First();

bool TryReduce(IEnumerable<Func<string, string>> enumerable, ref string line) {
    var reduction = enumerable.Aggregate(line, (modifiedLine, modifier) =>
        modifier(modifiedLine));
    if (reduction == line) return false;
    line = reduction;
    return true;
}

Console.Clear();

var lines = new List<string>(File.ReadAllLines(sourceFile));

var blocks = new List<Func<string, string>> {
    input => Regex.Replace(input, "(if|while|for|foreach) (.+)$", match => {
        if (!match.Success) return input;
        var op = match.Groups[1].Value;
        var expression = match.Groups[2].Value;
        if (!Regex.Match(expression, "[=!<>]").Success)
            expression = $"{expression} != null";
        return $"{op} ({expression}) {{";
    }),
    input => Regex.Replace(input, "else$", "else {"),
    input => Regex.Replace(input, "throw (.+?)$", "throw new $1;")
};

var variables = new List<Func<string, string>> {
    input => Regex.Replace(input, "(.+) = (.+)", match => {
        if (!match.Success) return input;
        var name = match.Groups[1].Value.Replace(" ", "_");
        var value = match.Groups[2].Value.Replace("\'", "\"");
        return $"var {name} = {value};"; 
    }),
    input => Regex.Replace(input, "using (.+)$", "using $1;")
};

var modifications = new List<Func<string, string>> {
    input => new Regex("(\\w+)\\s(.+)").Replace(input, "$1($2)", 1),
    input => Regex.Replace(input, "(.[^;])$", "$1;"),
};

var strings = new List<Func<string, string>> {
    input => Regex.Replace(input, "\'(.+)\'", match => {
        if (!match.Success) return input;
        var replacement = match.Groups[1].Value.Replace("\'\'", "\\\'");
        return $@"$""{replacement}""";
    })    
};

var pipeline = strings.Concat(modifications);

bool inBlock = false;
var scriptContent = lines
    .Select(line => {

        if (!TryReduce(blocks, ref line)) {
            if (!TryReduce(variables, ref line)) {
                line = pipeline.Aggregate(
                    line, (modifiedInput, modifier) => modifier(modifiedInput)
                );
            }
        } else {
            inBlock = true;
        }

        if (line.StartsWith("else"))
            return "}\n" + line;
        if (line.Trim() == "" && inBlock) {
            inBlock = false;
            return "}\n";
        }
        return line; 
    });

var tempFile = Path.GetTempFileName();
File.WriteAllText(tempFile, String.Join(Environment.NewLine, scriptContent));

var executable = Path.Combine(
    Environment.CurrentDirectory, 
    Environment.CommandLine.Substring(0, Environment.CommandLine.IndexOf(" ")));

Console.WriteLine("executable " + executable);
Console.WriteLine("tempFile " + tempFile);
Console.WriteLine("-");
Console.WriteLine(String.Join(Environment.NewLine, scriptContent));
var psi = new ProcessStartInfo("cmd", "/c" + executable + " " + tempFile);
var subProcess = Process.Start(psi);
