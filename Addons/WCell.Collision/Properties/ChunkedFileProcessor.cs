using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WCell.Collision
{
    public abstract class ChunkedFileProcessor
    {
        protected BinaryReader m_binReader;

        protected ChunkedFileProcessor(string fileName)
        {
            FileStream fs = File.Open(fileName, FileMode.Open);
            m_binReader = new BinaryReader(fs);
        }

        public void Process()
        {
            while (m_binReader.BaseStream.Position < m_binReader.BaseStream.Length)
            {
                string chunk = m_binReader.ReadFixedReversedAsciiString(4);

                // sometimes this is wrong :/ so we pass it to the readers so they can fix it
                int size = m_binReader.ReadInt32();
                long start = m_binReader.BaseStream.Position;

                ProcessChunk(chunk, ref size);

                m_binReader.BaseStream.Seek(start + size, SeekOrigin.Begin);
            }

            PostProcess();
        }


        protected virtual void PostProcess()
        {
            m_binReader.Close();
        }
        protected abstract void ProcessChunk(string chunkSignature, ref int chunkSize);
    }
}
