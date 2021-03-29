using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Library.Methods methods = new Library.Methods();
            string path = @"C:\Users\Admin\Desktop\Code\text.txt";
            methods.Send(path);
            //Send();
        }
    }
}
