using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private List<string> _FileList = new List<string>();

        private int _ArrowPosition = 0;

        public string SelectedFileName { get; set; }

        public enum ArrowMovement
        {
            None,
            Up,
            Down
        }

        public void Draw(InputBuffer buffer, ArrowMovement arrowMovement)
        {
            Erase();
            SelectedFileName = null;

            int height = Console.WindowHeight;
            int width = Console.WindowWidth;

            string buffString = buffer.ToString().Trim();
            if (buffString == string.Empty) return;
            // ignore strings that might be directory commands
            if (!buffString.All(Char.IsLetterOrDigit)) return;

            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), buffString + "*");
            int count = files.Count();

            if (count == 0)
            {
                Erase();
                return;
            }

            if (arrowMovement == ArrowMovement.Up)
            {
                _ArrowPosition--;
                if (_ArrowPosition < 1) _ArrowPosition = count;
            }
            else if (arrowMovement == ArrowMovement.Down)
            {
                _ArrowPosition++;
                if (_ArrowPosition > count) _ArrowPosition = 1;
            }

            int maxlen = 0;
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                if (filename.Length > maxlen)
                    maxlen = file.Length;
                _FileList.Add(filename);
            }

            _Top = (height - count) / 2;
            _Bottom = (_Top + count) + 2;
            _Left = (width - maxlen) / 2;
            _Right = _Left + maxlen;

            int topPosition = _Top;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.SetCursorPosition(_Left, topPosition++);
            Console.Write(cellLeftTop + string.Empty.PadRight(maxlen - 2, '─') + cellRightTop);
            int cntr = 0;
            foreach (string file in _FileList)
            {
                cntr++;
                Console.SetCursorPosition(_Left, topPosition++);

                string filedisplay = "";
                if (_ArrowPosition == cntr)
                {
                    filedisplay = "> " + file;
                    SelectedFileName = file;
                }
                else
                    filedisplay = file;

                Console.Write(string.Format("{0}{1}{2}", cellVerticalLine,
                           filedisplay.PadRight(maxlen - 2), cellVerticalLine));
            }
            Console.SetCursorPosition(_Left, topPosition++);
            Console.Write(cellLeftBottom + string.Empty.PadRight(maxlen - 2, '─') + cellRightBottom);
            Console.ResetColor();
            _FileList.Clear();
        }

        public void Erase()
        {
            for (int i = _Top; i < _Bottom; i++)
            {
                Console.SetCursorPosition(_Left - 1, i);
                Console.Write(string.Empty.PadRight(_Right + 1 - _Left, ' '));
            }
            _FileList.Clear();
        }
    }
}
