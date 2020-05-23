#include "pch.h"
#include "NativePlugin.h"

int EncodeRVL(short* input, char* output, int numPixels)
{
	return RVL::CompressRVL(input, output, numPixels);
}

void DecodeRVL(char* input, short* output, int numPixels)
{
	RVL::DecompressRVL(input, output, numPixels);
}

TemporalRVL::TemporalRVLEncoder* CreateTemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold)
{
	return new TemporalRVL::TemporalRVLEncoder(frameSize, changeThreshold, invalidThreshold);
}

TemporalRVL::TemporalRVLDecoder* CreateTemporalRVLDecoder(int frameSize)
{
	return new TemporalRVL::TemporalRVLDecoder(frameSize);
}

void DeleteTemporalRVLEncoder(TemporalRVL::TemporalRVLEncoder* encoder)
{
	delete encoder;
}

void DeleteTemporalRVLDecoder(TemporalRVL::TemporalRVLDecoder* decoder)
{
	delete decoder;
}

int EncodeTemporalRVL(TemporalRVL::TemporalRVLEncoder* encoder, short* depthBuffer, char* output, bool keyFrame)
{
	return encoder->Encode(depthBuffer, output, keyFrame);
}

void DecodeTemporalRVL(TemporalRVL::TemporalRVLDecoder* decoder, char* trvlFrame, short* output, bool keyFrame)
{
	decoder->Decode(trvlFrame, output, keyFrame);
}
