using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using UnityEngine;
using XRTK.Definitions;
using XRTK.Services;

public class SpeechToTextService : BaseExtensionService {
    public Action<Dictionary<string, string>> OnTranslationSuccessful;

    private const string Region = "northeurope";
    private const string SubscriptionKey = "<yourid>";

    private readonly SpeechTranslationConfig
        _config = SpeechTranslationConfig.FromSubscription(SubscriptionKey, Region);

    private readonly object _threadLocker = new object();
    private bool _isListening;

    private readonly Queue<Action> _dispatchQueue = new Queue<Action>();

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
                Dictionary<string, string> results = new Dictionary<string, string>();
                if (e.Result.Reason == ResultReason.TranslatedSpeech) {
                    foreach (KeyValuePair<string, string> element in e.Result.Translations) {
                        Debug.Log("Language: " + element.Key + " Translation: " + element.Value);
                        results.Add(element.Key, element.Value);
                    }
                } else if (e.Result.Reason == ResultReason.NoMatch) {
                    Debug.LogWarning("No match for: " + e.Result.Text);
                    results.Add("default", "No match for: " + e.Result.Text);
                }

                QueueOnUpdate(() => { OnTranslationSuccessful?.Invoke(results); });
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