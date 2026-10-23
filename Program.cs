using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

Encoding inEncoding = Encoding.UTF8;
Encoding outEncoding = Encoding.UTF8;
bool ByteOrderMark = false;
bool verbose = false;

if (args.Length < 1)
{
    ErrorMessageAndExit("Too few arguments. Use -help for more information");
}


//-source file
//-target file
//-inencoding
//-outencoding
//-rlblank - removes blank lines
//-rlfirst n - remove n number of lines from start of file
//-rllast n - remove n number of lines from end of file
//-rlcontains "aaa" - remove lines containing string (case insensitive)
//-rlexact "aaa" - remove lines containing an exact match (case insensitive)
//-replace "aaa" "bbb" - replace all instances of a string with
//-trim
//-verbose
//-help

// Build arguments

Dictionary<string, Argument> arguments = [];

for (int i = 0; i < args.Length; i++)
{
    string arg = args[i];
    if (arg.StartsWith("-"))
    {
        Argument newArg = new(arg);
        if (arguments.ContainsKey(arg))
        {
            ErrorMessageAndExit($"You can't add the argument {arg} twice");
        }
        arguments.Add(arg, newArg);
        for (int a = i + 1; a <= i + 2; a++)
        {
            if (args.Length > a)
            {
                if (args[a].StartsWith("-") == false)
                {
                    newArg.Subargs.Add(args[a]);
                }
                else
                {
                    break;
                }
            }
        }
    }
}

VerboseWriteLine("---ARGUMENTS---");
foreach (Argument arg in arguments.Values)
{
    VerboseWriteLine($"Name: {arg.Name}");
    foreach (string subarg in arg.Subargs)
    {
        VerboseWriteLine($"Subarg: {subarg}");
    }
    VerboseWriteLine("");
}

string sourcefile = "";

// Process arguments

if (arguments.ContainsKey("-verbose"))
{
    verbose = true;
}

if (arguments.ContainsKey("-help") || arguments.ContainsKey("-?") || arguments.ContainsKey("/help") || arguments.ContainsKey("/?"))
{
    ShowHelp();
    Environment.Exit(0);
}

// source file -----------------------------------------------------
if (arguments.ContainsKey("-source"))
{
    Argument source = arguments["-source"];
    if (source.Subargs.Count == 0)
    {
        ErrorMessageAndExit("Missing filename in -source argument, please specify an input file.");
    }
    sourcefile = source.Subargs[0];
    if (File.Exists(sourcefile))
    {
        VerboseWriteLine($"Found file {sourcefile}");
    }
    else
    {
        ErrorMessageAndExit($"Source file not found: {sourcefile}");
    }
}
else
{
    ErrorMessageAndExit("Missing -source argument, please specify an input file.");
}

// target file -----------------------------------------------------
string? targetfile = null;
if (arguments.ContainsKey("-target"))
{
    Argument target = arguments["-target"];
    if (target.Subargs.Count == 0)
    {
        WarningMessage("Missing filename in -source argument, please specify an input file.");
        
    }

    targetfile = target.Subargs[0];
    string? targetDirectory = Path.GetDirectoryName(targetfile);
    if (Directory.Exists(Path.GetDirectoryName(targetfile)))
    {
        VerboseWriteLine($"Found directory {targetDirectory}");
    }
    else
    {
        WarningMessage($"Target directory not found: {targetDirectory}");
    }
}
else
{
    WarningMessage("Missing -target argument, please specify an output file.");
}
if (targetfile == "" || targetfile == null)
{
    WarningMessage("Showing processing, but no file is created");
}

// encoding ----------------------------------------------------

if (arguments.ContainsKey("-inencoding"))
{
    Argument arg = arguments["-inencoding"];
    if (arg.Subargs.Count < 1)
    {
        ErrorMessageAndExit("Too few arguments in -inencoding");
    }
    inEncoding = GetEncoding(arg.Subargs[0]);
}

if (arguments.ContainsKey("-outencoding"))
{
    Argument arg = arguments["-outencoding"];
    if (arg.Subargs.Count < 1)
    {
        ErrorMessageAndExit("Too few arguments in -outencoding");
    }
    outEncoding = GetEncoding(arg.Subargs[0]);
}

// load file -----------------------------------------------------
string[] linesArray = [];
try
{
    linesArray = File.ReadAllLines(sourcefile, inEncoding);
}
catch (Exception ex)
{
    ErrorMessageAndExit($"Error loading source file {sourcefile}\r\n{ex.Message}");
}

