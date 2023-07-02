namespace Assembler
{
    internal class ListFileWriter
    {
        private readonly string _path;
        private readonly string _filename;
        private List<string> _lines;

        /// This Class Writes a List to a File
        public ListFileWriter(string path, string filename, List<string> lines)
        {
            _path = path;
            _filename = filename;
            _lines = lines;
        }

        public void WriteToFile()
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);


            string PATH = Path.Combine(_path, _filename);

            
            if (!File.Exists(PATH))
                File.Create(PATH).Close();


            StreamWriter writer = new StreamWriter(PATH);
            

            foreach (string line in _lines)
            {
                writer.WriteLine(line);
                writer.Flush();
            }
        }
    }
}
