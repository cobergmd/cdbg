using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Samples.Tools.Mdbg;

namespace cmd
{
    public class Screen : IMDbgIO
    {
        private string[] _MainMenu = new string[] { "Help  ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "Debug ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "      ",
                                                 "Quit " };

        private string[] _AltMenu = new string[] { "Help  ",
                                                 "      ",
                                                 "View  ",
                                                 "Edit  ",
                                                 "Stop  ",
                                                 "Into  ",
                                                 "Over  ",
                                                 "Out   ",
                                                 "Sym   ",
                                                 "Quit " };

        private InputBuffer _InputBuffer = new InputBuffer();
        private FileList _FileList;
        private int _CursorPosition = 0;

        private OutputBuffer _CommandBuffer = new OutputBuffer("#cmd#", ConsoleColor.DarkGray);
        private OutputBuffer _CurrentBuffer = null;
        private List<OutputBuffer> _Buffers = new List<OutputBuffer>();
        private int _CurrentBufferIdx = -1;

        private IMDbgShell _Shell = null;
        private IMDbgIO _OldIo = null;
        private bool _Running = false;
        private string[] _CurrentMenu = null;

        public Screen(IMDbgShell shell)
        {
            _Shell = shell;
            _OldIo = _Shell.IO;
            _CurrentMenu = _MainMenu;
            _FileList = new FileList(shell);

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

        public void DisplayFile(string filename)
        {
            string path = Directory.GetCurrentDirectory() + "/" + filename;

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
                _CurrentBuffer = new OutputBuffer(path, ConsoleColor.DarkGreen);
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
            int commandLineOffset = Console.CursorLeft;
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
                    _CursorPosition = commandLineOffset;
                }
                else if (cki.Key == ConsoleKey.End)
                {
                    _CursorPosition = commandLineOffset + _InputBuffer.Length;
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    int bufpos = _CursorPosition - commandLineOffset;
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
                    int bufpos = _CursorPosition - commandLineOffset;
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
                    _FileList.Draw(_InputBuffer, FileList.ArrowMovement.Up);
                    if (_FileList.SelectedValue != null)
                    {
                        int len = _FileList.SelectedValue.Length;
                        _InputBuffer.Load(_FileList.PartialValue);
                    }
                }
                else if (cki.Key == ConsoleKey.DownArrow)
                {
                    _FileList.Draw(_InputBuffer, FileList.ArrowMovement.Down);
                    if (_FileList.SelectedValue != null)
                    {
                        int len = _FileList.SelectedValue.Length;
                        _InputBuffer.Load(_FileList.PartialValue);
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
                    int bufpos = _CursorPosition - commandLineOffset;
                    _InputBuffer.Insert(cki.KeyChar, bufpos - 1);
                    refreshList = true;
                }
            }
            if (refreshList)
            {
                _FileList.Draw(_InputBuffer, FileList.ArrowMovement.None);
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
            int idx = 0;
            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(idx, position);
                idx = idx + 8;
                Console.Write(i + 1);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.Write(_CurrentMenu[i]);
                Console.ResetColor();
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

                _Shell.Commands.ParseCommand(cmdLine, out dbgcmd, out cmdArgs);
                dbgcmd.Execute(cmdArgs);

                _InputBuffer.Clear();
                _CursorPosition = 0;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    WriteError(ex.InnerException.Message);
                else
                    WriteError(ex.Message);
            }
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
    }
}