Console.WriteLine($"Loaded {linesArray.Length} lines from source file {sourcefile}");
if (linesArray.Length < 1)
{
    ErrorMessageAndExit("No lines in source file, exiting");
}

List<string> linesOut = linesArray.ToList();

// fix lines ------------------------------------------------

if (arguments.ContainsKey("-rlfirst"))
{
    Argument arg = arguments["-rlfirst"];
    if (arg.Subargs.Count < 1)
    {
        ErrorMessageAndExit("Too few arguments in -rlfirst");
    }
    int linecount = 0;
    try
    {
        linecount = int.Parse(arg.Subargs[0]);
    }
    catch (Exception ex)
    {
        ErrorMessageAndExit($"Couldn't parse -rlfirst line count {arg.Subargs[0]}\n{ex.Message}");
    }

    int linesOld = linesOut.Count;
    
    linesOut.RemoveRange(0, linecount);
    VerboseWriteLine($"Removing first {linecount} lines: {linesOld - linesOut.Count}");
    VerboseWriteLine($"First line {linesOut.First()}");
}

if (arguments.ContainsKey("-rllast"))
{
    Argument arg = arguments["-rllast"];
    if (arg.Subargs.Count < 1)
    {
        ErrorMessageAndExit("Too few arguments in -rllast");
    }
    int linecount = 0;
    try
    {
        linecount = int.Parse(arg.Subargs[0]);
    }
    catch (Exception ex)
    {
        ErrorMessageAndExit($"Couldn't parse -rllast line count {arg.Subargs[0]}\r\n{ex.Message}");
    }

    int linesOld = linesOut.Count;
    
    linesOut.RemoveRange(linesOut.Count-linecount, linecount);
    VerboseWriteLine($"Removing last {linecount} lines: {linesOld - linesOut.Count}");
    VerboseWriteLine($"Last line: {linesOut.Last()}");
}

if (arguments.ContainsKey("-replace"))
{
    Argument arg = arguments["-replace"];
    if (arg.Subargs.Count < 2)
    {
        ErrorMessageAndExit("Too few arguments in -replace");
    }

    string oldtext = arg.Subargs[0];
    string newtext = arg.Subargs[1];

    int linesChanged = 0;
    for (int i = 0; i < linesOut.Count; i++)
    {
        int l = linesOut[i].Length;
        linesOut[i] = linesOut[i].Replace(oldtext, newtext);
        if (l != linesOut[i].Length)
        {
            linesChanged++;
        }
    }
    VerboseWriteLine($"Replace Text changed lines {linesChanged}");
}

if (arguments.ContainsKey("-rlcontains"))
{
    Argument arg = arguments["-rlcontains"];
    if (arg.Subargs.Count < 1)
    {
        ErrorMessageAndExit("Too few arguments in -rlcontains");
    }

    string text = arg.Subargs[0];

    int linesOld = 0;
    linesOut.RemoveAll(u => u.Contains(text, StringComparison.CurrentCultureIgnoreCase));
    VerboseWriteLine($"Removed lines containing '{text}': {linesOld - linesOut.Count}");
}

if (arguments.ContainsKey("-rlexact"))
{
    
    Argument arg = arguments["-rlexact"];
    if (arg.Subargs.Count < 1)
    {
        ErrorMessageAndExit("Too few arguments in -rlexact");
    }

    string text = arg.Subargs[0];

    int linesOld = linesOut.Count;
    linesOut.RemoveAll(u => u.Equals(text, StringComparison.CurrentCultureIgnoreCase));
    VerboseWriteLine($"Removing lines matching '{text}': {linesOld - linesOut.Count}");
}

if (arguments.ContainsKey("-trim"))
{
    int linesChanged = 0;
    for (int i = 0; i < linesOut.Count; i++)
    {
        int l = linesOut[i].Length;
        linesOut[i] = linesOut[i].Trim();
        if (l != linesOut[i].Length)
        {
            linesChanged++;
        }
    }
    VerboseWriteLine($"Trimmed lines: {linesChanged}");
}

if (arguments.ContainsKey("-rlblank"))
{
    int linesOld = linesOut.Count;
    linesOut.RemoveAll(u => u.Trim().Length == 0);
    VerboseWriteLine($"Removed empty lines: {linesOld -linesOut.Count}");
}

// save file

