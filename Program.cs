
using FFMpegCore;
using Newtonsoft.Json;
using Octokit;
using PPMLib;
using LinuxNote.Encoders;
using LinuxNote.Encoder;
using LinuxNote.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NDesk.Options;
using System.Reflection;
using NAudio.SoundFont;

namespace LinuxNote
{
    internal class Program
    {
        private static bool Running { get; set; }
        public static EncodeConfig FlipnoteConfig { get; set; }

        public static string exedir = Process.GetCurrentProcess().MainModule.FileName.Replace("FlipnoteEncoder", "");

        private static void Main(string[] args)
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = $"{exedir}ffmpeg/bin" });
            var show_help = false;
            int colormode = 1;
            int dithermode = 1;
            string inputfile = string.Empty;


            var p = new OptionSet
            {
                { "i=|input=", "Specify the input mp4 file", v => inputfile = v },
                { "c=|color=", "Specify the Color Mode (1-5)", (int v) => colormode = v },
                { "d=|dither=", "Specify the Dithering Mode (0-13)", (int v) => dithermode = v },
                { "help",  "show this message and exit", v => show_help = v != null }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("bundling: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `FlipnoteEncoder --help' for more information.");
                return;
            }

            if (show_help)
            {
                Show_help(p);
                return;
            }


            FileInfo info = new FileInfo(inputfile);

            var newEncodeConfig = new EncodeConfig();
                newEncodeConfig.Accurate = true;
                newEncodeConfig.DitheringMode = dithermode;
                newEncodeConfig.ColorMode = colormode;
                newEncodeConfig.Contrast = 0;
                newEncodeConfig.InputFilename = $"input.mp4";
                newEncodeConfig.InputFolder = $"{exedir}/frames";
                newEncodeConfig.Split = false;
                newEncodeConfig.DeleteOnFinish = true;
                newEncodeConfig.SplitAmount = 2;
                FlipnoteConfig = newEncodeConfig;

            try 
            {
                info.CopyTo($"{exedir}/frames/input.mp4", true);
                var encoder = new FlipnoteEncoder();

                var encoded = encoder.Encode();
                encoded.Save(info.DirectoryName+$"/{encoded.CurrentFilename}.ppm");
            }
            catch(Exception e) 
            {
                Console.WriteLine($"{e.Message}\n{e.StackTrace}");
            }


            Cleanup();
            info.Delete();


        }
        static void Show_help(OptionSet p)
        {
            Console.WriteLine("Parameterlist to use:");
            p.WriteOptionDescriptions(Console.Out);
        }

        public static void EncodeFlipnote()
        {
            CreateEncodeConfig();

            var encoder = new FlipnoteEncoder();

            var encoded = encoder.Encode();
            if (encoded != null)
            {
                Directory.CreateDirectory("tmp");
                encoded.Save($"tmp/{encoded.CurrentFilename}.ppm");

                if (FlipnoteConfig.Split)
                {
                    double bytelength = new FileInfo($"tmp/{encoded.CurrentFilename}.ppm").Length;
                    bytelength = bytelength / 1024;
                    Console.WriteLine(bytelength);
                    var MB = bytelength / 1024;
                    if (MB >= 1)
                    {
                        List<PPMFile> files = new List<PPMFile>();
                        Console.WriteLine(MB);
                        var framesframes = encoded.Frames.ToArray().Split((int)(encoded.Frames.Length / MB + 1));
                        var audioaudio = encoded.Audio.SoundData.RawBGM.ToArray().Split((int)(encoded.Audio.SoundData.RawBGM.Length / MB + 1));
                        if (MB > 1.3)
                        {
                            framesframes = encoded.Frames.ToArray().Split((int)(encoded.Frames.Length / MB + 2));
                            audioaudio = encoded.Audio.SoundData.RawBGM.ToArray().Split((int)(encoded.Audio.SoundData.RawBGM.Length / MB + 2));
                        }

                        for (int i = 0; i < framesframes.Count(); i++)
                        {
                            var aaa = PPMFile.Create(encoded.CurrentAuthor, framesframes.ToList()[i].ToList(), audioaudio.ToList()[i].ToArray());
                            aaa.Save($"out/{aaa.CurrentFilename}_{i}.ppm");
                        }
                    }
                    else
                    {
                        encoded.Save($"out/{encoded.CurrentFilename}.ppm");
                    }
                }
                else
                {
                    encoded.Save($"out/{encoded.CurrentFilename}.ppm");
                }
                var mp4try = new PPMFile();
                mp4try.LoadFrom($"tmp/{encoded.CurrentFilename}.ppm");
                Mp4Encoder mp4 = new Mp4Encoder(mp4try);
                var a = mp4.EncodeMp4("out", 2);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem creating the flipnote.");
                Console.WriteLine("Please join the support server for further assistance with this issue");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any Key to continue...");
                Console.ReadKey();
            }
            Cleanup();
            
        }

        private static void Cleanup()
        {
            if (Directory.Exists($"{exedir}/tmp"))
            {
                try
                {
                    string[] files = Directory.EnumerateFiles($"{exedir}/tmp").ToArray();
                    files.ToList().ForEach(f =>
                    {
                        File.Delete(f);
                    });
                    Directory.Delete($"{exedir}/tmp");
                }
                catch (Exception e)
                {

                }
            }
            if (Directory.Exists($"{exedir}/out/temp"))
            {
                try
                {
                    string[] files = Directory.EnumerateFiles($"{exedir}/out/temp").ToArray();
                    files.ToList().ForEach(f =>
                    {
                        File.Delete(f);
                    });
                    Directory.Delete($"{exedir}/out/temp");
                }
                catch (Exception e)
                {

                }
            }

            try
            {
                string[] files = Directory.EnumerateFiles(FlipnoteConfig.InputFolder, "*.png").ToArray();
                files.ToList().ForEach(f =>
                {
                    File.Delete(f);
                    if (File.Exists($"{FlipnoteConfig.InputFolder}/{FlipnoteConfig.InputFilename}.wav"))
                    {
                        File.Delete($"{FlipnoteConfig.InputFolder}/{FlipnoteConfig.InputFilename}.wav");
                    }
                });
            }
            catch (Exception e)
            {

            }
        }

        private static void DecodeFlipnote()
        {
        }

        private static void CreateEncodeConfig()
        {
            if (!File.Exists("config.json"))
            {
                var newEncodeConfig = new EncodeConfig();
                newEncodeConfig.Accurate = true;
                newEncodeConfig.DitheringMode = 1;
                newEncodeConfig.ColorMode = 1;
                newEncodeConfig.Contrast = 0;
                newEncodeConfig.InputFilename = "input.mp4";
                newEncodeConfig.InputFolder = "frames";
                newEncodeConfig.Split = false;
                newEncodeConfig.DeleteOnFinish = true;
                newEncodeConfig.SplitAmount = 2;
                FlipnoteConfig = newEncodeConfig;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamWriter sw = new StreamWriter("config.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, newEncodeConfig);
                }
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sw = new StreamReader("config.json"))
                using (JsonReader writer = new JsonTextReader(sw))
                {
                    var read = serializer.Deserialize<EncodeConfig>(writer);
                    FlipnoteConfig = read;
                }
            }
        }
    }
}
