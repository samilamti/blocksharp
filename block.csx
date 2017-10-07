using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System;

var sourceFile = Env.ScriptArgs[0];
var targetFile = Env.ScriptArgs.Count() > 1 ? Env.ScriptArgs[1] : null;

System.Console.WriteLine("Processing " + sourceFile);

bool TryReduce(IEnumerable<Func<string, string>> enumerable, ref string line) {
    var reduction = enumerable.Aggregate(line, (modifiedLine, modifier) =>
        modifier(modifiedLine));
    if (reduction == line) return false;
    line = reduction;
    return true;
}

//Console.Clear();

var lines = new List<string>(File.ReadAllLines(sourceFile));

var processors = new Dictionary<string, Func<string, string>> {
    { "using", s => $"using {s};" },
    { "else", s => $"\\t{s};"}
};
Func<string, string> processor = null;

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
    input => Regex.Replace(input, "throw (.+?)$", "throw new $1;"),
    input => Regex.Replace(input, "using", match => {
        Console.WriteLine("W00p " + match.Value);
        processor = processors["using"];
        return String.Empty;
    }),
    input => Regex.Replace(input, "^\\s+(\\S+?)$", match => {
        var val = match.Groups[1].Value;
        return processor?.Invoke(val) ?? val;
    })
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

var scriptContent = lines
    .Select(line => {

        if (!TryReduce(blocks, ref line)) {
            if (!TryReduce(variables, ref line)) {
                line = pipeline.Aggregate(
                    line, (modifiedInput, modifier) => modifier(modifiedInput)
                );
            }
        }

        return line; 
    })
    .Where(line => !String.IsNullOrEmpty(line));

var tempFile = targetFile ?? Path.GetTempFileName();
File.WriteAllText(tempFile, String.Join(Environment.NewLine, scriptContent));
System.Console.WriteLine("Writing " + tempFile);

// var executable = Path.Combine(
//     Environment.CurrentDirectory, 
//     Environment.CommandLine.Substring(0, Environment.CommandLine.IndexOf(" ")));

// Console.WriteLine("executable " + executable);
// Console.WriteLine("tempFile " + tempFile);
// Console.WriteLine("-");
// Console.WriteLine(String.Join(Environment.NewLine, scriptContent));
// var psi = new ProcessStartInfo("cmd", "/c" + executable + " " + tempFile);
// var subProcess = Process.Start(psi);
