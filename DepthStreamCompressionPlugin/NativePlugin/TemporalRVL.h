#pragma once

#include <vector>

namespace TemporalRVL
{
    struct Pixel
    {
    public:
        short value;
        short invalidCount;
    };

    class TemporalRVLEncoder
    {
    public:
        TemporalRVLEncoder(int frameSize, short changeThreshold, int invalidThreshold);
        int Encode(short* depthBuffer, char* output, bool keyFrame);

    private:
        std::vector<Pixel>_pixels;
        short _changeThreshold;
        int _invalidThreshold;
    };

    class TemporalRVLDecoder
    {
    public:
        TemporalRVLDecoder(int frameSize);
        void Decode(char* trvlFrame, short* output, bool keyFrame);

    private:
        std::vector<short> _pixelDiffs;
    };
}