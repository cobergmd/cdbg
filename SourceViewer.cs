using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    class SourceViewer : OutputBuffer
    {
        public SourceViewer(string name) 
            : base(name)
        {
        }

        public override void Draw()
        {
            int top = Console.WindowTop;
            int bottom = Console.WindowHeight - 3;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
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
                    string linenum = (idx + 1).ToString().PadLeft(4);
                    if (i == HighLight - 1)
                        Console.BackgroundColor = ConsoleColor.Yellow;

                    Console.Write(String.Format("{0} {1}", linenum, _Lines[idx]));

                    if (i == HighLight - 1)
                        Console.BackgroundColor = currentBGC;
                }
            }

            Console.ResetColor();
        }
    }
}
