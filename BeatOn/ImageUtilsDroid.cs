using System;
using System.IO;
using System.Runtime.InteropServices;
using Android.Graphics;
using QuestomAssets;
using QuestomAssets.AssetsChanger;
using QuestomAssets.Utils;
using Android.Opengl;
namespace BeatOn
{
    public class ImageUtilsDroid : IImageUtils
    {
        public void AssignImageToTexture(byte[] imageData, Texture2DObject targetTexture, int targetWidth, int targetHeight, int targetMips = int.MaxValue, TextureConversionFormat format = TextureConversionFormat.Auto)
        {
            int actualMips;
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            byte[] textureBytes = MipmapToRGBBytes(bitmap, targetWidth, targetHeight, targetMips, out actualMips);
            targetTexture.TextureFormat = Texture2DObject.TextureFormatType.RGB24;

            targetTexture.ForcedFallbackFormat = 4;
            targetTexture.DownscaleFallback = false;
            targetTexture.Width = targetWidth;
            targetTexture.Height = targetHeight;
            targetTexture.CompleteImageSize = textureBytes.Length;
            targetTexture.MipCount = actualMips;
            targetTexture.IsReadable = false;
            targetTexture.StreamingMipmaps = false;
            targetTexture.StreamingMipmapsPriority = 0;
            targetTexture.ImageCount = 1;
            targetTexture.TextureDimension = 2;
            targetTexture.TextureSettings = new GLTextureSettings()
            {
                FilterMode = 2,
                Aniso = 1,
                MipBias = -1,
                WrapU = 1,
                WrapV = 1,
                WrapW = 0
            };
            targetTexture.LightmapFormat = 6;
            targetTexture.ColorSpace = 1;
            targetTexture.ImageData = textureBytes;
            targetTexture.StreamData = new StreamingInfo()
            {
                offset = 0,
                size = 0,
                path = ""
            };
        }

        public byte[] TextureToPngBytes(Texture2DObject texture)
        {
            if (texture.TextureFormat == Texture2DObject.TextureFormatType.RGB24)
            {
                //what's the quality setting here?
                using (MemoryStream msPng = new MemoryStream())
                {
                    RGBBytesToBitmap(texture.ImageData, texture.Width, texture.Height).Compress(Bitmap.CompressFormat.Png, 90, msPng);
                    return msPng.ToArray();
                }
            }
            else if (texture.TextureFormat == Texture2DObject.TextureFormatType.ETC_RGB4 || texture.TextureFormat == Texture2DObject.TextureFormatType.ETC2_RGB)
            {
                using (MemoryStream msPng = new MemoryStream())
                {
                    ETCBytesToBitmap(texture.ImageData, texture.Width, texture.Height).Compress(Bitmap.CompressFormat.Png, 90, msPng);
                    return msPng.ToArray();
                }
            }
            else
            {
                Log.LogErr($"{texture?.Name} is in an unsupported format for decoding: {texture?.TextureFormat}");
                return null;
            }
        }

