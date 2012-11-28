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

        public string Name { get; set; }

        public OutputBuffer(string name, ConsoleColor textColor)
        {
            _TextColor = textColor;
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

            int count = _Lines.Count;
            for (int i = 0; i <= bottom; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
                Console.SetCursorPosition(0, i);
                if ((_LineOffset + i) <= (count - 1))
                    Console.Write(_Lines[i + _LineOffset]);
            }

            Console.ResetColor();
        }
    }
}
