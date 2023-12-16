
using FFMpegCore;
using FFMpegCore.Enums;
using PPMLib.Extensions;
using PPMLib;
using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;

namespace LinuxNote.Encoders
{
    public class Mp4Encoder
    {
        private PPMFile Flipnote { get; set; }

        /// <summary>
        /// Simple Mp4 Encoder. Requires FFMpeg to be installed in path.
        /// </summary>
        /// <param name="flipnote"></param>
        public Mp4Encoder(PPMFile flipnote)
        {
            this.Flipnote = flipnote;
        }

        /// <summary>
        /// Encode the Mp4.
        /// </summary>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4()
        {
            return Encode();
        }

        /// <summary>
        /// Encode the Mp4 and save it to the specified path
        /// </summary>
        /// <param name="path">Creates path if it doesn't exist. Doesn't save if path is "temp"</param>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4(string path)
        {
            return Encode(path);
        }

        /// <summary>
        /// Encode the Mp4 with the specified scale multiplier
        /// </summary>
        /// <param name="scale">Scale Multiplier</param>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4(int scale)
        {
            return Encode("out", scale);
        }

        /// <summary>
        /// Encode the Mp4 with the specified scale and save it to the given path.
        /// </summary>
        /// <param name="path">Creates path if it doesn't exist. Doesn't save if path is "temp"</param>
        /// <param name="scale">Scale Multiplier</param>
        /// <returns>Mp4 byte array</returns>
        public byte[] EncodeMp4(string path, int scale)
        {
            return Encode(path, scale);
        }

        private byte[] Encode(string path = "temp", int scale = 1)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                if (!Directory.Exists($"{path}/temp"))
                {
                    Directory.CreateDirectory($"{path}/temp");
                }
                else
                {
                    Cleanup(path);
                }


                for (int i = 0; i < Flipnote.FrameCount; i++)
                {
                    try
                    {
                        PPMRenderer.GetFrameBitmap(Flipnote.Frames[i]).SaveAsPng($"{path}/temp/frame_{i}.png");
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                }

                var frames = Directory.EnumerateFiles($"{path}/temp").ToArray();
                Utils.NumericalSort(frames);
                
                File.WriteAllBytes($"{path}/temp/audio.wav", Flipnote.Audio.GetWavBGM(Flipnote));

                var a = FFMpegArguments
                        .FromDemuxConcatInput(frames, options => options
                        .WithFramerate(Flipnote.Framerate))
                        .AddFileInput($"{path}/temp/audio.wav", false)
                        .OutputToFile($"{path}/{Flipnote.CurrentFilename}.mp4", true, o =>
                        {
                            o.Resize(256 * scale, 192 * scale)
                            .WithVideoCodec(VideoCodec.LibX264)
                            .ForcePixelFormat("yuv420p")
                            .ForceFormat("mp4");
                        });

                a.ProcessSynchronously();

                var mp4 = File.ReadAllBytes($"{path}/{Flipnote.CurrentFilename}.mp4");

                Cleanup(path);

                return mp4;


            }
            catch (Exception e)
            {
                Cleanup(path);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }


        private void Cleanup(string path)
        {
            if (!Directory.Exists($"{path}/temp"))
            {
                return;
            }
            var files = Directory.EnumerateFiles($"{path}/temp");
            
            files.ToList().ForEach(file =>
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    // idk yet
                }
            
            });
            Directory.Delete($"{path}/temp");
        }
    }
}