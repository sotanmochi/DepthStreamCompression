#define EXPORT_API __declspec(dllexport)

#include "RVL.h"
#include "TemporalRVL.h"

extern "C"
{
	EXPORT_API int EncodeRVL(short* input, char* output, int numPixels);
	EXPORT_API void DecodeRVL(char* input, short* output, int numPixels);
	EXPORT_API TemporalRVL::TemporalRVLEncoder* CreateTemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold);
	EXPORT_API TemporalRVL::TemporalRVLDecoder* CreateTemporalRVLDecoder(int frameSize);
	EXPORT_API void DeleteTemporalRVLEncoder(TemporalRVL::TemporalRVLEncoder* encoder);
	EXPORT_API void DeleteTemporalRVLDecoder(TemporalRVL::TemporalRVLDecoder* decoder);	
	EXPORT_API int EncodeTemporalRVL(TemporalRVL::TemporalRVLEncoder* encoder, short* depthBuffer, char* output, bool keyFrame);
	EXPORT_API void DecodeTemporalRVL(TemporalRVL::TemporalRVLDecoder* decoder, char* trvlFrame, short* output, bool keyFrame);	
}
