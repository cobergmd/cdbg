using System;
using System.Collections;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    class VariableViewer : OutputBuffer
    {
        private IMDbgShell _Shell = null;

        public VariableViewer(string name, IMDbgShell shell) 
            : base(name)
        {
            _Shell = shell;
            this.Append("********** Variable Buffer **********");
        }

        public override void Draw()
        {
            // get all active variables
            MDbgFrame frame = _Shell.Debugger.Processes.Active.Threads.Active.CurrentFrame;
            MDbgFunction f = frame.Function;

            ArrayList vars = new ArrayList();
            MDbgValue[] vals = f.GetActiveLocalVars(frame);
            if (vals != null)
            {
                vars.AddRange(vals);
            }

            vals = f.GetArguments(frame);
            if (vals != null)
            {
                vars.AddRange(vals);
            }
            Console.SetCursorPosition(0, Console.WindowHeight - 3);
            foreach (MDbgValue v in vars)
            {
                Console.WriteLine(v.Name + "=" + v.GetStringValue(0, true));
            }
        }
    }
}
