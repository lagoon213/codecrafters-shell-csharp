using System.Diagnostics;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Text;

class Program
{
    static void Main()
    {
        bool isRunning = true;

        string[] builtInCommands = new string[]
        {
            "exit",
            "echo",
            "type",
            "pwd",
            "cd"
        };

        string[] redirectOperator = new string[]
        {
            ">",
            "1>"
        };

        while (isRunning)
        {
            Console.Write("$ ");

            string command = Console.ReadLine();
            string fileName = null;
            string output = null;
            bool shouldRedirect = false;

            List<string> Arguments = CheckCommandForQuotesAndReturnArguments(command);


            if (Arguments[0] == "exit")
            {
                break;
            }
            if (redirectOperator.Any(op => Arguments.Contains(op)))
            {
                string? foundOperator =
                    redirectOperator.FirstOrDefault(op => Arguments.Contains(op));

                if (foundOperator != null)
                {
                    shouldRedirect = true;

                    int index = Arguments.IndexOf(foundOperator);

                    fileName = Arguments[index + 1];

                    Arguments.RemoveRange(index, Arguments.Count - index);
                }
            }
            if (Arguments[0] == "type")
            {
                string commandType = Arguments[1];
                
                if (builtInCommands.Contains(commandType))
                {
                    if (shouldRedirect)
                    {
                        File.WriteAllText(fileName, $"{commandType} is a shell builtin" + Environment.NewLine);
                    }
                    Console.WriteLine($"{commandType} is a shell builtin");
                }
                else if (GetExecutablePath(commandType) is string executablePath)
                {
                    if (shouldRedirect)
                    {
                        File.WriteAllText(fileName, $"{commandType} is {executablePath}" + Environment.NewLine);
                    }
                    Console.WriteLine($"{commandType} is {executablePath}");
                }
                else
                {
                    if (shouldRedirect)
                    {
                        File.WriteAllText(fileName, $"{commandType}: not found" + Environment.NewLine);
                    }
                    Console.WriteLine($"{commandType}: not found");
                }
            }
            else if (Arguments[0] == "echo")
            {
                string echoOutput = string.Join(" ", Arguments.Skip(1));

                if (shouldRedirect)
                {
                    File.WriteAllText(fileName, echoOutput + Environment.NewLine);
                }
                else
                {
                    Console.WriteLine(echoOutput);
                }
            }
            else if (Arguments[0] == "pwd")
            {
                var PwdOutput = Directory.GetCurrentDirectory();
                if (shouldRedirect)
                {
                    File.WriteAllText(fileName, PwdOutput + Environment.NewLine);
                }
                Console.WriteLine(PwdOutput);
            }
            else if (Arguments[0] == "cd")
            {
                string path = Arguments[1];
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
            else if (GetExecutablePath(Arguments[0]) is string executablePath)
            {        
                var startInfo = new ProcessStartInfo();

                startInfo.FileName = Arguments[0];

                for (int i = 1; i < Arguments.Count; i++)
                {
                    startInfo.ArgumentList.Add(Arguments[i]);
                }

                if (shouldRedirect)
                {
                    startInfo.RedirectStandardOutput = true;
                }

                Process process = Process.Start(startInfo);

                if (shouldRedirect)
                {
                    string processOutput = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();

                    File.WriteAllText(fileName, processOutput);
                }
                process.WaitForExit();
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
        }
    }

    public static List<string> CheckCommandForQuotesAndReturnArguments(string command)
    {
        
            bool isSingleQuotes = false;
            bool isDoubleQuotes = false;
            bool escapeNext = false;
            List<string> Arguments = new();
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
                        Arguments.Add(currentArgument.ToString());
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
                Arguments.Add(currentArgument.ToString());
            }

            return Arguments;
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
