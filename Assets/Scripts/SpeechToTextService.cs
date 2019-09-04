using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Services;

public class SpeechToTextService : BaseExtensionService {
    public Action<string> OnRecognitionSuccessful;

    private const string FromLanguage = "en-US";
    private const string GermanVoice = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";

    private const string SubscriptionKey = "76f89a5ca8dd42cf802bf7173b01359b";
    private const string Region = "northeurope";

    private readonly SpeechTranslationConfig _config = SpeechTranslationConfig.FromSubscription(SubscriptionKey, Region);
    private readonly SpeechConfig _outputSpeechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);

    private readonly object _threadLocker = new object();
    private readonly Queue<Action> _dispatchQueue = new Queue<Action>();

    private bool _isListening;

    public SpeechToTextService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(name,
        priority, profile) {
        _outputSpeechConfig.SpeechSynthesisLanguage = "de-DE";
        _outputSpeechConfig.SpeechSynthesisVoiceName = GermanVoice;

        _config.SpeechRecognitionLanguage = FromLanguage;
        _config.VoiceName = GermanVoice;
        _config.SpeechSynthesisLanguage = "de-DE";

        _config.AddTargetLanguage("de");
//        config.AddTargetLanguage("ar");
//        config.AddTargetLanguage("ja");
    }

    public async Task StartRecognizeSpeech() {
        using (TranslationRecognizer recognizer = new TranslationRecognizer(_config)) {
            lock (_threadLocker) {
                _isListening = true;
            }

            // Subscribes to events.
            recognizer.Recognizing += (s, e) => {
                Debug.Log($"RECOGNIZING in '{FromLanguage}': Text={e.Result.Text}");
                foreach (KeyValuePair<string, string> element in e.Result.Translations) {
                    Debug.Log($"    TRANSLATING into '{element.Key}': {element.Value}");
                }
            };

            recognizer.Recognized += (s, e) => {
                string message = "";
                if (e.Result.Reason == ResultReason.TranslatedSpeech) {
                    Debug.Log($"RECOGNIZED in '{FromLanguage}': Text={e.Result.Text}");
                    foreach (KeyValuePair<string, string> element in e.Result.Translations) {
                        Debug.Log($"    TRANSLATED into '{element.Key}': {element.Value}");
                        message += "\n" + element.Value;
                    }
                } else if (e.Result.Reason == ResultReason.RecognizedSpeech) {
                    Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
                    Debug.Log($"    Speech not translated.");
                } else if (e.Result.Reason == ResultReason.NoMatch) {
                    message = "NOMATCH: Speech could not be recognized.";
                }

                QueueOnUpdate(() => { OnRecognitionSuccessful?.Invoke(message); });
            };

            recognizer.Synthesizing += (s, e) => {
                byte[] audio = e.Result.GetAudio();
                Debug.Log(audio.Length != 0
                    ? $"AudioSize: {audio.Length}"
                    : $"AudioSize: {audio.Length} (end of synthesis data)");

                //PlayAudio(audio);
            };

            recognizer.Canceled += (s, e) => {
                Debug.Log($"CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error) {
                    Debug.Log($"CANCELED: ErrorCode={e.ErrorCode}");
                    Debug.Log($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    Debug.Log($"CANCELED: Did you update the subscription info?");
                }
            };

            recognizer.SessionStarted += (s, e) => { Debug.Log("\nSession started event."); };

            recognizer.SessionStopped += (s, e) => { Debug.Log("\nSession stopped event."); };

            // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
            Debug.Log("Say something...");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            do {
                Debug.Log("Listening...");
            } while (_isListening);

            // Stops continuous recognition.
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }

        // </TranslationWithMicrophoneAsync>
    }

    public void StopRecognition() {
        lock (_threadLocker) {
            _isListening = false;
        }
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

    private float[] ConvertByteToFloat(byte[] array) {
        float[] floatArr = new float[array.Length / 4];
        for (int i = 0; i < floatArr.Length; i++) {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(array, i * 4, 4);
//            floatArr[i] = BitConverter.ToSingle(array, i*4) / 0x80000000;
            floatArr[i] = BitConverter.ToSingle(array, i * 4);
        }

        return floatArr;
    }
}