using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Channels;

namespace MemoryQueueChannels {
  public class ReadableFifoChannel : ReadableChannel {
    private string path_;
    private bool opened_;
    private int fd_ = -1;

    public ReadableFifoChannel(MemoryPool pool) : base(pool) { }

    public unsafe void OpenReadFile(string path) {
      fd_ = hello_open(path, O_RDONLY);

      Action fn = null;
      fn = () => {
          var buffer = _channel.Alloc(2048);
          void* pointer;
          if (!buffer.Memory.TryGetPointer(out pointer)) {
            throw new InvalidOperationException("Memory needs to be pinned");
          }

          var data = (IntPtr)pointer;
          var count = buffer.Memory.Length;
          var r = hello_read(fd_, data, count);

          buffer.Advance(r);
          var task = buffer.FlushAsync();

          if (r == 0 || _channel.Writing.IsCompleted) {
            _channel.CompleteWriter();
          } else if (task.IsCompleted) {
            fn();
          } else {
            task.ContinueWith((t) => fn());
          }

          //_channel.CompleteWriter();

      };

      _channel.ReadingStarted.ContinueWith((t) => fn());
    }

    public void Open() {
    }

    [DllImport("libc.so.6")]
    static extern int open(string path, int flags);
    [DllImport("libc.so.6")]
    static extern int mkfifo(string path, int mode);
    [DllImport("libc.so.6")]
    static extern int read(int fd, IntPtr buf, int count);

    [DllImport("libhello.so")]
    static extern void last_error();

    [DllImport("libhello.so")]
    static extern int hello_read(int fd, IntPtr buf, int count);

    [DllImport("libhello.so")]
    static extern int hello_open(string path, int flags);

    const int O_RDONLY = 0x00;
    const int O_WRONLY = 0x01;
    const int O_RDWR = 0x02;
    const int O_CREAT = 0x40; //0100
    const int O_NONBLOCK = 0x800; //08000
  }
}
