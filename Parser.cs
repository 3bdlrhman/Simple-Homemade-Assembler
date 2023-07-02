namespace Assembler
{
    internal class Parser
    {
        private string _path;
        private int _currentInstructionCount;
        private int _variableMemory;

        private readonly Dictionary<string, int> _sympols;
        private readonly Dictionary<string, string> _ccontrols;
        private readonly Dictionary<string, string> _cdestinations;
        private readonly Dictionary<string, string> _cjumps;
        
        
        private List<string> _instructionsCode;
        public List<string> GetInstructions()
        {
            if (_instructionsCode != null)
                return _instructionsCode;

            return new List<string>();
        }


        public Parser(string filePath)
        {
            _path = filePath;

            _variableMemory = 16;
            _currentInstructionCount = 0;
            _instructionsCode = new List<string>();

            #region Init Predefined Labels Dictionary
            _sympols = new();
            _sympols.Add("SP", 0);
            _sympols.Add("LCL", 1);
            _sympols.Add("ARG", 2);
            _sympols.Add("THIS", 3);
            _sympols.Add("THAT", 4);
            _sympols.Add("R0", 0);
            _sympols.Add("R1", 1);
            _sympols.Add("R2", 2);
            _sympols.Add("R3", 3);
            _sympols.Add("R4", 4);
            _sympols.Add("R5", 5);
            _sympols.Add("R6", 6);
            _sympols.Add("R7", 7);
            _sympols.Add("R8", 8);
            _sympols.Add("R9", 9);
            _sympols.Add("R10", 10);
            _sympols.Add("R11", 11);
            _sympols.Add("R12", 12);
            _sympols.Add("R13", 13);
            _sympols.Add("R14", 14);
            _sympols.Add("R15", 15);
            _sympols.Add("SCREEN", 16384);
            _sympols.Add("KBD", 24576);

            #endregion

            #region Init C Instructions Dictionary
            _ccontrols = new();
            _ccontrols.Add("0", "0101010");
            _ccontrols.Add("1", "0111111");
            _ccontrols.Add("-1", "0111010");
            _ccontrols.Add("D", "0001100");
            _ccontrols.Add("A", "0110000");
            _ccontrols.Add("M", "1110000");
            _ccontrols.Add("!D", "0001101");
            _ccontrols.Add("!A", "0110001");
            _ccontrols.Add("!M", "1110001");
            _ccontrols.Add("-D", "0001111");
            _ccontrols.Add("-A", "0110011");
            _ccontrols.Add("D+1", "0011111");
            _ccontrols.Add("A+1", "0110111");
            _ccontrols.Add("M+1", "1110111");
            _ccontrols.Add("D-1", "0001110");
            _ccontrols.Add("A-1", "0110010");
            _ccontrols.Add("M-1", "1110010");
            _ccontrols.Add("D+A", "0000010");
            _ccontrols.Add("D-A", "0010011");
            _ccontrols.Add("A-D", "0000111");
            _ccontrols.Add("D&A", "0000000");
            _ccontrols.Add("D|A", "0010101");
            _ccontrols.Add("D+M", "1000010");
            _ccontrols.Add("D-M", "1010011");
            _ccontrols.Add("M-D", "1000111");
            _ccontrols.Add("D&M", "1000000");
            _ccontrols.Add("D|M", "1010101");

            #endregion

            #region Init Destination Dictionary
            _cdestinations = new();
            _cdestinations.Add("M", "001");
            _cdestinations.Add("D", "010");
            _cdestinations.Add("MD", "011");
            _cdestinations.Add("A", "100");
            _cdestinations.Add("AM", "101");
            _cdestinations.Add("AD", "110");
            _cdestinations.Add("AMD", "111");
            #endregion

            #region Init Jumps Dictionary
            _cjumps = new();
            _cjumps.Add("JGT", "001");
            _cjumps.Add("JEQ", "010");
            _cjumps.Add("JGE", "011");
            _cjumps.Add("JLT", "100");
            _cjumps.Add("JNE", "101");
            _cjumps.Add("JLE", "110");
            _cjumps.Add("JMP", "111");
            #endregion
        }

        /// Go through all instructions locate labels then add them with their value to the labels dictionary
        public void ScanLabels()
        {
            var fileRead = File.OpenRead(_path);
            using StreamReader reader = new(fileRead);

            while (!reader.EndOfStream)
            {
                var currentLine = reader.ReadLine()?.Split("//")[0].Trim();

                bool notComment = currentLine?.StartsWith("//") == false;
                bool notLabel = currentLine?.StartsWith('(') == false;
                bool notEmpty = string.IsNullOrEmpty(currentLine) == false;

                if (notComment && notLabel && notEmpty)
                    _currentInstructionCount++;
                
                if (currentLine?.StartsWith('(') == true)
                {
                    var sym = currentLine.Split('(', ')')[1];
                    _sympols.Add(sym, _currentInstructionCount);
                }
            }

            fileRead.Close();
        }

        /// Go through the instructions test whether its A-instruction or C-instruction then handle it
        public void ScanInstructions()
        {
            var fileRead = File.OpenRead(_path);
            using StreamReader reader = new(fileRead);


            while (!reader.EndOfStream)
            {
                var currentLine = reader.ReadLine()?.Split("//")[0].Trim();
                
                bool notComment = currentLine?.StartsWith("//") == false;
                bool notLabel = currentLine?.StartsWith('(') == false;
                bool notEmpty = string.IsNullOrEmpty(currentLine) == false;

                if (notComment && notLabel && notEmpty)
                {
                    if (IsA_Instruction(currentLine))
                    {
                        string A_Code = HandleA_Instruction(currentLine);
                        AddCode(A_Code);
                    }
                    else if (IsC_Instruction(currentLine))
                    {
                        string C_Code = HandleC_Instruction(currentLine);
                        AddCode(C_Code);
                    }
                }
            }

            fileRead.Close();
        }

        /// Test if the instruction is an A-instruction
        public bool IsA_Instruction(string instruction) 
        {
            return instruction.Trim().StartsWith('@');
        }
        
        /// Test if the instruction is a C-instruction
        public bool IsC_Instruction(string instruction) 
        {
            return !IsA_Instruction(instruction);
        }

        /// Handles A-instruction by getting the binary value of the integer
        public string HandleA_Instruction(string AInstruction)
        {
            var inst_string = AInstruction.Split('@')[1].Trim();
            if (int.TryParse(inst_string, out int resultInt))
            {
                // return 16-bit binary code
                return GetBinaryFromInt(resultInt);
            }
            else if (_sympols.ContainsKey(inst_string))
            {
                // get its value
                int value = _sympols[inst_string];
                
                // return 16-bit binary code
                return GetBinaryFromInt(value);
            }
            else
            {
                // assign it to the current memory and add it to symbols table
                _sympols.Add(inst_string, _variableMemory);
                int value = _variableMemory;
                
                // increase memory value _variableMemory++
                _variableMemory++;

                // return 16-bit binary code
                return GetBinaryFromInt(value);
            }
        }

        /// takes an integer and returns its binary value
        private string GetBinaryFromInt(int integer)
        {
            string binaryString = Convert.ToString(integer, 2).PadLeft(16, '0');
            binaryString.Insert(0, "0");
            return binaryString;
        }

        // gets the parts of a C-instruction then the value of each part
        public string HandleC_Instruction(string CInstruction)
        {
            //  1-bit [c-inst] 2-bits [no-use] 7-bits [control] 3-bits [dest] 3-bits [jump]

            string[] parts = GetParts(CInstruction);

            string controlBits = GetControls(parts[1]);
            string destBits = GetDestination(parts[0]);
            string jmpBits = GetJump(parts[2]);

            string code = $"111{controlBits}{destBits}{jmpBits}";
            return code;
        }

        /// c-instruction has three parts [controls-jumps-destination]
        private string[] GetParts(string command)
        {
            string[] result = new string[3];

            // if there is = get dest
            if (command.Contains("="))
            {
                result[0] = command.Split('=')[0];
                result[1] = command.Split('=')[1].Split(';')[0];
            }

            if (command.Contains(";"))
                result[2] = command.Split(';')[1];

            if (command.Contains(';') && !command.Contains('='))
                result[1] = command.Split(';')[0];

            if (!command.Contains(';') && !command.Contains('='))
                result[1] = command;

            return result;
        }

        #region Get Part Code

        private string GetControls(string key)
        {
            if (key != null && _ccontrols.ContainsKey(key))
            {
                return _ccontrols[key];
            }

            return "0000000";
        }


        private string GetDestination(string key)
        {
            if (key != null && _cdestinations.ContainsKey(key))
            {
                return _cdestinations[key];
            }

            return "000";
        }


        private string GetJump(string key)
        {
            if (key != null && _cjumps.ContainsKey(key))
            {
                return _cjumps[key];
            }

            return "000";
        }

        #endregion

        private void AddCode(string code)
        {
            _instructionsCode.Add(code);
        }


        public void PrintInstructions()
        {
            foreach (string v in _instructionsCode)
            {
                Console.WriteLine(v);
            }
        }
    }
}
