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