        private static void WriteETCHeader(Java.Nio.ByteBuffer bfr, int width, int height)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                
                ms.WriteByte((byte)'P');
                ms.WriteByte((byte)'K');
                ms.WriteByte((byte)'M');
                ms.WriteByte((byte)' ');
                ms.WriteByte((byte)'1');
                ms.WriteByte((byte)'0');
                //for straight RGB, 01
                ms.WriteByte(0);
                ms.WriteByte((byte)0);
                byte[] word = BitConverter.GetBytes((ushort)width);
                ms.WriteByte(word[1]);
                ms.WriteByte(word[0]);
                word = BitConverter.GetBytes((ushort)height);
                ms.WriteByte(word[1]);
                ms.WriteByte(word[0]);
                //"active area" however you'd calculate that other than the image size?
                word = BitConverter.GetBytes((ushort)width);
                ms.WriteByte(word[1]);
                ms.WriteByte(word[0]);
                word = BitConverter.GetBytes((ushort)height);
                ms.WriteByte(word[1]);
                ms.WriteByte(word[0]);
                var bytes = ms.ToArray();
                bfr.Put(bytes);
            }
        }

        private Bitmap ETCBytesToBitmap(byte[] etc, int width, int height)
        {
            var etcsize = ETC1.GetEncodedDataSize(width, height);
            var inBfr = Java.Nio.ByteBuffer.AllocateDirect(etcsize + 16).Order(Java.Nio.ByteOrder.NativeOrder());
            var outBfr = Java.Nio.ByteBuffer.AllocateDirect(width * height * 3).Order(Java.Nio.ByteOrder.NativeOrder());
            WriteETCHeader(inBfr, width, height);
            inBfr.Put(etc, 0, etcsize);
            inBfr.Position(0);
            ETC1.DecodeImage(inBfr, outBfr, width, height, 3, width * 3);
            outBfr.Position(0);
            byte[] outBytes = new byte[width * height * 3];
            outBfr.Get(outBytes);
            return RGBBytesToBitmap(outBytes, width, height, false);            
        }

        private Bitmap RGBBytesToBitmap(byte[] rgb, int width, int height, bool includeAlpha = false)
        {
            Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            int[] rowData = new int[bitmap.Width];
            int bytePerPixel = includeAlpha ? 4 : 3;

            for (int inverseRow = height - 1; inverseRow >= 0; inverseRow--)
            {
                var row = (height - 1) - inverseRow;
                for (int col = 0; col < width; col++)
                {
                    int loc = ((width * row) + col) * bytePerPixel;
                    int val = 0;
                    if (includeAlpha)
                        val = (((int)rgb[loc]) << 24) | (((int)rgb[loc + 1]) << 16) | (((int)rgb[loc + 2]) << 8) | (int)rgb[loc + 3];
                    else
                        val = (((int)255) << 24) | (((int)rgb[loc]) << 16) | (((int)rgb[loc + 1]) << 8) | (int)rgb[loc + 2];
                    rowData[col] = val;
                }
                bitmap.SetPixels(rowData, 0, width, 0, inverseRow, width, 1);
            }
            return bitmap;
        }

        private byte[] MipmapToRGBBytes(Bitmap bitmap, int targetWidth, int targetHeight, int maxMips, out int actualMips)
        {
            //todo: eval quality, maybe set filtered to true
            bool filterResize = true;
            Bitmap curBitmap = bitmap;
            if (bitmap.Width != targetWidth || bitmap.Height != targetHeight)
            {

                curBitmap = Bitmap.CreateScaledBitmap(curBitmap, targetWidth, targetHeight, filterResize);
            }

            using (MemoryStream msMips = new MemoryStream())
            {
                int currentWidth = targetWidth;
                int currentHeight = targetHeight;
                int mipCount = 0;
                do
                {
                    byte[] mipData = CreateRGBFromBitmap(curBitmap);
                    msMips.Write(mipData, 0, mipData.Length);
                    currentWidth /= 2;
                    currentHeight /= 2;
                    mipCount++;

                    if (currentWidth < 1 || currentHeight < 1 || mipCount > maxMips)
                        break;
                    curBitmap = Bitmap.CreateScaledBitmap(curBitmap, currentWidth, currentHeight, filterResize);
                } while (true);
                actualMips = mipCount;
                return msMips.ToArray();
            }
        }

        private byte[] CreateRGBFromBitmap(Bitmap bitmap, bool includeAlpha = false)
        {
            using (MemoryStream msRGB = new MemoryStream())
            {
                int[] rowData = new int[bitmap.Width];
                int width = bitmap.Width;
                int height = bitmap.Height;
                for (int row = height - 1; row >= 0; row--)
                {
                    bitmap.GetPixels(rowData, 0, width, 0, row, width, 1);
                    for (int col = 0; col < width; col++)
                    {
                        int pixel = rowData[col];
                        if (includeAlpha)
                            msRGB.WriteByte((byte)((pixel >> 24) & 0xff)); //a

                        msRGB.WriteByte((byte)((pixel >> 16) & 0xff)); //r
                        msRGB.WriteByte((byte)((pixel >> 8) & 0xff)); //g
                        msRGB.WriteByte((byte)((pixel) & 0xff)); //b
                    }
                }
                return msRGB.ToArray();
            }
        }
    }
}