using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace MemoryQueueChannels {

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


  public class Epoll {
    private int epfd_;
    private bool started_;
    private epoll_event ev_ = new epoll_event();
    private epoll_event[] events_ = new epoll_event[10];
    private Thread thread_ = new Thread(Run);
    private Object lock_ = new Object();
    private List<int> fds_ = new List<int>();
    private bool is_disposed_;

    public Epoll () {
      epfd_ = epoll_create1(0);
    }

    ~Epoll() {
      Dispose(false);
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
      if (!is_disposed_) {
        if (disposing) {
          //
        }

        is_disposed_ = true;
      }
    }

    private static void Run(object state) {
      ((Epoll)state).RunLoop();
    }

    public void RunLoop() {
      while(true) {
        lock(lock_){
          foreach(var fd in fds_) {
            Console.WriteLine($"adding fd {fd} to epoll");
            ev_.data.fd = fd;
            ev_.events = EPOLLIN | EPOLLET;
            epoll_ctl(epfd_, EPOLL_CTL_ADD, fd, ref ev_);
          }
          fds_.Clear();
        }

        Console.WriteLine("waiting for event");
        int n = epoll_wait(epfd_, ref events_, 10, -1);
        Console.WriteLine($"{n} edge events were triggered");
      }
    }


    public void Add(ISignalable s) {
      Add(s.GetFD());
    }

    public void Add(int fd) {
      lock(lock_){
        fds_.Add(fd);
        if (!started_) {
          started_ = true;
          thread_.Start(this);
        }
      }
    }

    const UInt32 EPOLLIN = 0x001;
    const int EPOLL_CTL_ADD = 1;
    const int EPOLL_CTL_DEL = 2;
    const int EPOLL_CTL_MOD = 3;
    const UInt32 EPOLLET = 2147483648;

    [DllImport("libc.so.6")]
    static extern int epoll_create1(int flags);

    [DllImport("libc.so.6")]
    static extern int epoll_ctl(int epfd, int op, int fd, ref epoll_event e);

    [DllImport("libc.so.6")]
    static extern int epoll_wait(int epfd, ref epoll_event[] events, int maxevents, int timeout);
  }

}
