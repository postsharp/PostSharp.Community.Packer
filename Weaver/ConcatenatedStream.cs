using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    /// <summary>
    /// Multiple streams rolled into one. Read-only. Comes from
    /// https://stackoverflow.com/a/3879231/1580088.
    /// </summary>
    public class ConcatenatedStream : Stream
    {
        private readonly Stream[] allStreams;
        private readonly Queue<Stream> streams;

        public ConcatenatedStream(Stream[] streams)
        {
            if (streams == null)
                throw new ArgumentNullException(nameof(streams));
            if (streams.Length == 0)
                throw new ArgumentException(
                    "Value cannot be an empty collection.", nameof(streams)
                );

            allStreams = streams;
            this.streams = new Queue<Stream>(streams);
        }

        public override bool CanRead
            => true;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => false;

        public override long Length
            => throw new NotImplementedException();

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush()
            => throw new NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var totalBytesRead = 0;

            if (buffer == null) return totalBytesRead;
            if (buffer.Length == 0) return totalBytesRead;
            if (offset < 0) return totalBytesRead;
            if (count <= 0) return totalBytesRead;

            try
            {
                while (count > 0 && streams.Count > 0)
                {
                    var bytesRead = streams.Peek()
                                           .Read(buffer, offset, count);
                    if (bytesRead == 0)
                    {
                        streams.Dequeue();
                        continue;
                    }

                    totalBytesRead += bytesRead;
                    offset += bytesRead;
                    count -= bytesRead;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                totalBytesRead = 0;
            }

            return totalBytesRead;
        }

        public void ResetAllToZero()
        {
            if (!allStreams.Any()) return;
            
            foreach (var stream in allStreams) stream.Position = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotImplementedException();

        public override void SetLength(long value)
            => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotImplementedException();
    }
}