if (targetfile != null)
{
    Console.WriteLine($"Saving to file {targetfile}");
    try
    {
        if (outEncoding == Encoding.UTF8 && ByteOrderMark == false)
        {
            File.WriteAllLines(targetfile, linesOut.ToArray()); // Don't add Byte order mark (0x EF BB BF) to the start of the file in UTF-8
        }
        else
        {
            File.WriteAllLines(targetfile, linesOut.ToArray(), outEncoding); // set custom encoding, when UTF-8 encoding is specified, byte order mark gets added automatically
        }
    }
    catch (Exception ex)
    {
        ErrorMessageAndExit($"Error writing to file {targetfile}\n{ex.Message}");
    }
}


//  if (encoding == Encoding.UTF8 && Arguments.ByteOrderMark == false)
//     File.WriteAllLines(filename, linesOut); // Don't add Byte order mark (0x EF BB BF) to the start of the file in UTF-8
//  else
//     File.WriteAllLines(filename, linesOut, encoding); // set custom encoding, when UTF-8 encoding is specified, byte order mark gets added automatically

Encoding GetEncoding(string encodingName)
{
    VerboseWriteLine("Getting encoding from value:" + encodingName);
    switch (encodingName.ToLowerInvariant())
    {
        case "":
            return Encoding.UTF8;
        case "default":
            return Encoding.Default;
        case "utf-8":
        case "utf8":
            return Encoding.UTF8;
        case "utf-8-bom":
        case "utf8bom":
            ByteOrderMark = true;
            return Encoding.UTF8;
        case "latin1":
            return Encoding.Latin1;
        case "ascii":
            return Encoding.ASCII;
        default:
            try
            {
                VerboseWriteLine("Custom encoding detected");
                if (int.TryParse(encodingName, out int value))
                {
                    VerboseWriteLine("Encoding, parsing codepage: " + value);
                    VerboseWriteLine("Encoding result:" + GetEncodingDOS(value).EncodingName);
                    return GetEncodingDOS(value);
                }
                else
                {
                    VerboseWriteLine("Encoding, parsing name: " + encodingName);
                    VerboseWriteLine("Encoding result:" + Encoding.GetEncoding(encodingName));
                    return Encoding.GetEncoding(encodingName);
                }
            }
            catch
            {
                WarningMessage($"Error parsing encoding {encodingName}, defaulting to UTF-8");
                return Encoding.UTF8; // should be unreachable
            }
    }
}

static Encoding GetEncodingDOS(int codepage)
{
    Encoding encoding;
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    try
    {
        encoding = Encoding.GetEncoding(codepage);
    }
    catch
    {
        encoding = Encoding.UTF8;
    }
    return encoding;
}

void VerboseWriteLine(string message)
{
    if (verbose)
    {
        Console.WriteLine(message);
    }
}

void WarningMessage(string message, int code = 1)
{
    ConsoleColor previous = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(message);
    Console.ForegroundColor = previous;
}

void ErrorMessageAndExit(string message, int code = 1)
{
    ConsoleColor previous = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ForegroundColor = previous;
    Environment.Exit(code);
}

void ShowHelp()
{
    Console.WriteLine();
    Console.WriteLine("ALTERTEXTFILE.EXE -source FILENAME -target FILENAME (arguments)");
    Console.WriteLine("Processes a text file to remove unwanted lines or strings, or change codepage");
    Console.WriteLine();
    Console.WriteLine("-source FILEPATH        The file to read from");
    Console.WriteLine("-target FILEPATH        The file to write to");
    Console.WriteLine("-inencoding CODEPAGE    Read source file using UTF8/Latin1/865...");
    Console.WriteLine("-outencoding CODEPAGE   Write to target file using UTF8/Latin1/865...");
    Console.WriteLine("-rlblank                removes blank lines");
    Console.WriteLine("-rlfirst n              remove n number of lines from start of file");
    Console.WriteLine("-rllast n               remove n number of lines from end of file");
    Console.WriteLine("-rlcontains 'aaa'       remove lines containing string (case insensitive)");
    Console.WriteLine("-rlexact 'aaa'          remove lines containing an exact match (case insensitive)");
    Console.WriteLine("-replace 'aaa' 'bbb'    replace all instances of a string with");
    Console.WriteLine("-trim                   Trim white space at the start and end of lines");
    Console.WriteLine("-verbose                Displays the effect of each argument during processing");
    Console.WriteLine("-help                   This page");
    Console.WriteLine();
}

class Argument
{
    internal string Name;
    internal List<string> Subargs = [];

    public Argument(string name)
    {
        this.Name = name;
    }
}