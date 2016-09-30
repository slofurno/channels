using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Channels;

namespace MemoryQueueChannels {

  public interface ISignalable{
    int GetFD();
    void Readable();
  }


  [StructLayout(LayoutKind.Sequential)]
  public struct padding_32 {
    public long p1, p2, p3, p4;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct mq_attr {
    public long mq_flags;
    public long mq_maxmsg;      /* Max. # of messages on queue */
    public long mq_msgsize;     /* Max. message size (bytes) */
    public long mq_curmsgs;     /* # of messages currently in queue */
    public padding_32 padding;
  }

  public class MemoryQueue : ISignalable{
    private string name_;
    private int mqfd_;
    private TaskCompletionSource<string> tcs_;
    private TaskCompletionSource<bool> readable_;

    public MemoryQueue(string name) {
      name_ = name;
      mqfd_ = mq_open(name, O_RDWR|O_NONBLOCK|O_CREAT, S_IRUSR|S_IWUSR, IntPtr.Zero);
    }

    public void Readable() {
      readable_.SetResult(true);
    }

    public int GetFD() {
      return mqfd_;
    }

    public Task<string> ReceiveAsync() {
      tcs_ = new TaskCompletionSource<string>();
      return tcs_.Task;
    }

    [DllImport("librt.so")]
    static extern int mq_open(string name, int oflag);

    [DllImport("librt.so")]
    static extern int mq_open(string name, int oflag, int mode, ref mq_attr attr);

    [DllImport("librt.so")]
    static extern int mq_open(string name, int oflag, int mode, IntPtr a);

    [DllImport("librt.so")]
    static extern int mq_getattr(int fd, ref mq_attr attr);


    const int O_RDONLY = 0x00;
    const int O_WRONLY = 0x01;
    const int O_RDWR = 0x02;
    const int O_CREAT = 0x40; //0100
    const int O_NONBLOCK = 0x800; //08000

    const int S_IRUSR = 0x100; //0400
    const int S_IWUSR = 0x080; //0200
    const int QUEUE_MODE = 0x1A4; //0644

  }
}
