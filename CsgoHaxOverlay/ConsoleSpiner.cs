using System;

namespace CsgoHaxOverlay
{
    partial class ConsoleSpiner
    {
        private int _counter;
        public ConsoleSpiner()
        {
            _counter = 0;
        }
        public void Turn()
        {
            _counter++;
            switch (_counter % 4)
            {
                case 0: Console.Write("/"); break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
            }
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            Console.CursorVisible = false;
        }
    }
}