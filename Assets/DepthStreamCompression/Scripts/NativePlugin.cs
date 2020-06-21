// This code is licensed under the MIT License.
// Copyright (c) 2020 Soichiro Sugimoto

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DepthStreamCompression.NativePlugin
{
    class Plugin
    {
        [DllImport("DepthStreamCompression")]
        public static extern int EncodeRVL(short[] input, byte[] output, int numPixels);
        [DllImport("DepthStreamCompression")]
        public static extern void DecodeRVL(byte[] input, short[] output, int numPixels);

        [DllImport("DepthStreamCompression")]
        public static extern IntPtr CreateTemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold);
        [DllImport("DepthStreamCompression")]
        public static extern IntPtr CreateTemporalRVLDecoder(int frameSize);
        [DllImport("DepthStreamCompression")]
        public static extern void DeleteTemporalRVLEncoder(IntPtr encoderPtr);
        [DllImport("DepthStreamCompression")]
        public static extern void DeleteTemporalRVLDecoder(IntPtr decoderPtr);
        [DllImport("DepthStreamCompression")]
        public static extern int EncodeTemporalRVL(IntPtr encoderPtr, short[] depthBuffer, byte[] output, bool keyFrame);
        [DllImport("DepthStreamCompression")]
        public static extern void DecodeTemporalRVL(IntPtr decoderPtr, byte[] trvlFrame, short[] output, bool keyFrame);

        [DllImport("ZdepthPlugin")]
        public static extern IntPtr CreateZdepthCompressor();
        [DllImport("ZdepthPlugin")]
        public static extern void DeleteZdepthCompressor(IntPtr compressorPtr);
        [DllImport("ZdepthPlugin")]
        public static extern int CompressZdepth(IntPtr compressorPtr, int width, int height, short[] depthData, byte[] compressedData, bool keyFrame);
        [DllImport("ZdepthPlugin")]
        public static extern int DecompressZdepth(IntPtr compressorPtr, int width, int height, int compressedSize, byte[] compressedData, short[] depthData);

        [DllImport("ZdepthPlugin")]
        public static extern IntPtr CreateTemporalZdepthCompressor(int width, int height, short changeThreshold, int invalidThreshold);
        [DllImport("ZdepthPlugin")]
        public static extern IntPtr CreateTemporalZdepthDecompressor(int width, int height);
        [DllImport("ZdepthPlugin")]
        public static extern void DeleteTemporalZdepthCompressor(IntPtr compressorPtr);
        [DllImport("ZdepthPlugin")]
        public static extern void DeleteTemporalZdepthDecompressor(IntPtr decompressorPtr);
        [DllImport("ZdepthPlugin")]
        public static extern int CompressTemporalZdepth(IntPtr compressorPtr, short[] depth, byte[] compressedData, bool keyframe);
        [DllImport("ZdepthPlugin")]
        public static extern int DecompressTemporalZdepth(IntPtr decompressorPtr, int compressedSize, byte[] compressedData, short[] depth, bool keyframe);
    }

    public class RVL
    {
        public static int EncodeRVL(ref short[] input, ref byte[] output, int numPixels)
        {
            Array.Resize(ref output, numPixels);
            int numBytes = Plugin.EncodeRVL(input, output, numPixels);
            Array.Resize(ref output, numBytes);
            return numBytes;
        }

        public static void DecodeRVL(ref byte[] input, ref short[] output, int numPixels)
        {
            Plugin.DecodeRVL(input, output, numPixels);
        }
    }

    public class TemporalRVLEncoder
    {
        private IntPtr _ptr;
        private int _frameSize;

        public TemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold)
        {
            _ptr = Plugin.CreateTemporalRVLEncoder(frameSize, changeThreshold, invalidThreshold);
            _frameSize = frameSize;
        }

        ~TemporalRVLEncoder()
        {
            Plugin.DeleteTemporalRVLEncoder(_ptr);
        }

        public int Encode(ref short[] depthBuffer, ref byte[] output, bool keyFrame)
        {
            Array.Resize(ref output, _frameSize);
            int numBytes = Plugin.EncodeTemporalRVL(_ptr, depthBuffer, output, keyFrame);
            Array.Resize(ref output, numBytes);
            return numBytes;
        }
    }

    public class TemporalRVLDecoder
    {
        private IntPtr _ptr;

        public TemporalRVLDecoder(int frameSize)
        {
            _ptr = Plugin.CreateTemporalRVLDecoder(frameSize);
        }

        ~TemporalRVLDecoder()
        {
            Plugin.DeleteTemporalRVLDecoder(_ptr);
        }

        public void Decode(ref byte[] trvlFrame, ref short[] output, bool keyFrame)
        {
            Plugin.DecodeTemporalRVL(_ptr, trvlFrame, output, keyFrame);
        }
    }

    public class ZdepthCompressor
    {
        private IntPtr _ptr;

        public ZdepthCompressor()
        {
            _ptr = Plugin.CreateZdepthCompressor();
        }

        ~ZdepthCompressor()
        {
            Plugin.DeleteZdepthCompressor(_ptr);
        }

        public int CompressZdepth(int width, int height, ref short[] depthData, ref byte[] output, bool keyFrame)
        {
            Array.Resize(ref output, width*height);
            int numBytes = Plugin.CompressZdepth(_ptr, width, height, depthData, output, keyFrame);
            Array.Resize(ref output, numBytes);            
            return numBytes;
        }

        public int DecompressZdepth(int width, int height, ref byte[] compressedData, ref short[] output)
        {
            int result = Plugin.DecompressZdepth(_ptr, width, height, compressedData.Length, compressedData, output);
            if (result != 4)
            {
                Debug.LogError("Zdepth decompression failed: " + result);
            }
            return result;
        }
    }


    public class TemporalZdepthCompressor
    {
        private IntPtr _ptr;
        private int _frameSize;

        public TemporalZdepthCompressor(int width, int height, short changeThreshold, int invalidThreshold)
        {
            _ptr = Plugin.CreateTemporalZdepthCompressor(width, height, changeThreshold, invalidThreshold);
            _frameSize = width * height;
        }

        ~TemporalZdepthCompressor()
        {
            Plugin.DeleteTemporalZdepthCompressor(_ptr);
        }

        public int CompressTemporalZdepth(ref short[] depth, ref byte[] compressedData, bool keyframe)
        {
            Array.Resize(ref compressedData, _frameSize);
            int numBytes = Plugin.CompressTemporalZdepth(_ptr, depth, compressedData, keyframe);
            Array.Resize(ref compressedData, numBytes);
            return numBytes;
        }
    }

    public class TemporalZdepthDecompressor
    {
        private IntPtr _ptr;

        public TemporalZdepthDecompressor(int width, int height)
        {
            _ptr = Plugin.CreateTemporalZdepthDecompressor(width, height);
        }

        ~TemporalZdepthDecompressor()
        {
            Plugin.DeleteTemporalZdepthDecompressor(_ptr);
        }

        public int DecompressTemporalZdepth(ref byte[] compressedData, ref short[] depth, bool keyframe)
        {
            return Plugin.DecompressTemporalZdepth(_ptr, compressedData.Length, compressedData, depth, keyframe);
        }
    }
}
