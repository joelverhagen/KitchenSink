using System;
using System.IO;
using System.Threading.Tasks;

namespace Knapcode.KitchenSink.Extensions
{
    public static class StreamExtensions
    {
        public static async Task FilterAsync(this Stream originalStream, int bufferSize, int blockSize, Func<MemoryStream, Task<Stream>> getFilterStream, Func<byte[], int, int, Task> handleBlock)
        {
            var blockBuffer = new byte[blockSize];
            int remainingBlock = blockSize;

            await FilterAsync(
                originalStream,
                bufferSize,
                getFilterStream,
                async (sourceBuffer, sourceStartIndex, remainingSource) =>
                {
                    do
                    {
                        int copyLength = Math.Min(remainingBlock, remainingSource);
                        if (copyLength == blockSize)
                        {
                            await handleBlock(sourceBuffer, sourceStartIndex, copyLength);
                            remainingBlock = blockSize;
                            sourceStartIndex += copyLength;
                            remainingSource -= copyLength;
                        }
                        else
                        {
                            Buffer.BlockCopy(sourceBuffer, sourceStartIndex, blockBuffer, blockSize - remainingBlock, copyLength);
                            remainingBlock -= copyLength;
                            sourceStartIndex += copyLength;
                            remainingSource -= copyLength;

                            if (remainingBlock == 0)
                            {
                                await handleBlock(blockBuffer, 0, blockSize);
                                remainingBlock = blockSize;
                            }
                        }
                    } while (remainingSource > 0);
                });

            if (remainingBlock < blockSize)
            {
                await handleBlock(blockBuffer, 0, blockSize - remainingBlock);
            }
        }

        public static async Task FilterAsync(this Stream originalStream, int bufferSize, Func<MemoryStream, Task<Stream>> getFilterStream, Func<byte[], int, int, Task> handleBlock)
        {
            var buffer = new byte[bufferSize];
            var outputStream = new MemoryStream();
            Stream filterStream = await getFilterStream(outputStream);

            int read;
            do
            {
                read = await originalStream.ReadAsync(buffer, 0, bufferSize);

                if (read > 0)
                {
                    filterStream.Write(buffer, 0, read);
                    filterStream.Flush();

                    await handleBlock(outputStream.GetBuffer(), 0, (int) outputStream.Length);
                    outputStream.SetLength(0);
                }
                else
                {
                    filterStream.Dispose();
                    await handleBlock(outputStream.GetBuffer(), 0, (int) outputStream.Length);
                }
            } while (read != 0);
        }
    }
}