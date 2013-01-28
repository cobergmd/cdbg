// CDBG - A console extension for the Microsoft MDBG debugger
// Copyright (c) 2013 Craig Oberg
// Licensed under the MIT License (MIT) http://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    class CommandPrompt 
    {
        private InputBuffer _Buffer = new InputBuffer();
        private int _CursorPosition = 0;
        private int _CursorOffset = 0;
        private int _Position = 0;

        public void Draw(int position)
        {
            _Position = position;
            int width = Console.WindowWidth;

            Console.SetCursorPosition(0, _Position);
            Console.Write(string.Empty.PadRight(Console.WindowWidth, ' '));
            Console.SetCursorPosition(0, _Position);
            Console.Write(Directory.GetCurrentDirectory() + ">");
            if (_CursorPosition == 0) _CursorPosition = Console.CursorLeft;
            if (_CursorPosition >= width) _CursorPosition = width - 1;
            _CursorOffset = Console.CursorLeft;

            Console.Write(_Buffer.ToString());
            Console.SetCursorPosition(_CursorPosition, _Position);
        }

        public void MoveToHome()
        {
            _CursorPosition = _CursorOffset;
        }

        public void MoveToEnd()
        {
            _CursorPosition = _CursorOffset + _Buffer.Length;
        }

        public void Backspace()
        {
            int bufpos = _CursorPosition - _CursorOffset;
            if (bufpos > 0)
            {
                _Buffer.Remove(bufpos - 1);
                _CursorPosition--;
            }
        }

        public void Delete()
        {
            int bufpos = _CursorPosition - _CursorOffset;
            if (bufpos >= 0)
            {
                _Buffer.Remove(bufpos);
            }
        }

        public void MoveRight()
        {
            _CursorPosition++;
        }

        public void MoveLeft()
        {
            _CursorPosition--;
        }

        public void AddCharacter(char c)
        {
            _CursorPosition++;
            int bufpos = _CursorPosition - _CursorOffset;
            _Buffer.Insert(c, bufpos - 1);
        }

        public void Load(string s)
        {
            _Buffer.Clear();
            _Buffer.Load(s);
            _CursorPosition = 0;
        }

        public void EraseToEnd()
        {
            _Buffer.Clear();
            _CursorPosition = 0;
        }

        public override string ToString()
        {
            return _Buffer.ToString();
        }
    }
}
