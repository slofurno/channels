using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("libhello.so")]
    static extern int addInts(int a, int b);

    [DllImport("libmq.so")]
    static extern string mqueue_read(string name);

    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        Console.WriteLine("{0} + {1} = {2}", 4, 5, addInts(4,5));

        wait().Wait();
        string read = mqueue_read("/asdf");
        Console.WriteLine($"{read}");
    }

    static async Task wait() {
      await Task.Delay(2000);
    }
}

