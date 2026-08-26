using System.Diagnostics;
using System.Formats.Asn1;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

class Program
{
    static void Main()
    {
        string[] builtInCommands = 
        {
            "exit",
            "echo",
            "type",
            "pwd",
            "cd"
        };

        string[] redirectOperators = 
        {
            ">",
            "1>",
            "2>"
        };

        while (true)
        {
            Console.Write("$ ");

            string command = Console.ReadLine();

            List<string> arguments = ParseCommand(command);


            if (arguments[0] == "exit")
            {
                break;
            }
            var redirect = ParseRedirection(arguments, redirectOperators);

            bool shouldRedirectOutput = redirect.redirectOutput;
            bool shouldRedirectError = redirect.redirectError;
            string? fileName = redirect.fileName;

            if(arguments[0] == "cd")
            {
                ExecuteCd(arguments, shouldRedirectOutput, fileName);
            }
            else if (arguments[0] == "echo")
            {
                ExecuteEcho(arguments, shouldRedirectOutput, shouldRedirectError, fileName);
            }
            else if (arguments[0] == "type")
            {
                ExecuteType(arguments, shouldRedirectOutput, fileName, builtInCommands);
            }
            else if (arguments[0] == "pwd")
            {
                ExecutePwd(arguments, shouldRedirectOutput, fileName);
            }
            else
            {
                ExecuteExternal(arguments, shouldRedirectOutput, shouldRedirectError, fileName, command);
            }  
        }
    }

    public static void ExecutePwd(List<string> arguments, bool shouldRedirectOutput, string fileName)
    {
        if (arguments[0] == "pwd")
            {
                var PwdOutput = Directory.GetCurrentDirectory();
                if (shouldRedirectOutput)
                {
                    File.WriteAllText(fileName, PwdOutput + Environment.NewLine);
                }
                else
                {
                    Console.WriteLine(PwdOutput);
                }
            }
    }

    public static void ExecuteType(List<string> arguments, bool shouldRedirectOutput, string fileName, string[] builtInCommands)
    {
        if (arguments[0] == "type")
            {
                string commandType = arguments[1];
                
                if (builtInCommands.Contains(commandType))
                {
                    if (shouldRedirectOutput)
                    {
                        File.WriteAllText(fileName, $"{commandType} is a shell builtin" + Environment.NewLine);
                    }
                    Console.WriteLine($"{commandType} is a shell builtin");
                }
                else if (GetExecutablePath(commandType) is string executablePath)
                {
                    if (shouldRedirectOutput)
                    {
                        File.WriteAllText(fileName, $"{commandType} is {executablePath}" + Environment.NewLine);
                    }
                    Console.WriteLine($"{commandType} is {executablePath}");
                }
                else
                {
                    if (shouldRedirectOutput)
                    {
                        File.WriteAllText(fileName, $"{commandType}: not found" + Environment.NewLine);
                    }
                    else
                    {
                        Console.WriteLine($"{commandType}: not found");
                    }
                }
            }
    }

    public static (bool redirectOutput, bool redirectError, string? fileName) ParseRedirection(List<string> arguments, string[] redirectOperators)
    {
        string? foundOperator = redirectOperators.FirstOrDefault(op => arguments.Contains(op));

        if (foundOperator == null)
        {
            return(false, false, null);
        }

        int index = arguments.IndexOf(foundOperator);
        string fileName = arguments[index +1];
        arguments.RemoveRange(index, arguments.Count - index);

        if (foundOperator == ">" || foundOperator == "1>")
        {
            bool redirectOutput = true;
            return(true, false, fileName);
        }
        else if (foundOperator == "2>")
        {
            bool redirectError = true;
            return(false, true, fileName);
        }
        else
        {
            return(false, false, null);
        }
    }

    // public static (bool shouldRedirectOutput, string? fileName) ParseRedirection(List<string> arguments, string[] redirectOperators)
    // {
    //     string? foundOperator =
    //         redirectOperators.FirstOrDefault(op => arguments.Contains(op));

    //     if (foundOperator == null)
    //     {
    //         return (false, null);
    //     }

    //     int index = arguments.IndexOf(foundOperator);

