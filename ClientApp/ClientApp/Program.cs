using System;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            MapReduceLibrary.Class mr = new MapReduceLibrary.Class();
            string path = @"C:\Users\Admin\source\repos\Курсова\text.txt";
            mr.Send(path, "1", "2");
        }
    }
}
