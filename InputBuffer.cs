// CDBG - A console extension for the Microsoft MDBG debugger
// Copyright (c) 2013 Craig Oberg
// Licensed under the MIT License (MIT) http://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cjomd.Mdbg.Extensions.Cdbg
{
    public class InputBuffer
    {
        protected static readonly int _BufSize = 1000;
        protected char[] _Buffer = new char[_BufSize];
        protected int _BufferIdx = 0;

        public int Length
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _BufSize; i++)
                {
                    if (_Buffer[i] == '\0') break;
                    else count += 1;
                }

                return count;
            }
        }

        public void Insert(char c, int idx)
        {
            for (int i = _Buffer.Length - 2; i >= idx; i--)
            {
                _Buffer[i + 1] = _Buffer[i];
            }
            _Buffer[idx] = c;
            _BufferIdx++;
        }

        public void Remove(int idx)
        {
            for (int i = idx; i < _Buffer.Length - 1; i++)
            {
                _Buffer[i] = _Buffer[i + 1];
            }
            _BufferIdx--;
        }

        public void Clear()
        {
            for (int i = 0; i < _Buffer.Length; i++)
            {
                _Buffer[i] = '\0';
            }

            _BufferIdx = 0;
        }

        public void Load(string value)
        {
            if (value == null) return;

            for (int i = 0; i < value.Length; i++ )
            {
                _Buffer[i] = value[i];
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (char c in _Buffer)
            {
                if (c != '\0') builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
