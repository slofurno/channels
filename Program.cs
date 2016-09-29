using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Channels;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct mq_attr {
  public long mq_flags;
  public long mq_maxmsg;      /* Max. # of messages on queue */
  public long mq_msgsize;     /* Max. message size (bytes) */
  public long mq_curmsgs;     /* # of messages currently in queue */
}


[StructLayout(LayoutKind.Sequential)]
public struct epoll_event {
  public UInt32 events;
  public epoll_data data;
}

[StructLayout(LayoutKind.Explicit)]
public struct epoll_data {
  [FieldOffset(0)]
  public IntPtr ptr;
  [FieldOffset(0)]
  public int fd;
}

class Program
{
    [DllImport("libhello.so")]
    static extern int addInts(int a, int b);

    [DllImport("libmq.so")]
    static extern string mqueue_read(string name);

    [DllImport("libmq.so")]
    static extern int open_file(string name);

    [DllImport("librt.so")]
    static extern int mq_open(string name, int oflag);

    [DllImport("librt.so")]
    static extern int mq_open(string name, int oflag, int mode, ref mq_attr attr);

    [DllImport("librt.so")]
    static extern int mq_open(string name, int oflag, int mode, IntPtr a);

    [DllImport("librt.so")]
    static extern int mq_getattr(int fd, ref mq_attr attr);

    [DllImport("libc.so.6")]
    static extern int epoll_create1(int flags);

    [DllImport("libc.so.6")]
    static extern int epoll_ctl(int epfd, int op, int fd, ref epoll_event e);

    [DllImport("libc.so.6")]
    static extern int epoll_wait(int epfd, ref epoll_event[] events, int maxevents, int timeout);

    [DllImport("libc.so.6")]
    static extern int open(string path, int flags);

    [DllImport("libc.so.6")]
    static extern int mkfifo(string path, int mode);

    const int O_RDONLY = 0x00;
    const int O_WRONLY = 0x01;
    const int O_RDWR = 0x02;
    const int O_CREAT = 0x40; //0100
    const int O_NONBLOCK = 0x800; //08000

    const int S_IRUSR = 0x100; //0400
    const int S_IWUSR = 0x080; //0200
    const int QUEUE_MODE = 0x1A4; //0644

    const UInt32 EPOLLIN = 0x001;
    const int EPOLL_CTL_ADD = 1;
    const int EPOLL_CTL_DEL = 2;
    const int EPOLL_CTL_MOD = 3;
    const UInt32 EPOLLET = 2147483648;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
        Console.WriteLine("{0} + {1} = {2}", 4, 5, addInts(4,5));

        mq_attr attr = new mq_attr();
        attr.mq_flags = O_NONBLOCK;

        int mqfd = mq_open("/asdf", O_RDWR|O_NONBLOCK);
        //int mqfd = mq_open("/asdf", O_RDWR|O_NONBLOCK, S_IRUSR|S_IWUSR, ref attr);
        mq_getattr(mqfd, ref attr);
        Console.WriteLine("maxmsg: {0}\nmsgsize: {1}\nflags: {2}", attr.mq_maxmsg, attr.mq_msgsize, attr.mq_flags);

        int epfd = epoll_create1(0);
        var ev = new epoll_event();
        ev.events = EPOLLIN | EPOLLET;
        ev.data.fd = mqfd;

        if (epoll_ctl(epfd, EPOLL_CTL_ADD, mqfd, ref ev) < 0) {
          Console.WriteLine("epoll_ctl error");
        }

        //int fifo = mkfifo("four", 0666);
        //Console.WriteLine($"try to make fifo fd: {fifo}");
        //int f = open_file("four");
        int f = open("four", O_RDONLY|O_NONBLOCK);
        ev.data.fd = f;
        ev.events = EPOLLIN | EPOLLET;
        Console.WriteLine($"opened fifo fd: {f}");

        if (epoll_ctl(epfd, EPOLL_CTL_ADD, f, ref ev) < 0) {
          Console.WriteLine("epoll_ctl error");
        }

        var events = new epoll_event[10];
        Console.WriteLine($"waiting on fd {mqfd} or {f}");
        int n = epoll_wait(epfd, ref events, 10, -1);

        Console.WriteLine($"got {n} events");
        /*
        run().Wait();
        string read = mqueue_read("/asdf");
        Console.WriteLine($"{read}");
        */
    }

    static async Task read(IChannel<string> messages) {
      for(;;) {
        //await Task.Delay(2000);
        //await messages.WriteAsync("DFGDFGDF");
        string read = mqueue_read("/asdf");
        await messages.WriteAsync(read).ConfigureAwait(false);
        //await Task.Yield();
      }
    }

    static async Task run() {
      var messages = Channel.Create<string>();
      Console.WriteLine("111");
      Task.Run(() => read(messages));

      for(;;){
        Console.WriteLine("lets get a msg,,");
        var msg = await messages.ReadAsync().ConfigureAwait(false);
        Console.WriteLine($"{msg}");
      }

    }
}

