using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cmd
{
    public class InputBuffer
    {
        private static readonly int _BufSize = 100;
        private char[] _Buffer = new char[_BufSize];
        private int _BufferIdx = 0;

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
