using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace cmd
{
    public class Screen : IMDbgIO, IMDbgIO2
    {
        private string[] _MainMenu = new string[] { "Help  ",
                                                 "Status",
                                                 "      ",
                                                 "Attach",
                                                 "Debug ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "SetBrk",
                                                 "Quit  ",
                                                 "      ",
                                                 "Menu  "};

        private string[] _AltMenu = new string[] { "Help  ",
                                                 "      ",
                                                 "View  ",
                                                 "Edit  ",
                                                 "Stop  ",
                                                 "      ",
                                                 "      ",
                                                 "Out   ",
                                                 "SetBrk",
                                                 "Over  ",
                                                 "Into  ",
                                                 "Menu  "};

        private InputBuffer _InputBuffer = new InputBuffer();
        private FileList _FileList;
        private int _CursorPosition = 0;

        private OutputBuffer _CommandBuffer = new OutputBuffer("#cmd#", ConsoleColor.DarkGray, false);
        private OutputBuffer _CurrentBuffer = null;
        private List<OutputBuffer> _Buffers = new List<OutputBuffer>();
        private int _CurrentBufferIdx = -1;

        private IMDbgShell _Shell = null;
        private IMDbgIO _OldIo = null;
        private bool _Running = false;
        private string[] _CurrentMenu = null;
        private List<string> _CmdHistory = new List<string>();
        private int _CmdHistoryIdx = 0;
        private string _SelectedCmd = null;

        public Screen(IMDbgShell shell)
        {
            Console.WindowWidth = 100;
            _Shell = shell;
            _OldIo = _Shell.IO;
            _CurrentMenu = _MainMenu;
            _FileList = new FileList(shell, Console.WindowHeight - 6);

            _CommandBuffer.Append("");
            _CommandBuffer.Append("********** Command Output Buffer **********");
            _Buffers.Add(_CommandBuffer);
        }

        public void Start()
        {
            _Shell.IO = this;
            Console.Clear();
            _Running = true;

            while (_Running)
            {
                Draw();
            }
        }

        public void Stop()
        {
            Console.Clear();
            _Running = false;
            _Shell.IO = _OldIo;
        }

        public void Clear()
        {
            if (_CurrentBuffer == _CommandBuffer)
            {
                _CommandBuffer.Clear();
                _CommandBuffer.Draw();
            }
            else
            {
                // write message that you can't clear file buffer contents
            }
        }

        public void DisplayFile(string path, int highlight)
        {
            bool exists = false;
            for (int i = 0; i < _Buffers.Count; i++)
            {
                OutputBuffer buffer = _Buffers[i];
                if (buffer.Name.Equals(path))
                {
                    exists = true;
                    _CurrentBufferIdx = i;
                    _CurrentBuffer = buffer;
                    break;
                }
            }

            if (!exists)
            {
                _CurrentBuffer = new OutputBuffer(path, ConsoleColor.DarkGreen, true);
                string line;
                using (var reader = File.OpenText(path))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        _CurrentBuffer.Append(line);
                    }
                }

                _Buffers.Add(_CurrentBuffer);
                _CurrentBufferIdx = _Buffers.Count-1;
            }
            if (_CurrentBuffer != null)
            {
                _CurrentBuffer.HighLight = highlight;
                _CurrentBuffer.Draw();
            }
        }

        public void Draw()
        {
            int height = Console.WindowHeight;
            int width = Console.WindowWidth;

            DrawMenu(height);

            // draw prompt
            Console.SetCursorPosition(0, height - 1);
            Console.Write(Directory.GetCurrentDirectory() + ">");
            if (_CursorPosition == 0) _CursorPosition = Console.CursorLeft;
            if (_CursorPosition >= width) _CursorPosition = width - 1;
            int commandLineBegin = Console.CursorLeft;
            int commandLineLen = width - commandLineBegin;

            Console.Write(_InputBuffer.ToString());
            Console.SetCursorPosition(_CursorPosition, height - 1);

            bool refreshPrompt = false;
            bool refreshBuffer = false;
            bool refreshList = false;
            ConsoleKeyInfo cki = Console.ReadKey(true);
            if (cki != null)
            {
                if (cki.Key == ConsoleKey.Enter) // evaluate
                {
                    if (_FileList.IsVisible)
                    {
                        if (_SelectedCmd != null)
                        {
                            _InputBuffer.Clear();
                            _InputBuffer.Load(_SelectedCmd);
                            _SelectedCmd = null;
                        }
                        _FileList.Close();
                    }
                    ProcessCommandLine();
                    refreshPrompt = true;
                }
                else if (cki.Key == ConsoleKey.Tab && (cki.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    if (_CurrentBufferIdx == _Buffers.Count-1) 
                        _CurrentBufferIdx = 0;
                    else
                        _CurrentBufferIdx += 1;

                    _CurrentBuffer = _Buffers[_CurrentBufferIdx];
                    refreshBuffer = true;
                }
                else if (cki.Key == ConsoleKey.Tab)
                {
                    if (_FileList.IsVisible && _SelectedCmd != null)
                    {
                        _InputBuffer.Clear();
                        _InputBuffer.Load(_SelectedCmd);
                        _CursorPosition = commandLineBegin + _InputBuffer.Length;
                        _FileList.Close();
                        _SelectedCmd = null;
                        refreshBuffer = true;
                        refreshPrompt = true;
                    }
                }
                else if (cki.Key == ConsoleKey.F10)
                {
                    Stop();
                }
                else if (cki.Key == ConsoleKey.F12)
                {
                    if (_CurrentMenu == _AltMenu)
                        _CurrentMenu = _MainMenu;
                    else
                        _CurrentMenu = _AltMenu;
                }
                else if (cki.Key == ConsoleKey.Home)
                {
                    _CursorPosition = commandLineBegin;
                }
                else if (cki.Key == ConsoleKey.End)
                {
                    _CursorPosition = commandLineBegin + _InputBuffer.Length;
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    int bufpos = _CursorPosition - commandLineBegin;
                    if (bufpos > 0)
                    {
                        _InputBuffer.Remove(bufpos - 1);
                        _CursorPosition--;
                        refreshPrompt = true;
                        refreshList = true;
                    }
                }
                else if (cki.Key == ConsoleKey.Delete)
                {
                    int bufpos = _CursorPosition - commandLineBegin;
                    if (bufpos >= 0)
                    {
                        _InputBuffer.Remove(bufpos);
                        refreshPrompt = true;
                        refreshList = true;
                    }
                }
                else if (cki.Key == ConsoleKey.RightArrow)
                {
                    _CursorPosition++;
                }
                else if (cki.Key == ConsoleKey.LeftArrow)
                {
                    _CursorPosition--;
                }
                else if (cki.Key == ConsoleKey.Escape)
                {
                    _FileList.Close();
                    refreshBuffer = true;
                }
                else if (cki.Key == ConsoleKey.UpArrow)
                {
                    if (_FileList.IsVisible)
                    {
                        _SelectedCmd = _FileList.MoveSelection(FileList.ArrowMovement.Up);
                        refreshList = true;
                    }
                    else
                    {
                        // update prompt with command history
                        _InputBuffer.Clear();
                        _InputBuffer.Load(GetNextCommand());
                        _CursorPosition = commandLineBegin + _InputBuffer.Length;
                        refreshPrompt = true;
                    }
                }
                else if (cki.Key == ConsoleKey.DownArrow)
                {
                    if (_FileList.IsVisible)
                    {
                        _SelectedCmd = _FileList.MoveSelection(FileList.ArrowMovement.Down);
                        refreshList = true;
                    }
                    else
                    {
                        // update prompt with command history
                        _InputBuffer.Clear();
                        _InputBuffer.Load(GetPreviousCommand());
                        _CursorPosition = commandLineBegin + _InputBuffer.Length;
                        refreshPrompt = true;
                    }
                }
                else if (cki.Key == ConsoleKey.PageDown)
                {
                    if (_CurrentBuffer != null)
                    {
                        _CurrentBuffer.IncreasePage();
                        refreshBuffer = true;
                    }
                }
                else if (cki.Key == ConsoleKey.PageUp)
                {
                    if (_CurrentBuffer != null)
                    {
                        _CurrentBuffer.DecreasePage();
                        refreshBuffer = true;
                    }
                }
                else // add to buffer
                {
                    _CursorPosition++;
                    int bufpos = _CursorPosition - commandLineBegin;
                    _InputBuffer.Insert(cki.KeyChar, bufpos - 1);
                    refreshList = true;
                }
            }
            if (refreshList)
            {
                _FileList.Draw(_InputBuffer);
            }
            if (refreshPrompt)
            {
                Console.SetCursorPosition(0, height - 1);
                Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
            }
            if (_CurrentBuffer != null && refreshBuffer)
            {
                _CurrentBuffer.Draw();
            }
        }

        private void DrawMenu(int position)
        {
            Console.SetCursorPosition(0, position);
            int idx = 0;
            for (int i = 0; i < _CurrentMenu.Length; i++)
            {
                Console.Write(i + 1);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.Write(_CurrentMenu[i]);
                Console.ResetColor();
                Console.Write(" ");
            }
        }

        private void ProcessCommandLine()
        {
            try
            {
                IMDbgCommand dbgcmd = null;
                string cmdArgs = null;
                string cmdLine = _InputBuffer.ToString();
                if (string.IsNullOrEmpty(cmdLine)) return;

                _CmdHistory.Add(cmdLine);
                _CmdHistoryIdx = 0;
                _Shell.Commands.ParseCommand(cmdLine, out dbgcmd, out cmdArgs);
                dbgcmd.Execute(cmdArgs);

                _InputBuffer.Clear();
                _CursorPosition = 0;

                // display current location if debugging
                DisplaySource();
            }
            catch (Exception ex)
            {                                                 
                if (ex.InnerException != null)
                    WriteError(ex.InnerException.Message);
                else
                    WriteError(ex.Message);
            }
        }

        private void DisplaySource()
        {
            if (!_Shell.Debugger.Processes.HaveActive)
            {
                //CommandBase.WriteOutput("STOP: Process Exited");
                return; // don't try to display current location
            }
            else
            {
                Object stopReason = _Shell.Debugger.Processes.Active.StopReason;
                Type stopReasonType = stopReason.GetType();
                if (stopReasonType == typeof(StepCompleteStopReason))
                {
                    // just ignore those
                }
            }

            if (!_Shell.Debugger.Processes.Active.Threads.HaveActive)
            {
                return;  // we won't try to show current location
            }

            MDbgThread thr = _Shell.Debugger.Processes.Active.Threads.Active;
            MDbgSourcePosition pos = thr.CurrentSourcePosition;
            if (pos == null)
            {
                MDbgFrame f = thr.CurrentFrame;
                if (f.IsManaged)
                {
                    CorDebugMappingResult mappingResult;
                    uint ip;
                    f.CorFrame.GetIP(out ip, out mappingResult);
                    string s = "IP: " + ip + " @ " + f.Function.FullName + " - " + mappingResult;
                    CommandBase.WriteOutput(s);
                }
                else
                {
                    CommandBase.WriteOutput("<Located in native code.>");
                }
            }
            else
            {
                string fileLoc = _Shell.FileLocator.GetFileLocation(pos.Path);
                if (fileLoc == null)
                {
                    // Using the full path makes debugging output inconsistant during automated test runs.
                    // For testing purposes we'll get rid of them.
                    //CommandBase.WriteOutput("located at line "+pos.Line + " in "+ pos.Path);
                    CommandBase.WriteOutput("located at line " + pos.Line + " in " + System.IO.Path.GetFileName(pos.Path));
                }
                else
                {
                    IMDbgSourceFile file = _Shell.SourceFileMgr.GetSourceFile(fileLoc);
                    string prefixStr = pos.Line.ToString(CultureInfo.InvariantCulture) + ":";

                    if (pos.Line < 1 || pos.Line > file.Count)
                    {
                        CommandBase.WriteOutput("located at line " + pos.Line + " in " + pos.Path);
                        throw new MDbgShellException(string.Format("Could not display current location; file {0} doesn't have line {1}.",
                                                                   file.Path, pos.Line));
                    }

                    string lineContent = file[pos.Line];

                    DisplayFile(file.Path, pos.Line);
                }
            }
        }

        private string GetPreviousCommand()
        {
            if (_CmdHistory.Count == 0) return null;

            _CmdHistoryIdx--;
            if (_CmdHistoryIdx < 0) _CmdHistoryIdx = _CmdHistory.Count() - 1;
            return _CmdHistory[_CmdHistoryIdx];
        }

        private string GetNextCommand()
        {
            if (_CmdHistory.Count == 0) return null;

            _CmdHistoryIdx++;
            if (_CmdHistoryIdx > _CmdHistory.Count() - 1) _CmdHistoryIdx = 0;
            return _CmdHistory[_CmdHistoryIdx];
        }

        public void WriteError(string message)
        {
            Console.SetCursorPosition(0, Console.WindowTop);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error: " + message);
            Console.ResetColor();
        }

        public void WriteOutput(string outputType, string output)
        {
            _CommandBuffer.Append(output);
            _CurrentBuffer = _CommandBuffer;
            _CurrentBuffer.Draw();
        }

        public bool ReadCommand(out string command)
        {                                      
            return _OldIo.ReadCommand(out command);   
        }

        public void WriteOutput(string outputType, string message, int highlightStart, int highlightLen)
        {
            _CommandBuffer.Append(message);
            _CurrentBuffer = _CommandBuffer;
            _CurrentBuffer.Draw();
        }
    }
}
