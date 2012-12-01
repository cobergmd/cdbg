using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Samples.Tools.Mdbg;

namespace cmd
{
    public class FileList
    {
        const string cellLeftTop = "┌";
        const string cellRightTop = "┐";
        const string cellLeftBottom = "└";
        const string cellRightBottom = "┘";
        const string cellHorizontalJointTop = "┬";
        const string cellHorizontalJointbottom = "┴";
        const string cellVerticalJointLeft = "├";
        const string cellTJoint = "┼";
        const string cellVerticalJointRight = "┤";
        const string cellHorizontalLine = "─";
        const string cellVerticalLine = "│";

        private int _Top = 0;
        private int _Bottom = 0;
        private int _Right = 0;
        private int _Left = 0;

        private List<string> _MatchList = new List<string>();
        private List<string> _CmdList = new List<string>();
        private int _ArrowPosition = 0;
        private IMDbgShell _Shell = null;

        public bool IsVisible { get; set; }
                                                           
        public enum ArrowMovement
        {
            None,
            Up,
            Down
        }

        public FileList(IMDbgShell shell)
        {
            _Shell = shell;
            LoadCommandList(shell);
        }

        public void Draw(InputBuffer buffer)
        {
            Erase();

            int height = Console.WindowHeight;
            int width = Console.WindowWidth;

            string buffString = buffer.ToString().Trim();
            if (buffString == string.Empty) return;

            string activeToken = null;
            int maxlen = 0;
            string[] tokens = buffString.Split(new char[] { ' ' });
            if (tokens.Count() > 1)
            {
                activeToken = tokens[1];

                // ignore strings that might be directory commands
                if (!activeToken.All(Char.IsLetterOrDigit)) return;

                string[] values = null;
                if (tokens[0].Equals("open"))
                {
                    values = Directory.GetFiles(Directory.GetCurrentDirectory(), activeToken + "*");
                    for (int i = 0; i < values.Count(); i++)
                    {
                        string value = values[i];
                        values[i] = "open " + Path.GetFileName(value);
                    }
                }
                else if (tokens[0].Equals("cd"))
                {
                    string curDir = Directory.GetCurrentDirectory();
                    values = Directory.GetDirectories(curDir, activeToken + "*", 
                                                    SearchOption.TopDirectoryOnly);
                    for (int i = 0; i < values.Count(); i++)
                    {
                        string value = values[i];
                        values[i] = "cd " + value;
                    }
                }

                if (values == null) return; // GetDirectories returns null on no match 
                foreach (string value in values)
                {
                    if (value.Length > maxlen)
                        maxlen = value.Length;
                    _MatchList.Add(value);
                }
            }
            else if (tokens.Count() == 1)
            {
                activeToken = tokens[0];

                foreach (string cmd in _CmdList)
                {
                    if (cmd.StartsWith(buffString))
                    {
                        if (cmd.Length > maxlen)
                            maxlen = cmd.Length;
                        _MatchList.Add(cmd);
                    }
                }
            }

            int count = _MatchList.Count();
            if (count == 0) return; // nothing matched, get out

            _Top = (height - count) / 2;
            _Bottom = (_Top + count) + 2;
            _Left = (width - maxlen) / 2;
            _Right = _Left + maxlen;

            int topPosition = _Top;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.SetCursorPosition(_Left, topPosition++);
            Console.Write(cellLeftTop + string.Empty.PadRight(maxlen, '─') + cellRightTop);
            int cntr = 0;
            foreach (string match in _MatchList)
            {
                cntr++;
                Console.SetCursorPosition(_Left, topPosition++);
                Console.BackgroundColor = ConsoleColor.DarkCyan;

                if (_ArrowPosition == cntr)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue ;
                }

                Console.Write(string.Format("{0}{1}{2}", cellVerticalLine,
                           match.PadRight(maxlen), cellVerticalLine));
            }
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.SetCursorPosition(_Left, topPosition++);
            Console.Write(cellLeftBottom + string.Empty.PadRight(maxlen, '─') + cellRightBottom);
            Console.ResetColor();

            IsVisible = true;
        }

        public string MoveSelection(ArrowMovement arrowMovement)
        {
            string selectedValue = null;

            if (_MatchList != null && _MatchList.Count > 0)
            {
                if (arrowMovement == ArrowMovement.Up)
                {
                    _ArrowPosition--;
                    if (_ArrowPosition < 1) _ArrowPosition = _MatchList.Count();
                }
                else if (arrowMovement == ArrowMovement.Down)
                {
                    _ArrowPosition++;
                    if (_ArrowPosition > _MatchList.Count()) _ArrowPosition = 1;
                }
                selectedValue = _MatchList[_ArrowPosition - 1];
            }

            return selectedValue; 
        }

        public void Close()
        {
            Erase();
            _ArrowPosition = 0;
        }

        private void Erase()
        {
            for (int i = _Top; i < _Bottom; i++)
            {
                Console.SetCursorPosition(_Left - 1, i); // move an extra space to the left to clear a sliver left over
                Console.Write(string.Empty.PadRight((_Right - _Left) + 3, ' '));
            }
            _MatchList.Clear();
            IsVisible = false;
        }

        private void LoadCommandList(IMDbgShell shell)
        {
            foreach (IMDbgCommand command in shell.Commands)
            {
                _CmdList.Add(command.CommandName);
            }
        }
    }
}
