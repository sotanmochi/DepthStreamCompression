﻿// This code is licensed under CC0.
// http://creativecommons.org/publicdomain/zero/1.0/deed.ja
// https://creativecommons.org/publicdomain/zero/1.0/deed.en

using System;
using UnityEngine;
using UnityEngine.UI;
using AzureKinect4Unity;

namespace DepthStreamCompression.Test
{
    public class RVLDepthImageCompressionTest : MonoBehaviour
    {
        [SerializeField] AzureKinectManager _AzureKinectManager;
        [SerializeField] Shader _DepthVisualizer;
        [SerializeField] Shader _DiffVisualizer;
        [SerializeField] Material _UnlitTextureMaterial;
        [SerializeField] GameObject _DepthImageObject;
        [SerializeField] GameObject _DecodedDepthImageObject;
        [SerializeField] GameObject _DiffImageObject;
        [SerializeField] GameObject _ColorImageObject;

        [SerializeField] Text _Text_DepthImageSize;
        [SerializeField] Text _CompressedDepthImageSize;
        [SerializeField] Text _ProcessingTime;

        Texture2D _DepthImageTexture;
        Texture2D _DecodedDepthImageTexture;
        Texture2D _DiffImageTexture;
        Texture2D _ColorImageTexture;

        AzureKinectSensor _KinectSensor;
        byte[] _DepthRawData;
        byte[] _EncodedDepthData;
        short[] _DecodedDepthData;
        short[] _Diff;
        int _DepthImageSize;

        System.Diagnostics.Stopwatch _Stopwatch = new System.Diagnostics.Stopwatch();

        void Start()
        {
            _KinectSensor = _AzureKinectManager.Sensor;
            if (_KinectSensor != null)
            {
                _DepthImageSize = _KinectSensor.DepthImageWidth * _KinectSensor.DepthImageHeight;
                _DepthRawData = new byte[_DepthImageSize * sizeof(short)];
                _EncodedDepthData = new byte[_DepthImageSize];
                _DecodedDepthData = new short[_DepthImageSize];
                _Diff = new short[_DepthImageSize];

                _DepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _DecodedDepthImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _DiffImageTexture = new Texture2D(_KinectSensor.DepthImageWidth, _KinectSensor.DepthImageHeight, TextureFormat.R16, false);
                _ColorImageTexture = new Texture2D(_KinectSensor.ColorImageWidth, _KinectSensor.ColorImageHeight, TextureFormat.BGRA32, false);

                MeshRenderer depthMeshRenderer = _DepthImageObject.GetComponent<MeshRenderer>();
                depthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);
                depthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DepthImageTexture);

                MeshRenderer decodedDepthMeshRenderer = _DecodedDepthImageObject.GetComponent<MeshRenderer>();
                decodedDepthMeshRenderer.sharedMaterial = new Material(_DepthVisualizer);
                decodedDepthMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DecodedDepthImageTexture);

                MeshRenderer diffMeshRenderer = _DiffImageObject.GetComponent<MeshRenderer>();
                diffMeshRenderer.sharedMaterial = new Material(_DiffVisualizer);
                diffMeshRenderer.sharedMaterial.SetTexture("_DepthTex", _DiffImageTexture);

                MeshRenderer colorMeshRenderer = _ColorImageObject.GetComponent<MeshRenderer>();
                colorMeshRenderer.sharedMaterial = new Material(_UnlitTextureMaterial);
                colorMeshRenderer.sharedMaterial.SetTexture("_MainTex", _ColorImageTexture);

                Debug.Log("ColorResolution: " + _KinectSensor.ColorImageWidth + "x" + _KinectSensor.ColorImageHeight);
                Debug.Log("DepthResolution: " + _KinectSensor.DepthImageWidth + "x" + _KinectSensor.DepthImageHeight);
            }
        }

        void Update()
        {
            if (_KinectSensor.RawDepthImage != null)
            {
                // Visualize original depth image
                short[] depthImage = _KinectSensor.RawDepthImage;
                Buffer.BlockCopy(depthImage, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DepthImageTexture.Apply();

                _Stopwatch.Reset();
                _Stopwatch.Start();

                // RVL compression
                // int encodedDataBytes = RVL.CompressRVL(depthImage, _EncodedDepthData);
                int encodedDataBytes = NativePlugin.RVL.EncodeRVL(ref depthImage, ref _EncodedDepthData, depthImage.Length);
                // Debug.Log("Encoded.Length: " + _EncodedDepthData.Length);

                _Stopwatch.Stop();
                long encodingTimeMillseconds = _Stopwatch.ElapsedMilliseconds;

                _Stopwatch.Reset();
                _Stopwatch.Start();

                // RVL decompression
                // RVL.DecompressRVL(_EncodedDepthData, _DecodedDepthData);
                NativePlugin.RVL.DecodeRVL(ref _EncodedDepthData, ref _DecodedDepthData, _DecodedDepthData.Length);

                _Stopwatch.Stop();
                long decodingTimeMillseconds = _Stopwatch.ElapsedMilliseconds;

                // Visualize decoded depth image
                Buffer.BlockCopy(_DecodedDepthData, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DecodedDepthImageTexture.LoadRawTextureData(_DepthRawData);
                _DecodedDepthImageTexture.Apply();

                // Difference of depth images
                for (int i = 0; i < depthImage.Length; i++)
                {
                    _Diff[i] = (short)Math.Abs(depthImage[i] - _DecodedDepthData[i]);
                }

                // Visualize diff image
                Buffer.BlockCopy(_Diff, 0, _DepthRawData, 0, _DepthRawData.Length * sizeof(byte));
                _DiffImageTexture.LoadRawTextureData(_DepthRawData);
                _DiffImageTexture.Apply();

                // Display info
                int originalDepthDataSize = depthImage.Length * sizeof(short);
                int compressedDepthDataSize = encodedDataBytes;
                float compressionRatio = originalDepthDataSize / compressedDepthDataSize;

                _Text_DepthImageSize.text = string.Format("Size: {2:#,0} [bytes]  Resolution: {0}x{1}",
                                                     _DepthImageTexture.width, _DepthImageTexture.height, originalDepthDataSize);
                _CompressedDepthImageSize.text = string.Format("Size: {0:#,0} [bytes]  Data compression ratio: {1:F1}",
                                                               compressedDepthDataSize, compressionRatio);
                _ProcessingTime.text = string.Format("Processing time:\n Encode: {0} [ms]\n Decode: {1} [ms]",
                                                     encodingTimeMillseconds, decodingTimeMillseconds);
            }

            if (_KinectSensor.RawColorImage != null)
            {
                _ColorImageTexture.LoadRawTextureData(_KinectSensor.RawColorImage);
                _ColorImageTexture.Apply();
            }
        }
    }
}
