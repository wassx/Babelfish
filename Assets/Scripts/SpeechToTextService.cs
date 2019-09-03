using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Services;

public class SpeechToTextService : BaseExtensionService {
    private readonly SpeechConfig _config =
        SpeechConfig.FromSubscription("433296e9a13c48928cdeef3d4d1433d1", "northeurope");

    private readonly object _threadLocker = new object();
    private bool waitingForReco;
    private string _message;

    private bool _micPermissionGranted = false;

    public bool IsListening { get; set; }

    public SpeechToTextService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(name,
        priority, profile) { }

    public async Task<string> RecognizeSpeech() {
//        using (var recognizer = new SpeechRecognizer(_config)) {
//            lock (_threadLocker) {
//                waitingForReco = true;
//            }
//
//            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);
//
//            string newMessage = string.Empty;
//            if (result.Reason == ResultReason.RecognizedSpeech) {
//                newMessage = result.Text;
//            } else if (result.Reason == ResultReason.NoMatch) {
//                newMessage = "NOMATCH: Speech could not be recognized.";
//            } else if (result.Reason == ResultReason.Canceled) {
//                var cancellation = CancellationDetails.FromResult(result);
//                newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
//            }
//
//            lock (_threadLocker) {
//                _message = newMessage;
//                waitingForReco = false;
//            }
//
//            return _message;
//        }
        await TranslationWithMicrophoneAsync();
        return "";
    }

    public async Task TranslationWithMicrophoneAsync() {
        IsListening = true;
        // <TranslationWithMicrophoneAsync>
        // Translation source language.
        // Replace with a language of your choice.
        string fromLanguage = "en-US";

        // Voice name of synthesis output.
        const string GermanVoice = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";

        // Creates an instance of a speech translation config with specified subscription key and service region.
        // Replace with your own subscription key and service region (e.g., "westus").
        var config = SpeechTranslationConfig.FromSubscription("433296e9a13c48928cdeef3d4d1433d1", "northeurope");
        config.SpeechRecognitionLanguage = fromLanguage;
        config.VoiceName = GermanVoice;

        // Translation target language(s).
        // Replace with language(s) of your choice.
        config.AddTargetLanguage("de");

        // Creates a translation recognizer using microphone as audio input.
        using (var recognizer = new TranslationRecognizer(config)) {
            // Subscribes to events.
            recognizer.Recognizing += (s, e) => {
                Debug.Log($"RECOGNIZING in '{fromLanguage}': Text={e.Result.Text}");
                foreach (var element in e.Result.Translations) {
                    Debug.Log($"    TRANSLATING into '{element.Key}': {element.Value}");
                }
            };

            recognizer.Recognized += (s, e) => {
                if (e.Result.Reason == ResultReason.TranslatedSpeech) {
                    Debug.Log($"RECOGNIZED in '{fromLanguage}': Text={e.Result.Text}");
                    foreach (var element in e.Result.Translations) {
                        Debug.Log($"    TRANSLATED into '{element.Key}': {element.Value}");
                    }
                } else if (e.Result.Reason == ResultReason.RecognizedSpeech) {
                    Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
                    Debug.Log($"    Speech not translated.");
                } else if (e.Result.Reason == ResultReason.NoMatch) {
                    Debug.Log($"NOMATCH: Speech could not be recognized.");
                }
            };

            recognizer.Synthesizing += (s, e) => {
                var audio = e.Result.GetAudio();
                Debug.Log(audio.Length != 0
                    ? $"AudioSize: {audio.Length}"
                    : $"AudioSize: {audio.Length} (end of synthesis data)");

                if (audio.Length > 0) {
#if NET461
                        using (var m = new MemoryStream(audio))
                        {
                            SoundPlayer simpleSound = new SoundPlayer(m);
                            simpleSound.PlaySync();
                        }
#endif
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
            } while (IsListening);

            // Stops continuous recognition.
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }

        // </TranslationWithMicrophoneAsync>
    }
}