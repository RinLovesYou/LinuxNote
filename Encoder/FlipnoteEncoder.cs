using PPMLib;
using PPMLib.Extensions;
using LinuxNote;
using LinuxNote.Utilities;
using LinuxNote.Encoder;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace LinuxNote.Encoder
{
    class FlipnoteEncoder
    {
        private EncodeConfig Config { get; set; }

        public FlipnoteEncoder()
        {
            Config = Program.FlipnoteConfig;
        }

        public PPMFile Encode()
        {
            PPMFile Dummy = new PPMFile();
            PPMFile encoded = new PPMFile();
            LinuxNote.Encoder.Encoder encoder = new LinuxNote.Encoder.Encoder();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Getting Dummy...");
            try
            {
                var DummyPath = Directory.GetFiles("DummyFlipnote", "*.ppm");
                Dummy.LoadFrom(DummyPath[0]);
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Dummy got!      ");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not get Dummy!");
                return null;
            }

            try
            {
                var audio = encoder.PrepareAudio();
                encoder.PrepareFrames();
            }
            catch (Exception e)
            {

            }


            List<PPMFrame> EncodedFrames = new List<PPMFrame>();

            Directory.CreateDirectory("out");
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Dithering Frames...");
                switch (Config.ColorMode)
                {
                    

                    case 1: EncodedFrames = encoder.BlackWhiteFrames(Config.InputFolder).ToList(); break;
                    case 2: EncodedFrames = encoder.ThreeColorFrames(Config.InputFolder, PenColor.Red).ToList(); break;
                    case 3: EncodedFrames = encoder.ThreeColorFrames(Config.InputFolder, PenColor.Blue).ToList(); break;
                    case 4: EncodedFrames = encoder.FullColorFrames(Config.InputFolder).ToList(); break;
                    case 5: EncodedFrames = encoder.AutoColorFrames(Config.InputFolder).ToList(); break;
                    default: EncodedFrames = encoder.BlackWhiteFrames(Config.InputFolder).ToList(); break;
                }
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Frames Dithered!          ");
            }
            catch (Exception e)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not dither Frames!");
            }


            if (File.Exists($"{Config.InputFolder}/{Config.InputFilename}.wav"))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Preparing Audio...");
                using (WaveFileReader reader = new WaveFileReader($"{Config.InputFolder}/{Config.InputFilename}.wav"))
                {

                    byte[] buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    short[] sampleBuffer = new short[read / 2];
                    Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

                    List<byte> bgm = new List<byte>();
                    AdpcmEncoder aencoder = new AdpcmEncoder();
                    for (int i = 0; i < sampleBuffer.Length; i += 2)
                    {
                        try
                        {
                            bgm.Add((byte)(aencoder.Encode(sampleBuffer[i]) | aencoder.Encode(sampleBuffer[i + 1]) << 4));
                        }
                        catch (Exception e)
                        {

                        }

                    }

                    encoded = PPMFile.Create(Dummy.CurrentAuthor, EncodedFrames, bgm.ToArray());
                }

                
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Audio Prepared!             ");
            }
            else
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No Audio Detected         ");
                encoded = PPMFile.Create(Dummy.CurrentAuthor, EncodedFrames, new List<byte>().ToArray());
            }

            Random rnd = new Random();

            //encoded.Thumbnail.Buffer = CreateThumbnailW64(encoded.Frames[0]);

            //encoded.Save($"out/{encoded.CurrentFilename}.ppm");

            return encoded;

        }

        private void w64SetPixel(byte[] raw, int x, int y, TColor color)
        {
            var val = (int)color;
            int offset = (8 * (y / 8) + (x / 8)) * 32 + 4 * (y % 8) + (x % 8) / 2;
            int nibble = x & 1;
            raw[offset] &= (byte)~(0x0F << (4 * nibble));
            raw[offset] |= (byte)(val << (4 * nibble));
        }
    }
}
