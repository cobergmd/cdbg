using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    public class Screen : IMDbgIO, IMDbgIO2
    {
        private string[] _MainMenu = new string[] { "Help  ",
                                                 "View",
                                                 "      ",
                                                 "Quit  ",
                                                 "Run   ",
                                                 "Attach",
                                                 "      ",
                                                 "Out   ",
                                                 "SetBrk",
                                                 "Over  ",
                                                 "Into  ",
                                                 "Menu  "};

        private string[] _AltMenu = new string[] { "Help  ",
                                                 "      ",
                                                 "      ",
                                                 "Edit  ",
                                                 "Stop  ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "Menu  "};

        private bool _Running = false;
        private string[] _CurrentMenu = null;
        private List<string> _CmdHistory = new List<string>();
        private int _CmdHistoryIdx = 0;
        private string _SelectedCmd = null;

        private FileList _FileList;
        private CommandPrompt _Prompt = new CommandPrompt();
        private CommandViewer _CommandView = new CommandViewer("#cmd#");
        private VariableViewer _VariableView;
        private OutputBuffer _CurrentView = null;
        private List<OutputBuffer> _Buffers = new List<OutputBuffer>();
        private int _CurrentBufferIdx = -1;

        private IMDbgShell _Shell = null;
        private IMDbgIO _OldIo = null;

        public Screen(IMDbgShell shell)
        {
            Console.WindowWidth = 100;
            _Shell = shell;
            _OldIo = _Shell.IO;
            _CurrentMenu = _MainMenu;
            _FileList = new FileList(shell, Console.WindowHeight - 6);
            _VariableView = new VariableViewer("#var#", _Shell);
            _Buffers.Add(_CommandView);
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
            if (_CurrentView == _CommandView)
            {
                _CommandView.Clear();
                _CommandView.Draw();
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
                    _CurrentView = buffer;
                    break;
                }
            }

            if (!exists)
            {
                _CurrentView = new SourceViewer(path);
                string line;
                using (var reader = File.OpenText(path))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        _CurrentView.Append(line);
                    }
                }

                _Buffers.Add(_CurrentView);
                _CurrentBufferIdx = _Buffers.Count - 1;
            }
            if (_CurrentView != null)
            {
                _CurrentView.HighLight = highlight;
                _CurrentView.Draw();
            }
        }

        public void Draw()
        {
            int winHeight = Console.WindowHeight;
            DrawMenu(winHeight - 1);
            _Prompt.Draw(winHeight - 2);

            bool refreshViewer = false;
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
                            _Prompt.Load(_SelectedCmd);
                            _SelectedCmd = null;
                        }
                        _FileList.Close();
                    }
                    ProcessCommandLine();
                }
                else if (cki.Key == ConsoleKey.Tab && (cki.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    if (_CurrentBufferIdx == _Buffers.Count-1) 
                        _CurrentBufferIdx = 0;
                    else
                        _CurrentBufferIdx += 1;

                    _CurrentView = _Buffers[_CurrentBufferIdx];
                    refreshViewer = true;
                }
                else if (cki.Key == ConsoleKey.Tab)
                {
                    if (_FileList.IsVisible && _SelectedCmd != null)
                    {
                        _Prompt.Load(_SelectedCmd);
                        _FileList.Close();
                        _SelectedCmd = null;
                        refreshViewer = true;
                    }
                }
                else if (cki.Key == ConsoleKey.F2)
                {
                    ShowVariableViewer();
                }
                else if (cki.Key == ConsoleKey.F4)
                {
                    Stop();
                }
                else if (cki.Key == ConsoleKey.F10)
                {
                    string cmdArgs = null;
                    IMDbgCommand dbgcmd = null;
                    _Shell.Commands.ParseCommand("n", out dbgcmd, out cmdArgs);
                    dbgcmd.Execute(cmdArgs);
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
                    _Prompt.MoveToHome();
                }
                else if (cki.Key == ConsoleKey.End)
                {
                    _Prompt.MoveToEnd();
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    _Prompt.Backspace();
                    refreshList = true;
                }
                else if (cki.Key == ConsoleKey.Delete)
                {
                    _Prompt.Delete();
                    refreshList = true;
                }
                else if (cki.Key == ConsoleKey.RightArrow)
                {
                    _Prompt.MoveRight();
                }
                else if (cki.Key == ConsoleKey.LeftArrow)
                {
                    _Prompt.MoveLeft();
                }
                else if (cki.Key == ConsoleKey.Escape)
                {
                    _FileList.Close();
                    refreshViewer = true;
                }
                else if (cki.Key == ConsoleKey.UpArrow)
                {
                    if ((cki.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        if (_CurrentView != null)
                        {
                            _CurrentView.DecreaseLine(1);
                            refreshViewer = true;
                        }
                    }
                    else
                    {
                        if (_FileList.IsVisible)
                        {
                            _SelectedCmd = _FileList.MoveSelection(FileList.ArrowMovement.Up);
                            refreshList = true;
                        }
                        else
                        {
                            // update prompt with command history
                            _Prompt.Load(GetNextCommand());
                        }
                    }
                }
                else if (cki.Key == ConsoleKey.DownArrow)
                {
                    if ((cki.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        if (_CurrentView != null)
                        {
                            _CurrentView.IncreaseLine(1);
                            refreshViewer = true;
                        }
                    }
                    else
                    {
                        if (_FileList.IsVisible)
                        {
                            _SelectedCmd = _FileList.MoveSelection(FileList.ArrowMovement.Down);
                            refreshList = true;
                        }
                        else
                        {
                            // update prompt with command history
                            _Prompt.Load(GetPreviousCommand());
                        }
                    }
                }
                else if (cki.Key == ConsoleKey.PageDown)
                {
                    if (_CurrentView != null)
                    {
                        _CurrentView.IncreasePage();
                        refreshViewer = true;
                    }
                }
                else if (cki.Key == ConsoleKey.PageUp)
                {
                    if (_CurrentView != null)
                    {
                        _CurrentView.DecreasePage();
                        refreshViewer = true;
                    }
                }
                else // add to buffer
                {
                    _Prompt.AddCharacter(cki.KeyChar);
                    refreshList = true;
                }
            }
            if (refreshList)
            {
                _FileList.Draw(_Prompt.ToString());
            }
            if (_CurrentView != null && refreshViewer)
            {
                _CurrentView.Draw();
            }
        }

        private void ShowVariableViewer()
        {
            _CurrentView = _VariableView;
            _CurrentView.Draw();
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
                string cmdLine = _Prompt.ToString();
                if (string.IsNullOrEmpty(cmdLine)) return;

                _CmdHistory.Add(cmdLine);
                _CmdHistoryIdx = 0;
                _Shell.Commands.ParseCommand(cmdLine, out dbgcmd, out cmdArgs);
                _Prompt.EraseToEnd();
                dbgcmd.Execute(cmdArgs);
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
            _CommandView.Append(output);
            _CurrentView = _CommandView;
            _CurrentView.Draw();
        }

        public bool ReadCommand(out string command)
        {                                      
            return _OldIo.ReadCommand(out command);   
        }

        public void WriteOutput(string outputType, string message, int highlightStart, int highlightLen)
        {
            _CommandView.Append(message);
            _CurrentView = _CommandView;
            _CurrentView.Draw();
        }
    }
}
