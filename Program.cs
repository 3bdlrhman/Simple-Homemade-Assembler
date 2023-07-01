namespace Assembler
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Please inter the Assembly file path: \n");
            string inputFile = Console.ReadLine();

            
            while (!File.Exists(inputFile))
            {
                Console.WriteLine("Please inter the Assembly file path or press CTRL C to exist \n");
                inputFile = Console.ReadLine();
            }

            Console.WriteLine("Please inter the output file name: \n");
            string outFile = Console.ReadLine();

            Parser parser = new Parser(inputFile);
            parser.ScanLabels();
            parser.ScanInstructions();

            
            // Now we should write output to file
            ListFileWriter writer = new("E:\\Projects\\", outFile, parser.GetInstructions());
            writer.WriteToFile();
        }
    }
}