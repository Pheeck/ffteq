using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ffteq
{
    class Wav
    {
        private const int HEADER_LEN = 44;
        private const int FMT_CHUNK_LEN = 16;
        private const int PCM_TYPE_NUM = 1;

        public Signal WavSignal;
        public int BitsPerSample;

        /// <summary>
        /// Consume everything from "RIFF" to size of data chunk (including),
        /// set this Wav object as 8bit or 16bit and return sample rate.
        ///
        /// Any optional RIFF chunks are skipped.
        /// If this isn't a mono PCM WAVE file, throw exception.
        /// </summary>
        /// <returns> Sample rate in Hz </returns>
        private int ReadMetadata(Stream stream)
        {
            byte[] buff = new byte[4];

            // "RIFF"
            stream.Read(buff, 0, 4);
            if (Encoding.Default.GetString(buff) != "RIFF")
            {
                throw new ApplicationException("Isn't a RIFF file");
            }

            // File size
            stream.Read(buff, 0, 4);

            // "WAVE"
            stream.Read(buff, 0, 4);
            if (Encoding.Default.GetString(buff) != "WAVE")
            {
                throw new ApplicationException("Isn't a WAVE file");
            }

            // "fmt "
            stream.Read(buff, 0, 4);
            if (Encoding.Default.GetString(buff) != "fmt ")
            {
                throw new ApplicationException(
                    "File doesn't start with format chunk"
                );
            }

            // Format chunk lenght
            stream.Read(buff, 0, 4);

            // Type of format and number of channels
            stream.Read(buff, 0, 2);
            if (buff[0] != 1 || buff[1] != 0)
            {
                throw new ApplicationException(
                    "Isn't a PCM file"
                );
            }
            stream.Read(buff, 0, 2);
            if (buff[0] != 1 || buff[1] != 0)
            {
                throw new ApplicationException(
                    "Isn't a mono file"
                );
            }

            // Sample rate
            stream.Read(buff, 0, 4);
            int sampleRate = BitConverter.ToInt32(buff, 0);
            if (sampleRate < 1)
            {
                throw new ApplicationException(
                    "Invalid sample rate: " + sampleRate
                );
            }

            // Average bytes per second
            stream.Read(buff, 0, 4);

            // Data block size and bits per sample
            stream.Read(buff, 0, 2);
            stream.Read(buff, 0, 2);
            int bitsPerSample = buff[0];
            if (bitsPerSample != 8)
            {
                throw new ApplicationException(
                    "Ins't an 8bit file"
                );
            }
            BitsPerSample = 8;

            // Skip optional chunks, "data" and data chunk length
            string chunkName;
            int chunkLen;
            do
            {
                stream.Read(buff, 0, 4);
                chunkName = Encoding.Default.GetString(buff);
                stream.Read(buff, 0, 4);
                chunkLen = BitConverter.ToInt32(buff, 0);
            }
            while (chunkName != "data");

            return sampleRate;
        }

        /// <summary>
        /// Consume rest of the stream.
        /// To be used after ReadMetadata() has consumed all metadata.
        /// </summary>
        /// <returns> Array of 8bit samples </returns>
        private int[] ReadData8Bit(Stream stream)
        {
            List<int> result = new List<int>();
            int sample;
            while ((sample = stream.ReadByte()) != -1)
            {
                result.Add(sample);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Convert array of 8bit unsigned samples to array of double floating
        /// point samples with -1.0 as min and 1.0 as max.
        /// </summary>
        private static double[] Conv8ToDouble(int[] data8Bit)
        {
            double[] result = new double[data8Bit.Length];
            for (int i = 0; i < data8Bit.Length; i++)
            {
                int inSample = data8Bit[i];
                double outSample;
                inSample -= 128;
                outSample = inSample / 128.0;
                result[i] = outSample;
            }
            return result;
        }

        /// <summary>
        /// Convert array of double floating point samples with -1.0 as min and
        /// 1.0 as max to array of 8bit unsigned samples.
        /// samples.
        /// </summary>
        private static int[] ConvDoubleTo8(double[] dataDouble)
        {
            int[] result = new int[dataDouble.Length];
            for (int i = 0; i < dataDouble.Length; i++)
            {
                double inSample = dataDouble[i];
                int outSample;
                inSample *= 128;
                inSample += 128;

                // Handle case where inSample = 1.0
                if (inSample == 256)
                {
                    inSample = 255;
                }

                outSample = (int) inSample;
                result[i] = outSample;
            }
            return result;
        }

        public Wav(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Can't read stream");
            }

            int sampleRate = ReadMetadata(stream);
            int[] data8Bit = ReadData8Bit(stream);
            double[] dataDouble = Conv8ToDouble(data8Bit);
            WavSignal = new Signal(dataDouble, sampleRate);
        }

        public void WriteFile(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Can't write to stream");
            }

            int sampleRate = WavSignal.SampleRate;
            double[] dataDouble = WavSignal.Data;

            byte[] buff = new byte[4];
            
            if (BitsPerSample == 8)
            {
                int[] data8Bit = ConvDoubleTo8(dataDouble);
                int totalFileSize = HEADER_LEN + data8Bit.Length;

                // "RIFF"
                buff = Encoding.Default.GetBytes("RIFF");
                stream.Write(buff, 0, 4);

                // RIFF chunk size
                buff = BitConverter.GetBytes(totalFileSize - 8);
                stream.Write(buff, 0, 4);

                // "WAVE"
                buff = Encoding.Default.GetBytes("WAVE");
                stream.Write(buff, 0, 4);

                // "fmt "
                buff = Encoding.Default.GetBytes("fmt ");
                stream.Write(buff, 0, 4);

                // fmt chunk length
                buff = BitConverter.GetBytes(FMT_CHUNK_LEN);
                stream.Write(buff, 0, 4);

                // Type of format (PCM) and number of channels (1)
                buff = BitConverter.GetBytes(PCM_TYPE_NUM);
                stream.Write(buff, 0, 2);
                buff = BitConverter.GetBytes(1);
                stream.Write(buff, 0, 2);

                // Sample rate
                buff = BitConverter.GetBytes(sampleRate);
                stream.Write(buff, 0, 4);

                // Average bytes per second
                buff = BitConverter.GetBytes(sampleRate * BitsPerSample / 8);
                stream.Write(buff, 0, 4);

                // Data block size and bits per sample
                buff = BitConverter.GetBytes(BitsPerSample / 8);
                stream.Write(buff, 0, 2);
                buff = BitConverter.GetBytes(BitsPerSample);
                stream.Write(buff, 0, 2);

                // "data"
                buff = Encoding.Default.GetBytes("data");
                stream.Write(buff, 0, 4);

                // Data chunk size
                buff = BitConverter.GetBytes(data8Bit.Length - 1);
                stream.Write(buff, 0, 4);

                // Write samples
                foreach (int sample in data8Bit)
                {
                    buff = BitConverter.GetBytes(sample);
                    stream.Write(buff, 0, 1);
                }
            }
            else
            {
                throw new ApplicationException("Internal Error: Unreachable");
            }
        }
    }
}
