using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Channels;
using MemoryQueueChannels;

class Program {
  static void Main(string[] args) {
    Run().Wait();

    /*
    var epoll = new Epoll();
    var mq = new MemoryQueue("/randomqueue");
    epoll.Add(mq);
    */

  }

  static async Task Run() {
    Console.WriteLine("running...");
    var pool = new MemoryPool();
    var fifo = new ReadableFifoChannel(pool);
    fifo.OpenReadFile("four");
    while(true) {
      var buffer = await fifo.ReadAsync();
      fifo.Advance(buffer.End);
      if (buffer.Length == 0) { break; }

      var array = new byte[buffer.Length];
      buffer.First.Span.TryCopyTo(array);
      Console.WriteLine(Encoding.ASCII.GetString(array));
    }
  }
}
