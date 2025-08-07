﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace DominoWPF.Classes
{

    public class LoopStream : WaveStream
    {
        private readonly WaveStream _sourceStream;

        public LoopStream(WaveStream source)
        {
            _sourceStream = source;
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => long.MaxValue;
        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _sourceStream.Read(buffer, offset, count);
            if (read == 0)
            {
                _sourceStream.Position = 0;
                read = _sourceStream.Read(buffer, offset, count);
            }
            return read;
        }
    }

}
