using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QrClient
{
    class Program
    {
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        static QrClient qr;
        static void Main(string[] args)
        {
            qr = new QrClient();
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

            string s = "";
            while (s != "exit")
            {
                s = Console.ReadLine();
            }
        }
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    qr.Close();
                    break;
            }
            return true;
        }
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
    }
}


