using System.Diagnostics;
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

        
    
        while (isRunning)
        {
            Console.Write("$ ");

            string command = Console.ReadLine();

            bool isSingleQuotes = false;
            List<string> Arguments = new();
            StringBuilder currentArgument = new();

            foreach (char c in command)
            {
                if (c == '\'')
                {
                    isSingleQuotes = !isSingleQuotes;
                }
                else if (c == ' ' && !isSingleQuotes)
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


            if (Arguments[0] == "exit")
            {
                break;
            }
            else if (Arguments[0] == "type")
            {
                string commandType = Arguments[1];
                if (builtInCommands.Contains(commandType))
                {
                    Console.WriteLine($"{commandType} is a shell builtin");
                }
                else if (GetExecutablePath(commandType) is string executablePath)
                {
                    Console.WriteLine($"{commandType} is {executablePath}");
                }
                else
                {
                    Console.WriteLine($"{commandType}: not found");
                }
            }
            else if (Arguments[0] == "echo")
            {
                Console.WriteLine(string.Join(" ", Arguments.Skip(1)));
            }
            else if (Arguments[0] == "pwd")
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
            }
            else if (Arguments[0] == "cd")
            {
                string path = command.Split(' ')[1];
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

                Process process = Process.Start(startInfo);
                process.WaitForExit();
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
        }
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
