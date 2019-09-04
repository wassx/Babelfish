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

    private readonly SpeechTranslationConfig config =
        SpeechTranslationConfig.FromSubscription("76f89a5ca8dd42cf802bf7173b01359b", "northeurope");

    private bool _isListening;
    private readonly object _threadLocker = new object();
    private readonly Queue<Action> _dispatchQueue = new Queue<Action>();

    public SpeechToTextService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(name,
        priority, profile) { }

    public async Task StartRecognizeSpeech() {
        // <TranslationWithMicrophoneAsync>
        // Translation source language.
        // Replace with a language of your choice.
        string fromLanguage = "en-US";

        // Voice name of synthesis output.
        const string GermanVoice = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";

        config.SpeechRecognitionLanguage = fromLanguage;
        config.VoiceName = GermanVoice;

        // Translation target language(s).
        // Replace with language(s) of your choice.
        config.AddTargetLanguage("de");
        config.AddTargetLanguage("ar");
        config.AddTargetLanguage("ja");

        // Creates a translation recognizer using microphone as audio input.
        using (var recognizer = new TranslationRecognizer(config)) {
            lock (_threadLocker) {
                _isListening = true;
            }

            // Subscribes to events.
            recognizer.Recognizing += (s, e) => {
                Debug.Log($"RECOGNIZING in '{fromLanguage}': Text={e.Result.Text}");
                foreach (var element in e.Result.Translations) {
                    Debug.Log($"    TRANSLATING into '{element.Key}': {element.Value}");
                }
            };

            recognizer.Recognized += (s, e) => {
                string message = "";
                if (e.Result.Reason == ResultReason.TranslatedSpeech) {
                    Debug.Log($"RECOGNIZED in '{fromLanguage}': Text={e.Result.Text}");
                    foreach (var element in e.Result.Translations) {
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

                if (audio.Length > 0) {
                    Wav wav = new Wav(audio);
                    QueueOnUpdate(() => {
                        Debug.Log(wav);
                        AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false, false);
                        audioClip.SetData(wav.LeftChannel, 0);
                        AudioSource.PlayClipAtPoint(audioClip, new Vector3(0, 0, 0), 1.0f);
                    });
                }
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
            floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
        }

        return floatArr;
    }
}