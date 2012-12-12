using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cmd
{
    public class OutputBuffer
    {
        private List<string> _Lines = new List<string>();
        private int _LineOffset = 0;
        private ConsoleColor _TextColor = ConsoleColor.DarkGreen;
        private bool _ShowLineNumbers = false;
        
        public string Name { get; set; }
        public int HighLight { get; set; }

        public OutputBuffer(string name, ConsoleColor textColor, bool showLineNumbers)
        {
            _TextColor = textColor;
            _ShowLineNumbers = showLineNumbers;
            this.Name = name;
        }

        public void IncreasePage()
        {
            int pagesize = ((Console.WindowHeight - 2) - Console.WindowTop) + 1;
            _LineOffset += pagesize;
        }

        public void DecreasePage()
        {
            int pagesize = ((Console.WindowHeight - 2) - Console.WindowTop) + 1;
            if (pagesize < _LineOffset)
            {
                _LineOffset -= pagesize;
            }
            else
            {
                _LineOffset = 0;
            }
        }

        public void IncreaseLine(int count)
        {
            _LineOffset += count;
        }

        public void DecreaseLine(int count)
        {
            if (count < _LineOffset)
            {
                _LineOffset -= count;
            }
            else
            {
                _LineOffset = 0;
            }
        }

        public void Append(string line)
        {
            _Lines.Add(line);
        }

        public void Clear()
        {
            _Lines.Clear();
        }

        public void Draw()
        {
            int top = Console.WindowTop;
            int bottom = Console.WindowHeight - 2;

            Console.ForegroundColor = _TextColor;
            ConsoleColor currentBGC = Console.BackgroundColor;

            int count = _Lines.Count;
            for (int i = 0; i <= bottom; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
                Console.SetCursorPosition(0, i);
                if ((_LineOffset + i) <= (count - 1))
                {
                    int idx = i + _LineOffset;
                    string linenum = idx.ToString().PadLeft(4);
                    if (i == HighLight-1)
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    else
                        Console.BackgroundColor = currentBGC;

                    Console.Write(String.Format("{0} {1}", _ShowLineNumbers ? linenum : "", _Lines[idx]));
                }
            }

            Console.ResetColor();
        }
    }
}
