using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework
{
    static class Libc
    {
        [DllImport("libc")]
        public static extern int open(string pathname, int flags);

        [DllImport("libc")]
        public static extern int close(int fd);

        [DllImport("libc")]
        public static unsafe extern int read(int fd, void* buf, int count);
    }
}
