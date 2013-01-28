// CDBG - A console extension for the Microsoft MDBG debugger
// Copyright (c) 2013 Craig Oberg
// Licensed under the MIT License (MIT) http://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    class CommandViewer : OutputBuffer
    {
        public CommandViewer(string name) 
            : base(name)
        {
            this.Append("********** Command Output Buffer **********");
        }

        public override void Draw(int position, int height)
        {
            int top = Console.WindowTop;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            int count = _Lines.Count;
            for (int i = position; i <= height; i++)
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
            Console.ResetColor();
        }
    }
}
