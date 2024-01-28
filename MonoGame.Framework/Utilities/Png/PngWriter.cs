﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace MonoGame.Utilities.Png
{
    public class PngWriter
    {
        private const int _bitsPerSample = 8;
        private ColorType _colorType;
        private Color[] _colorData;
        private int _width;
        private int _height;

        public PngWriter()
        {
            _colorType = ColorType.RgbWithAlpha;
        }

        public void Write(ITexture2DStrategy texture2D, Stream outputStream)
        {
            _width = texture2D.Width;
            _height = texture2D.Height;

            _colorData = TextureHelpers.GetColorData(texture2D);

            // write PNG signature
            outputStream.Write(HeaderChunk.PngSignature, 0, HeaderChunk.PngSignature.Length);

            // write header chunk
            var headerChunk = new HeaderChunk();
            headerChunk.Width = (uint)texture2D.Width;
            headerChunk.Height = (uint)texture2D.Height;
            headerChunk.BitDepth = 8;
            headerChunk.ColorType = _colorType;
            headerChunk.CompressionMethod = 0;
            headerChunk.FilterMethod = 0;
            headerChunk.InterlaceMethod = 0;

            var headerChunkBytes = headerChunk.Encode();
            outputStream.Write(headerChunkBytes, 0, headerChunkBytes.Length);

            // write data chunks
            var encodedPixelData = EncodePixelData();
            var compressedPixelData = new MemoryStream();

            try
            {
                using (var deflateStream = new ZlibStream(new MemoryStream(encodedPixelData), CompressionMode.Compress))
                {
                    deflateStream.CopyTo(compressedPixelData);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("An error occurred during DEFLATE compression.", exception);
            }
            
            var dataChunk = new DataChunk();
            dataChunk.Data = compressedPixelData.ToArray();
            var dataChunkBytes = dataChunk.Encode();
            outputStream.Write(dataChunkBytes, 0, dataChunkBytes.Length);

            // write end chunk
            var endChunk = new EndChunk();
            var endChunkBytes = endChunk.Encode();
            outputStream.Write(endChunkBytes, 0, endChunkBytes.Length);
        }

        private byte[] EncodePixelData()
        {
            List<byte[]> filteredScanlines = new List<byte[]>();

            int bytesPerPixel = CalculateBytesPerPixel();
            byte[] previousScanline = new byte[_width * bytesPerPixel];

            for (int y = 0; y < _height; y++)
            {
                var rawScanline = GetRawScanline(y);

                var filteredScanline = GetOptimalFilteredScanline(rawScanline, previousScanline, bytesPerPixel);

                filteredScanlines.Add(filteredScanline);

                previousScanline = rawScanline;
            }

            List<byte> result = new List<byte>();

            foreach (var encodedScanline in filteredScanlines)
            {
                result.AddRange(encodedScanline);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Applies all PNG filters to the given scanline and returns the filtered scanline that is deemed
        /// to be most compressible, using lowest total variation as proxy for compressibility.
        /// </summary>
        /// <param name="rawScanline"></param>
        /// <param name="previousScanline"></param>
        /// <param name="bytesPerPixel"></param>
        /// <returns></returns>
        private byte[] GetOptimalFilteredScanline(byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            var candidates = new List<Tuple<byte[], int>>();
            
            var sub = SubFilter.Encode(rawScanline, bytesPerPixel);
            candidates.Add(new Tuple<byte[], int>(sub, CalculateTotalVariation(sub)));

            var up = UpFilter.Encode(rawScanline, previousScanline);
            candidates.Add(new Tuple<byte[], int>(up, CalculateTotalVariation(up)));

            var average = AverageFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
            candidates.Add(new Tuple<byte[], int>(average, CalculateTotalVariation(average)));

            var paeth = PaethFilter.Encode(rawScanline, previousScanline, bytesPerPixel);
            candidates.Add(new Tuple<byte[], int>(paeth, CalculateTotalVariation(paeth)));

            int lowestTotalVariation = Int32.MaxValue;
            int lowestTotalVariationIndex = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Item2 < lowestTotalVariation)
                {
                    lowestTotalVariationIndex = i;
                    lowestTotalVariation = candidates[i].Item2;
                }
            }

            return candidates[lowestTotalVariationIndex].Item1;
        }

        /// <summary>
        /// Calculates the total variation of given byte array.  Total variation is the sum of the absolute values of
        /// neighbour differences.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int CalculateTotalVariation(byte[] input)
        {
            int totalVariation = 0;

            for (int i = 1; i < input.Length; i++)
            {
                totalVariation += Math.Abs(input[i] - input[i - 1]);
            }

            return totalVariation;
        }

        private byte[] GetRawScanline(int y)
        {
            var rawScanline = new byte[4 * _width];
            
            for (int x = 0; x < _width; x++)
            {
                var color = _colorData[(y * _width) + x];

                rawScanline[4 * x] = color.R;
                rawScanline[(4 * x) + 1] = color.G;
                rawScanline[(4 * x) + 2] = color.B;
                rawScanline[(4 * x) + 3] = color.A;
            }

            return rawScanline;
        }

        private int CalculateBytesPerPixel()
        {
            switch (_colorType)
            {
                case ColorType.Grayscale:
                    return _bitsPerSample / 8;

                case ColorType.GrayscaleWithAlpha:
                    return (2 * _bitsPerSample) / 8;

                case ColorType.Palette:
                    return _bitsPerSample / 8;

                case ColorType.Rgb:
                    return (3 * _bitsPerSample) / 8;

                case ColorType.RgbWithAlpha:
                    return (4 * _bitsPerSample) / 8;

                default:
                    return -1;
            }
        }
    }
}
