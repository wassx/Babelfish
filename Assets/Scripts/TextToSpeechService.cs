using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Services;

public class TextToSpeechService : BaseExtensionService {
    private const string GermanVoice = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";
    private const string SubscriptionKey = "76f89a5ca8dd42cf802bf7173b01359b";
    private const string Region = "northeurope";

    private readonly SpeechConfig _outputSpeechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);
    private readonly Queue<Action> _dispatchQueue = new Queue<Action>();

    public TextToSpeechService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(name,
        priority, profile) {
        _outputSpeechConfig.SpeechSynthesisLanguage = "de-DE";
        _outputSpeechConfig.SpeechSynthesisVoiceName = GermanVoice;
    }

    public async Task TextToSpeech(string text) {
        using (SpeechSynthesizer synthesizer = new SpeechSynthesizer(_outputSpeechConfig, null)) {
            // Receive a text from "Text for Synthesizing" text box and synthesize it to speaker.
            using (SpeechSynthesisResult result = await synthesizer.SpeakTextAsync(text).ConfigureAwait(false)) {
                // Checks result.
                if (result.Reason == ResultReason.SynthesizingAudioCompleted) {
                    PlayAudio(result.AudioData);
                }
            }
        }
    }

    private void PlayAudio(byte[] audio) {
        int sampleCount = audio.Length / 2;
        float[] audioData = new float[sampleCount];
        for (int i = 0; i < sampleCount; ++i) {
            audioData[i] = (short) (audio[i * 2 + 1] << 8 | audio[i * 2]) / 32768.0F;
        }

        // The default output audio format is 16K 16bit mono
        QueueOnUpdate(() => {
            AudioClip audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
            audioClip.SetData(audioData, 0);
            AudioSource.PlayClipAtPoint(audioClip, new Vector3(0, 0, 0), 1.0f);
        });
    }

    public override void Update() {
        lock (_dispatchQueue) {
            if (_dispatchQueue.Count > 0) {
                _dispatchQueue.Dequeue()();
            }
        }
    }

    private void QueueOnUpdate(Action updateAction) {
        lock (_dispatchQueue) {
            _dispatchQueue.Enqueue(updateAction);
        }
    }
}