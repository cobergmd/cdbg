// CDBG - A console extension for the Microsoft MDBG debugger
// Copyright (c) 2013 Craig Oberg
// Licensed under the MIT License (MIT) http://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    class SourceViewer : OutputBuffer
    {
        private List<int> _Breakpoints = new List<int>();
        private bool _ForceHighlight = false;
        private int _Highlight;

        public int HighLight 
        { 
            set
            {
                _Highlight = value;
                _ForceHighlight = true;
            }
        }

        public SourceViewer(string name) 
            : base(name)
        {
        }

        public override void Draw(int position, int height)
        {
            int top = Console.WindowTop;

            // adjust position based on ip if we just stopped 
            if (_ForceHighlight)
            {
                if (_Highlight < _LineOffset ||
                    _Highlight > _LineOffset + height)
                {
                    _LineOffset = _Highlight - 5;
                }
                _ForceHighlight = false;
            }

            ConsoleColor currentBGC = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            int lineidx = _LineOffset;
            int count = _Lines.Count;

            for (int i = position; i <= height; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
                Console.SetCursorPosition(0, i);
                string linenum = (lineidx + 1).ToString().PadLeft(4);

                if (lineidx < _Lines.Count)
                {
                    if (_Breakpoints.Contains(lineidx + 1))
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    Console.Write(String.Format("{0}", linenum));
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.BackgroundColor = currentBGC;

                    if (lineidx == _Highlight - 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    Console.Write(String.Format(" {0}", _Lines[lineidx]));
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.BackgroundColor = currentBGC;

                    lineidx++;
                }
            }

            Console.ResetColor();
        }

        public void AddBreakpoint(int breakpoint)
        {
            _Breakpoints.Add(breakpoint);
        }
    }
}
