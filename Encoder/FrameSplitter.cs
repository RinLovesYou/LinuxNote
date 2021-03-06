using FFMpegCore;
using LinuxNote.Utilities;
using LinuxNote;
using System;

namespace LinuxNote.Encoder
{
    public class FrameSplitter
    {
        private EncodeConfig Config { get; set; }

        public FrameSplitter()
        {
            Config = Program.FlipnoteConfig;
        }

        public bool SplitFrames(string Folder, string Filename)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Splitting Frames...");
                FFMpegArguments
                .FromFileInput($"{Folder}/{Filename}", true, o =>
                {

                })
                .OutputToFile($"{Folder}/frame_%d.png", true, o =>
                {
                    if (Config.Accurate)
                        o.WithFramerate(30);
                    o.Resize(new System.Drawing.Size(256, 192));


                })
                .ProcessSynchronously();

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Frames Split!          ");
                return true;
            }
            catch (Exception e)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not Split Frames!       ");
                return false;
            }


        }
    }
}