    //     string fileName = arguments[index + 1];

    //     arguments.RemoveRange(index, arguments.Count - index);

    //     return (true, fileName);
    // }

    public static void ExecuteCd(List<string> arguments, bool shouldRedirectOutput, string fileName)
    {
        if (arguments[0] == "cd")
        {
            string path = arguments[1];
            try
            {
                if (path == "~")
                {
                    string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    if (homeDirectory != null)
                    {
                        Directory.SetCurrentDirectory(homeDirectory);
                    }
                }
                else
                {
                    Directory.SetCurrentDirectory(path); 
                }
            }
            catch
            {
                Console.WriteLine($"cd: {path}: No such file or directory");
            }
        }
    }

    public static void ExecuteEcho(List<string> arguments, bool shouldRedirectOutput, bool shouldRedirectError, string fileName)
    {
        if (arguments[0] == "echo")
        {
            string echoOutput = string.Join(" ", arguments.Skip(1));
            

            if (shouldRedirectOutput)
            {
                File.WriteAllText(fileName!, echoOutput + Environment.NewLine);
            }
            else
            {
                Console.WriteLine(echoOutput);
            }

            if (shouldRedirectError)
            {
                File.WriteAllText(fileName!, "");
            }
        }
    }

    public static void ExecuteExternal(List<string> arguments, bool shouldRedirectOutput, bool shouldRedirectError, string fileName, string command)
    {
        if (GetExecutablePath(arguments[0]) is string executablePath)
            {        
                var startInfo = new ProcessStartInfo();

                startInfo.FileName = arguments[0];

                for (int i = 1; i < arguments.Count; i++)
                {
                    startInfo.ArgumentList.Add(arguments[i]);
                }

                if (shouldRedirectOutput)
                {
                    startInfo.RedirectStandardOutput = true;
                }
                else if (shouldRedirectError)
                {
                   startInfo.RedirectStandardError = true; 
                }

                Process process = Process.Start(startInfo);

                if (shouldRedirectOutput)
                {
                    string processOutput = process.StandardOutput.ReadToEnd();

                    File.WriteAllText(fileName, processOutput);
                }
                else if (shouldRedirectError)
                {
                    string processError = process.StandardError.ReadToEnd();

                    File.WriteAllText(fileName, processError);
                }
                process.WaitForExit();
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
    }

    public static List<string> ParseCommand(string command)
    {
        
            bool isSingleQuotes = false;
            bool isDoubleQuotes = false;
            bool escapeNext = false;
            List<string> arguments = new();
            StringBuilder currentArgument = new();

            foreach (char c in command)
            {
                if (escapeNext)
                {
                    escapeNext = false;
                    currentArgument.Append(c);
                }
                else if (c == '\\' && !isSingleQuotes)
                {
                    escapeNext = true;
                }
                else if (c == '\'' && !isDoubleQuotes)
                {
                    isSingleQuotes = !isSingleQuotes;
                }
                else if (c == '\"' && !isSingleQuotes)
                {
                    isDoubleQuotes = !isDoubleQuotes;
                }
                else if (c == ' ' && !isSingleQuotes && !isDoubleQuotes)
                {
                    if (currentArgument.Length > 0)
                    {
                        arguments.Add(currentArgument.ToString());
                        currentArgument.Clear();
                    }
                }
                else
                {
                    currentArgument.Append(c);
                }
            }
            if (currentArgument.Length > 0)
            {
                arguments.Add(currentArgument.ToString());
            }

            return arguments;
    }

    public static string GetExecutablePath(string command)
        {
            var path = Environment.GetEnvironmentVariable("PATH");

            foreach (var dir in path.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(dir, command);
                if (File.Exists(fullPath))
                {
                    if (OperatingSystem.IsWindows())
                    {
                        return fullPath;
                    }

                    UnixFileMode fileMode = File.GetUnixFileMode(fullPath);

                    bool isExecutable = (fileMode & UnixFileMode.UserExecute) != 0 ||
                                        (fileMode & UnixFileMode.GroupExecute) != 0 ||
                                        (fileMode & UnixFileMode.OtherExecute) != 0;

                    if (isExecutable)
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }
}
