using System;
using System.IO;
using System.Threading;
using System.Security.Permissions;

using Microsoft.Samples.Tools.Mdbg;

[assembly:CLSCompliant(true)]

namespace cjomd.Mdbg.Extensions.Cdbg
{
    // extension class name must have [MDbgExtensionEntryPointClass] attribute on it and implement a LoadExtension()
    [MDbgExtensionEntryPointClass(
        Url = "http://blogs.msdn.com/jmstall",
        ShortDescription = "Console MDbg extension."
    )]
    class cdbg : CommandBase
    {
        private static Screen _console;

        public static void LoadExtension()
        {
            try
            {
                MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands, typeof(cdbg));
            }
            catch
            {
                // we'll ignore errors about multiple defined gui command in case gui is loaded
                // multiple times.
            }

            Con("");
        }

        [CommandDescription(
            CommandName = "con",
            ShortHelp = "con [close] - starts/closes a console interface",
            LongHelp =  "Usage: con [close]" 
        )]
        public static void Con(string args)
        {
            ArgParser ap = new ArgParser(args);
            if (ap.Exists(0))
            {
                if (ap.AsString(0) == "close")
                {
                    if (_console != null)
                    {
                        _console.Stop();
                        return;
                    }
                    else
                        throw new MDbgShellException("Console not started.");
                }
                else
                    throw new MDbgShellException("invalid argument");
            }

            _console = new Screen(Shell);
            _console.Start();
        }

        [CommandDescription(
            CommandName = "cd",
            ShortHelp = "cd [directory] - changes directory",
            LongHelp = "Usage: cd [directory]"
        )]
        public static void ChangeDirectory(string args)
        {
            try
            {
                Directory.SetCurrentDirectory(args);
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.Message);
            }
        }

        [CommandDescription(
            CommandName = "cls",
            ShortHelp = "cls - clear console",
            LongHelp = "Usage: cls"
        )]
        public static void ClearDisplay(string args)
        {
            _console.Clear();
        }

        [CommandDescription(
            CommandName = "open",
            ShortHelp = "open - open a file",
            LongHelp = "Usage: open [filename]"
        )]
        public static void OpenFile(string args)
        {
            string path = Directory.GetCurrentDirectory() + "/" + args;

            _console.DisplayFile(path, -1);
        }
    }
}
