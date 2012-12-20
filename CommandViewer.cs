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

        public override void Draw()
        {
            int top = Console.WindowTop;
            int bottom = Console.WindowHeight - 3;

            Console.ForegroundColor = ConsoleColor.DarkGray;

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
            Console.ResetColor();
        }
    }
}
