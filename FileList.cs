// CDBG - A console extension for the Microsoft MDBG debugger
// Copyright (c) 2013 Craig Oberg
// Licensed under the MIT License (MIT) http://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Samples.Tools.Mdbg;

namespace cjomd.Mdbg.Extensions.Cdbg
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
        const string upArrow = "↑";
        const string downArrow = "↓";
        const string rightArrow = "→";
        const string leftArrow = "←";

        private int _Top = 0;
        private int _Bottom = 0;
        private int _Right = 0;
        private int _Left = 0;
        private int _MaxHeight = 0;

        private List<string> _AutoCompleteList = new List<string>();
        private List<string> _CmdList = new List<string>();
        private int _ArrowPosition = -1;
        private IMDbgShell _Shell = null;

        public bool IsVisible { get; set; }
                                                           
        public enum ArrowMovement
        {
            None,
            Up,
            Down
        }

        public FileList(IMDbgShell shell, int maxHeight)
        {
            _Shell = shell;
            _MaxHeight = maxHeight;
            LoadCommandList(shell);
        }

        public void Draw(string pattern)
        {
            Erase();

            string matchString = pattern.Trim();
            if (matchString == string.Empty) return;

            int maxlen = CreateAutoCompleteList(matchString);

            int displaycount = _AutoCompleteList.Count();
            if (displaycount == 0) return; // nothing matched, get out
            if (displaycount > _MaxHeight) displaycount = _MaxHeight;

            _Top = (Console.WindowHeight - displaycount) / 2;
            _Bottom = (_Top + displaycount) + 2;
            _Left = (Console.WindowWidth - maxlen) / 2;
            _Right = _Left + maxlen;

            int topPosition = _Top;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.SetCursorPosition(_Left, topPosition++);
            Console.Write(cellLeftTop + string.Empty.PadRight(maxlen, '─') + cellRightTop);

            int listOffset = 0;
            if (_ArrowPosition >= displaycount)
                listOffset = _ArrowPosition - (displaycount - 1);
                                                               
            for (int cntr = listOffset; cntr < (displaycount + listOffset); cntr++)
            {
                string match = _AutoCompleteList[cntr];
                
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

        private int CreateAutoCompleteList(string buffString)
        {
            string activeToken = null;
            int maxlen = 0;
            string[] tokens = buffString.Split(new char[] { ' ' });
            if (tokens.Count() > 1)
            {
                activeToken = tokens[1];

                // ignore strings that might be directory commands
                if (!activeToken.All(Char.IsLetterOrDigit)) return 0;

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

                if (values != null)
                {
                    foreach (string value in values)
                    {
                        if (value.Length > maxlen)
                            maxlen = value.Length;
                        _AutoCompleteList.Add(value);
                    }
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
                        _AutoCompleteList.Add(cmd);
                    }
                }
            }
            return maxlen;
        }

        public string MoveSelection(ArrowMovement arrowMovement)
        {
            string selectedValue = null;

            if (_AutoCompleteList != null && _AutoCompleteList.Count > 0)
            {
                if (arrowMovement == ArrowMovement.Up)
                {
                    _ArrowPosition--;
                    if (_ArrowPosition < 0) _ArrowPosition = _AutoCompleteList.Count() - 1;
                }
                else if (arrowMovement == ArrowMovement.Down)
                {
                    _ArrowPosition++;
                    if (_ArrowPosition > _AutoCompleteList.Count() - 1) _ArrowPosition = 0;
                }
                selectedValue = _AutoCompleteList[_ArrowPosition];
            }

            return selectedValue; 
        }

        public void Close()
        {
            Erase();
            _ArrowPosition = -1;
        }

        private void Erase()
        {
            for (int i = _Top; i < _Bottom; i++)
            {
                Console.SetCursorPosition(_Left - 1, i); // move an extra space to the left to clear a sliver left over
                Console.Write(string.Empty.PadRight((_Right - _Left) + 3, ' '));
            }
            _AutoCompleteList.Clear();
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
