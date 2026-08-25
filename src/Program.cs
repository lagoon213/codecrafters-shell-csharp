using System.Diagnostics;

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

            if (command.StartsWith("exit"))
            {
                break;
            }
            else if (command.StartsWith("type"))
            {
                string commandType = command.Substring("type".Length + 1);
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
            else if (command.StartsWith("echo"))
            {
                Console.WriteLine(command.Substring("echo".Length + 1));
            }
            else if (GetExecutablePath(command.Split(' ')[0]) is string executablePath)
            {
                string[] arguments = command.Split(' ');
                
                var startInfo = new ProcessStartInfo();

                startInfo.FileName = arguments[0];

                for (int i = 1; i < arguments.Length; i++)
                {
                    startInfo.ArgumentList.Add(arguments[i]);
                }

                Process process = Process.Start(startInfo);
                process.WaitForExit();
            }
            else if (command.StartsWith("pwd"))
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
            }
            else if (command.StartsWith("cd"))
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
