class Program
{
    static void Main()
    {
        bool isRunning = true;

        string[] builtInCommands = new string[]
        {
            "exit",
            "echo",
            "type"
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
                else
                {
                    var path = Environment.GetEnvironmentVariable("PATH");

                    bool found = false;

                    foreach (var dir in path.Split(Path.PathSeparator))
                    {
                        var fullPath = Path.Combine(dir, commandType);
                        if (File.Exists(fullPath))
                        {
                            UnixFileMode fileMode = File.GetUnixFileMode(fullPath);

                            bool isExecutable = (fileMode & UnixFileMode.UserExecute) != 0 ||
                                                (fileMode & UnixFileMode.GroupExecute) != 0 ||
                                                (fileMode & UnixFileMode.OtherExecute) != 0;

                            if (isExecutable)
                            {
                                Console.WriteLine($"{commandType} is {fullPath}");
                                found = true;  
                            }
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine($"{commandType}: not found");
                    }
                }
            }
            else if (command.StartsWith("echo"))
            {
                Console.WriteLine(command.Substring("echo".Length + 1));
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
        }
    }
}
