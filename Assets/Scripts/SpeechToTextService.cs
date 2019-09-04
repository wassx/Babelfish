using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Services;

public class SpeechToTextService : BaseExtensionService {
    private const string Region = "northeurope";
    private const string SubscriptionKey = "<yourid>";
    private readonly SpeechTranslationConfig _config = SpeechTranslationConfig.FromSubscription(SubscriptionKey, Region);

    private readonly object _threadLocker = new object();
    private bool _isListening;

    public SpeechToTextService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(
        name, priority, profile) {
        _config.SpeechRecognitionLanguage = "en-US";
        _config.AddTargetLanguage("de");
        _config.AddTargetLanguage("ja");
        _config.AddTargetLanguage("ar");
    }

    public async Task StartRecognizeSpeech() {
        using (TranslationRecognizer recognizer = new TranslationRecognizer(_config)) {
            lock (_threadLocker) {
                _isListening = true;
            }
            
            recognizer.Recognized += (s, e) => {
                if (e.Result.Reason == ResultReason.TranslatedSpeech) {
                    foreach (KeyValuePair<string,string> element in e.Result.Translations) {
                        Debug.Log("Language: " + element.Key + " Translation: " + element.Value);
                    }
                } else if (e.Result.Reason == ResultReason.NoMatch) {
                    Debug.LogWarning("No match for: " + e.Result.Text);
                }
            };

            recognizer.SessionStarted += (s, e) => { Debug.Log("\nSession started event."); };
            recognizer.SessionStopped += (s, e) => { Debug.Log("\nSession stopped event"); };

            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            do {
                Debug.Log("Listening...");
            } while (_isListening);

            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }
    }

    public void StopRecognizeSpeech() {
        lock (_threadLocker) {
            _isListening = false;
        }
    }
}