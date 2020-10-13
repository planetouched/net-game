using System;
using System.Collections.Generic;
using Shared.Utils;

namespace Shared.Decoders
{
    public class ByteToMessageDecoder
    {
        private readonly byte[] _storedBytes;
        private int _bytesOffset;

        public ByteToMessageDecoder(int bufferSize)
        {
            _storedBytes = new byte[bufferSize];
        }

        public List<byte[]> Decode(byte[] bytes)
        {
            List<byte[]> fullMessages = null;
            
            if (_bytesOffset + bytes.Length >= _storedBytes.Length)
            {
                throw new IndexOutOfRangeException("critical error on decoding");
            }

            Buffer.BlockCopy(bytes, 0, _storedBytes, _bytesOffset, bytes.Length);
            
            _bytesOffset += bytes.Length;

            bool tryGetMessage = true;
            
            while (_bytesOffset >= 4 && tryGetMessage)
            {
                tryGetMessage = false;
                
                int messageSize = BitConverter.ToInt32(_storedBytes, 0);

                if (_bytesOffset >= messageSize)
                {
                    if (fullMessages == null)
                        fullMessages = new List<byte[]>();
                    
                    var fullMessage = new byte[messageSize];
                    Buffer.BlockCopy(_storedBytes, 0, fullMessage, 0, messageSize);
                    var tail = _bytesOffset - messageSize;
                    Buffer.BlockCopy(_storedBytes, messageSize, _storedBytes, 0, tail);
                    _bytesOffset = tail;
                    fullMessages.Add(fullMessage);
                    tryGetMessage = true;
                }
            }

            return fullMessages;
        }
    }
}
