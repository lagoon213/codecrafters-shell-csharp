class Program
{
    static void Main()
    {
        bool isRunning = true;

        string exitCommand = "exit";
        string echoCommand = "echo";
    
        while (isRunning)
        {
            Console.Write("$ ");

            string command = Console.ReadLine();
            if (command.StartsWith(exitCommand))
            {
                break;
            }
            if (command.StartsWith(echoCommand))
            {
                Console.WriteLine(command.Substring(echoCommand.Length + 1));
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
        }
    }
}
