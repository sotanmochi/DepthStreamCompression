#pragma once

namespace RVL
{
	int CompressRVL(short* input, char* output, int numPixels);
	void DecompressRVL(char* input, short* output, int numPixels);
}