using System;
using Cobalt.Core;

namespace Cobalt.Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Window window = new Window();

            while(window.IsOpen())
            {
                window.Refresh();
            }
        }
    }
}