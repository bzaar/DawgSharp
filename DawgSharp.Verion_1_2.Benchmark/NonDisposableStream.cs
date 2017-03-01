using System;
using System.IO;

namespace DawgSharp.Verion_1_2.Benchmark
{
    class NonDisposableStream : Stream
    {
        protected override void Dispose(bool disposing)
        {
        }

        private readonly Stream stream;

        public NonDisposableStream (Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override void Flush()
        {
        }

        public override long Length => stream.Length;

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}