class Program
{
    static void Main()
    {
        bool isRunning = true;

        string exitCommand = "exit";
    
        while (isRunning)
        {
            Console.Write("$ ");

            string command = Console.ReadLine();
            

            if (command == exitCommand)
            {
                isRunning = false;
            }
            else
            {
                Console.WriteLine($"{command}: command not found");
            }
        }
    }
}
