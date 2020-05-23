//
// This is a modified version of the Temporal RVL source code from 
// Temporal RVL: A Depth Stream Compression Method (H. Jun and J. Bailenson., 2020).
//
// This code is licensed under the MIT License.
// Copyright (c) 2020 Hanseul Jun
// Copyright (c) 2020 Soichiro Sugimoto
//
// The original Temporal RVL source code is available on GitHub.
// https://github.com/hanseuljun/temporal-rvl/blob/master/cpp/src/trvl.h
//
// H. Jun and J. Bailenson. (2020). Temporal RVL: A Depth Stream Compression Method. 
// https://vhil.stanford.edu/mm/2020/02/jun-vr-temporal.pdf
//
// -----
//
// MIT License
//
// Copyright (c) 2020 Hanseul Jun
// Copyright (c) 2020 Soichiro Sugimoto
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

#include "pch.h"
#include "RVL.h"
#include "TemporalRVL.h"

namespace TemporalRVL
{
    short AbsDiff(short x, short y)
    {
        if (x > y) { return x - y; }
        else { return y - x; }
    }

    void UpdatePixel(Pixel& pixel, short rawValue, short changeThreshold, int invalidationThreshold)
    {
        if (pixel.value == 0)
        {
            if (rawValue > 0)
            {
                pixel.value = rawValue;
            }
            return;
        }

        // Reset the pixel if the depth value indicates the input was invalid two times in a row.
        if (rawValue == 0)
        {
            ++pixel.invalidCount;
            if (pixel.invalidCount >= invalidationThreshold)
            {
                pixel.value = 0;
                pixel.invalidCount = 0;
            }
            return;
        }
        pixel.invalidCount = 0;

        // Update pixel value when change is detected.
        if (AbsDiff(pixel.value, rawValue) > changeThreshold)
        {
            pixel.value = rawValue;
        }
    }

    TemporalRVLEncoder::TemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold)
        : _pixels(frameSize), _changeThreshold(changeThreshold), _invalidThreshold(invalidThreshold)
    {
    }

    int TemporalRVLEncoder::Encode(short* depthBuffer, char* output, bool keyFrame)
    {
        int frameSize = _pixels.size();

        if (keyFrame)
        {
            for (int i = 0; i < frameSize; i++)
            {
                _pixels[i].value = depthBuffer[i];
                _pixels[i].invalidCount = (depthBuffer[i] == 0) ? 1 : 0;
            }

            return RVL::CompressRVL(depthBuffer, output, frameSize);;
        }

        std::vector<short> pixelDiffs(frameSize);
        for (int i = 0; i < frameSize; i++)
        {
            pixelDiffs[i] = _pixels[i].value;
            UpdatePixel(_pixels[i], depthBuffer[i], _changeThreshold, _invalidThreshold);
            pixelDiffs[i] = _pixels[i].value - pixelDiffs[i];
        }

        return RVL::CompressRVL(reinterpret_cast<short*>(pixelDiffs.data()), output, frameSize);;
    }

    TemporalRVLDecoder::TemporalRVLDecoder(int frameSize) : _pixelDiffs(frameSize)
    {
    }

    void TemporalRVLDecoder::Decode(char* trvlFrame, short* output, bool keyFrame)
    {
        int frameSize = _pixelDiffs.size();
        if (keyFrame)
        {
            RVL::DecompressRVL(trvlFrame, output, frameSize);
            return;
        }

        RVL::DecompressRVL(trvlFrame, reinterpret_cast<short*>(_pixelDiffs.data()), frameSize);
        for (int i = 0; i < frameSize; i++)
        {
            output[i] += _pixelDiffs[i];
        }
    }
}
