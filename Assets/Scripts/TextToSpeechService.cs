using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Services;

public class TextToSpeechService : BaseExtensionService {
    private const string SubscriptionKey = "<yourkey>";
    private const string Region = "northeurope";
    private readonly Queue<Action> _dispatchQueue = new Queue<Action>();

    private readonly SpeechConfig _config = SpeechConfig.FromSubscription(SubscriptionKey, Region);

    public TextToSpeechService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(name,
        priority, profile) {
        _config.SpeechSynthesisLanguage = "de-DE";
        _config.SpeechSynthesisVoiceName = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";
    }

    public async Task TextToSpeech(string text) {
        using (SpeechSynthesizer synthesizer = new SpeechSynthesizer(_config)) {
            using (SpeechSynthesisResult result = await synthesizer.SpeakTextAsync(text).ConfigureAwait(false)) {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted) {
                    PlayAudio(result.AudioData);
                }
            }
        }
    }

    private void PlayAudio(byte[] audio) {
        float[] audioData = AudioByteConverter.ConvertByteToFloat(audio);

        QueueOnUpdate(() => {
            AudioClip audioClip = AudioClip.Create("SynthesizedAudio", audioData.Length, 1, 16000, false);
            audioClip.SetData(audioData, 0);
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero, 1.0f);
        });
    }

    private void QueueOnUpdate(Action action) {
        lock (_dispatchQueue) {
            _dispatchQueue.Enqueue(action);
        }
    }

    public override void Update() {
        lock (_dispatchQueue) {
            if (_dispatchQueue.Count > 0) {
                _dispatchQueue.Dequeue()();
            }
        }
    }
}