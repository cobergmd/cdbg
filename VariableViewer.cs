// CDBG - A console extension for the Microsoft MDBG debugger
// Copyright (c) 2013 Craig Oberg
// Licensed under the MIT License (MIT) http://opensource.org/licenses/MIT

using System;
using System.Collections;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    class VariableViewer : OutputBuffer
    {
        const char cellHorizontalLine = '─';
        const string cellVerticalLine = "│";
        const string cellHorizontalJointTop = "┬";

        private IMDbgShell _Shell = null;

        public VariableViewer(IMDbgShell shell, string name)
            : base(name)
        {
            _Shell = shell;
        }

        public override void Draw(int startY, int endY)
        {
            RefreshVarData();

            int top = Console.WindowTop;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            int count = _Lines.Count;
            int lineidx = 0;
            for (int i = startY; i <= endY; i++)
            {
                lineidx += _LineOffset;
                Console.SetCursorPosition(0, i);
                Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
                Console.SetCursorPosition(0, i);
                if ((lineidx) <= (count - 1))
                {
                    Console.Write(String.Format("{0}", _Lines[lineidx]));
                }
                lineidx++;
            }
            Console.ResetColor();
        }

        private void RefreshVarData()
        {
            _Lines.Clear();

            //this.Append(string.Empty.PadRight(Console.WindowWidth, cellHorizontalLine));
            this.Append(string.Empty.PadRight(29, cellHorizontalLine) +
                        cellHorizontalJointTop +
                        string.Empty.PadRight(39, cellHorizontalLine) +
                        cellHorizontalJointTop +
                        string.Empty.PadRight(30, cellHorizontalLine));

            // get all active variables
            MDbgFrame frame = _Shell.Debugger.Processes.Active.Threads.Active.CurrentFrame;
            MDbgFunction f = frame.Function;

            ArrayList vars = new ArrayList();
            MDbgValue[] vals = f.GetArguments(frame);
            if (vals != null)
            {
                vars.AddRange(vals);
            }

            vals = f.GetActiveLocalVars(frame);
            if (vals != null)
            {
                vars.AddRange(vals);
            }

            foreach (MDbgValue v in vars)
            {
                string name = v.Name.PadRight(29, ' ') + cellVerticalLine;
                string value = v.GetStringValue(0, true).PadRight(39, ' ') + cellVerticalLine;
                string type = v.TypeName.PadRight(30, ' ');

                this.Append(name + value + type);
            }
        }
    }
}
