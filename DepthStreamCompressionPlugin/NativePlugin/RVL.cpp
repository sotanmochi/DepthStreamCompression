//
// This is a modified version of the RVL source code from 
// Fast Lossless Depth Image Compression (A. D. Wilson, 2017).
//
// This code is licensed under the MIT License.
// Copyright (c) 2017 Andrew D. Wilson
// Copyright (c) 2020 Soichiro Sugimoto
//
// The original RVL source code is available in Wilson's paper.
//
// A. D. Wilson. (2017). Fast Lossless Depth Image Compression. 
// https://dl.acm.org/doi/pdf/10.1145/3132272.3134144
// https://www.microsoft.com/en-us/research/uploads/prod/2018/09/p100-wilson.pdf
//
// -----
//
// MIT License
//
// Copyright (c) 2017 Andrew D. Wilson
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

namespace RVL
{
	void EncodeVLE(int value, int*& pBuffer, int& word, int& nibblesWritten)
	{
		do
		{
			int nibble = value & 0x7; // lower 3 bits
			if ((value >>= 3) != 0) nibble |= 0x8; // more to come
			word <<= 4;
			word |= nibble;
			if (++nibblesWritten == 8) // output word
			{
				*pBuffer++ = word;
				nibblesWritten = 0;
				word = 0;
			}
		} while (value);
	}

	int DecodeVLE(int*& pBuffer, int& word, int& nibblesWritten)
	{
		unsigned int nibble;
		int value = 0, bits = 29;
		do
		{
			if (!nibblesWritten)
			{
				word = *pBuffer++; // load word
				nibblesWritten = 8;
			}
			nibble = word & 0xf0000000;
			value |= (nibble << 1) >> bits;
			word <<= 4;
			nibblesWritten--;
			bits -= 3;
		} while (nibble & 0x80000000);
		return value;
	}

	int CompressRVL(short* input, char* output, int numPixels)
	{
		int *buffer, *pBuffer;
		buffer = pBuffer = (int*)output;

		int word = 0;
		int nibblesWritten = 0;
		short previous = 0;

		short* end = input + numPixels;
		while (input != end)
		{
			int zeros = 0, nonzeros = 0;
			for (; (input != end) && !*input; input++, zeros++);
			EncodeVLE(zeros, pBuffer, word, nibblesWritten); // number of zeros
			for (short* p = input; (p != end) && *p++; nonzeros++);
			EncodeVLE(nonzeros, pBuffer, word, nibblesWritten); // number of nonzeros
			for (int i = 0; i < nonzeros; i++)
			{
				short current = *input++;
				int delta = current - previous;
				int positive = (delta << 1) ^ (delta >> 31);
				EncodeVLE(positive, pBuffer, word, nibblesWritten); // nonzero value
				previous = current;
			}
		}

		if (nibblesWritten) // last few values
		{
			*pBuffer++ = word << 4 * (8 - nibblesWritten);
		}

		return int((char*)pBuffer - (char*)buffer); // num bytes
	}

	void DecompressRVL(char* input, short* output, int numPixels)
	{
		int *buffer, *pBuffer;
		buffer = pBuffer = (int*)input;

		int word = 0;
		int nibblesWritten = 0;
		short current, previous = 0;

		int numPixelsToDecode = numPixels;
		while (numPixelsToDecode)
		{
			int zeros = DecodeVLE(pBuffer, word, nibblesWritten); // number of zeros
			numPixelsToDecode -= zeros;
			for (; zeros; zeros--)
			{
				*output++ = 0;
			}
			int nonzeros = DecodeVLE(pBuffer, word, nibblesWritten); // number of nonzeros
			numPixelsToDecode -= nonzeros;
			for (; nonzeros; nonzeros--)
			{
				int positive = DecodeVLE(pBuffer, word, nibblesWritten); // nonzero value
				int delta = (positive >> 1) ^ -(positive & 1);
				current = previous + delta;
				*output++ = current;
				previous = current;
			}
		}
	}
}