using LinuxNote.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LinuxNote.Encoder
{
    public class Ditherer
    {
        private EncodeConfig Config { get; set; }

        public Ditherer()
        {
            Config = Program.FlipnoteConfig;
        }

        public bool FullColor()
        {
            var Folder = Config.InputFolder;
            var files = Directory.EnumerateFiles(Folder, "*.png");
            var filenames = files.ToArray();
            MathUtils.NumericalSort(filenames);

            var contrast = Config.Contrast;

            IDither DitheringType = null;
            switch (Config.DitheringMode)
            {
                case 1:
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
                case 2:
                    DitheringType = KnownDitherings.Bayer4x4;
                    break;
                case 3:
                    DitheringType = KnownDitherings.Bayer2x2;
                    break;
                case 4:
                    DitheringType = KnownDitherings.FloydSteinberg;
                    break;
                case 5:
                    DitheringType = KnownDitherings.Atkinson;
                    break;
                case 6:
                    DitheringType = KnownDitherings.Burks;
                    break;
                case 7:
                    DitheringType = KnownDitherings.JarvisJudiceNinke;
                    break;
                case 8:
                    DitheringType = KnownDitherings.StevensonArce;
                    break;
                case 9:
                    DitheringType = KnownDitherings.Sierra2;
                    break;
                case 10:
                    DitheringType = KnownDitherings.Sierra3;
                    break;
                case 11:
                    DitheringType = KnownDitherings.SierraLite;
                    break;
                case 12:
                    DitheringType = KnownDitherings.Stucki;
                    break;
                case 13:
                    DitheringType = KnownDitherings.Ordered3x3;
                    break;
                default:
                    DitheringType = null;
                    break;
            }

            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.Black);
            colors.Add(Color.White);
            colors.Add(Color.Blue);
            Directory.CreateDirectory("tmp");
            if (DitheringType != null)
            {
                for (int i = 0; i < filenames.Length; i++)
                {
                    Image<Rgba32> image;
                    try
                    {
                        image = Image.Load<Rgba32>(filenames[i + 1]);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                    
                    Image<Rgba32> bw = image.Clone();
                    bw.Mutate(x =>
                    {
                        if (contrast != 0)
                        {
                            x.Contrast(contrast);
                        }
                        x.BinaryDither(DitheringType);
                    });
                    bw.SaveAsPng($"tmp/frame_{i+1}.png");
                    bw.Dispose();

                    image.Mutate(x =>
                    {
                        if (contrast != 0)
                        {
                            x.Contrast(contrast);
                        }
                        //x.BinaryDither(DitheringType);
                        var Palette = new ReadOnlyMemory<Color>(colors.ToArray());

                        x.Dither(DitheringType, Palette);
                    });

                    image.SaveAsPng($"frames/frame_{i+1}.png");
                    image.Dispose();
                }
            }

            return true;
        }

        public void ThreeColor(int WhichColor)
        {
            var Folder = Config.InputFolder;
            var files = Directory.EnumerateFiles(Folder, "*.png");
            var filenames = files.ToArray();
            MathUtils.NumericalSort(filenames);

            var contrast = Config.Contrast;

            IDither DitheringType = null;
            switch (Config.DitheringMode)
            {
                case 1:
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
                case 2:
                    DitheringType = KnownDitherings.Bayer4x4;
                    break;
                case 3:
                    DitheringType = KnownDitherings.Bayer2x2;
                    break;
                case 4:
                    DitheringType = KnownDitherings.FloydSteinberg;
                    break;
                case 5:
                    DitheringType = KnownDitherings.Atkinson;
                    break;
                case 6:
                    DitheringType = KnownDitherings.Burks;
                    break;
                case 7:
                    DitheringType = KnownDitherings.JarvisJudiceNinke;
                    break;
                case 8:
                    DitheringType = KnownDitherings.StevensonArce;
                    break;
                case 9:
                    DitheringType = KnownDitherings.Sierra2;
                    break;
                case 10:
                    DitheringType = KnownDitherings.Sierra3;
                    break;
                case 11:
                    DitheringType = KnownDitherings.SierraLite;
                    break;
                case 12:
                    DitheringType = KnownDitherings.Stucki;
                    break;
                case 13:
                    DitheringType = KnownDitherings.Ordered3x3;
                    break;
                default:
                    //this one is my favorite :)
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
            }

            List<Color> colors = new List<Color>();
            switch (WhichColor)
            {
                case 2: colors.Add(Color.Red); break;
                case 3: colors.Add(Color.Blue); break;
                default: colors.Add(Color.Red); break;

            }
            colors.Add(Color.Black);
            colors.Add(Color.White);

            for (int i = 0; i < filenames.Length; i++)
            {
                Image<Rgba32> image;
                try
                {
                    image = Image.Load<Rgba32>(filenames[i + 1]);
                }
                catch (Exception e)
                {
                    continue;
                }
                image.Mutate(x =>
                {
                    if (contrast != 0)
                    {
                        x.Contrast(contrast);
                    }
                    //x.BinaryDither(DitheringType);
                    var Palette = new ReadOnlyMemory<Color>(colors.ToArray());


                    x.Dither(DitheringType, Palette);
                });
                image.SaveAsPng($"frames/frame_{i+1}.png");
                image.Dispose();
            }
        }

        public bool TwoColor()
        {
            var Folder = Config.InputFolder;
            var files = Directory.EnumerateFiles(Folder, "*.png");
            var filenames = files.ToArray();
            MathUtils.NumericalSort(filenames);

            var contrast = Config.Contrast;

            IDither DitheringType = null;
            switch (Config.DitheringMode)
            {
                case 0:
                    break;
                case 1:
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
                case 2:
                    DitheringType = KnownDitherings.Bayer4x4;
                    break;
                case 3:
                    DitheringType = KnownDitherings.Bayer2x2;
                    break;
                case 4:
                    DitheringType = KnownDitherings.FloydSteinberg;
                    break;
                case 5:
                    DitheringType = KnownDitherings.Atkinson;
                    break;
                case 6:
                    DitheringType = KnownDitherings.Burks;
                    break;
                case 7:
                    DitheringType = KnownDitherings.JarvisJudiceNinke;
                    break;
                case 8:
                    DitheringType = KnownDitherings.StevensonArce;
                    break;
                case 9:
                    DitheringType = KnownDitherings.Sierra2;
                    break;
                case 10:
                    DitheringType = KnownDitherings.Sierra3;
                    break;
                case 11:
                    DitheringType = KnownDitherings.SierraLite;
                    break;
                case 12:
                    DitheringType = KnownDitherings.Stucki;
                    break;
                case 13:
                    DitheringType = KnownDitherings.Ordered3x3;
                    break;
                default:
                    //this one is my favorite :)
                    DitheringType = KnownDitherings.Bayer8x8;
                    break;
            }

            List<Color> colors = new List<Color>();

            for (int i = 0; i < filenames.Length; i++)
            {
                Image<Rgba32> image;
                try
                {
                    image = Image.Load<Rgba32>(filenames[i + 1]);
                }
                catch (Exception e)
                {
                    continue;
                }
                image.Mutate(x =>
                {
                    if (contrast != 0)
                    {
                        x.Contrast(contrast);
                    }
                    if(DitheringType == null)
                    {
                        x.AdaptiveThreshold();
                    } else
                    {
                        x.BinaryDither(DitheringType);
                    }
                        
                });
                image.SaveAsPng($"{Folder}/frame_{i+1}.png");
                image.Dispose();
            }
            return true;
        }

    }
}
