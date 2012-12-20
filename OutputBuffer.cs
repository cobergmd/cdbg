﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    public class OutputBuffer
    {
        protected List<string> _Lines = new List<string>();
        protected int _LineOffset = 0;
        
        public string Name { get; set; }
        public int HighLight { get; set; }

        public OutputBuffer(string name)
        {
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

        virtual public void Draw()
        {
            int bottom = Console.WindowHeight - 2;
            int count = _Lines.Count;

            for (int i = 0; i <= bottom; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
                Console.SetCursorPosition(0, i);
                if ((_LineOffset + i) <= (count - 1))
                {
                    int idx = i + _LineOffset;
                    Console.Write(String.Format("{0}", _Lines[idx]));
                }
            }
        }
    }
}
