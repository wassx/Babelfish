public class AudioByteConverter {
    public static float[] ConvertByteToFloat(byte[] audio) {
        int sampleCount = audio.Length / 2;
        float[] audioData = new float[sampleCount];
        for (int i = 0; i < sampleCount; ++i) {
            audioData[i] = (short) (audio[i * 2 + 1] << 8 | audio[i * 2]) / 32768.0F;
        }

        return audioData;
    }
}