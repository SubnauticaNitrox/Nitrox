using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Serialization.Resources.Datastructures;

namespace NitroxServer_Subnautica.Serialization.Resources.Parsers.Images
{
    public class Texture2DAssetParser : AssetParser
    {
        public override void Parse(AssetIdentifier identifier, AssetsFileReader reader, ResourceAssets resourceAssets, Dictionary<int, string> relativeFileIdToPath)
        {
            string assetName = reader.ReadCountStringInt32();

            if (assetName == "RandomStart")
            {
                reader.Position += 9;
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                int imageSize = reader.ReadInt32();
                reader.Position += 52;

                byte[] data = reader.ReadBytes(imageSize);
                byte[] decodedData = AssetsTools.NET.Extra.DXTDecoders.ReadDXT1(data, width, height);
                using (MemoryStream stream = new MemoryStream(decodedData))
                {
                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                    Rectangle dimension = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    BitmapData picData = bmp.LockBits(dimension, ImageLockMode.ReadWrite, bmp.PixelFormat);
                    picData.Stride = width * 4;
                    IntPtr pixelStartAddress = picData.Scan0;

                    Marshal.Copy(decodedData, 0, pixelStartAddress, decodedData.Length);

                    bmp.UnlockBits(picData);

                    resourceAssets.NitroxRandom = new RandomStartGenerator(bmp);
                }
            }
        }
    }
}
