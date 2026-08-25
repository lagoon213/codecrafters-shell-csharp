class Program
{
    static void Main()
    {
        bool isRunning = true;
    
        while (isRunning)
        {
            Console.Write("$ ");

            string command = Console.ReadLine();
            Console.WriteLine($"{command}: command not found");
        }
    }
}
