using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using XRTK.Definitions;
using XRTK.Services;

public class SpeechToTextService : BaseExtensionService  {
    private readonly SpeechConfig _config = SpeechConfig.FromSubscription("<azureid>", "northeurope");
    private readonly object _threadLocker = new object();
    private bool waitingForReco;
    private string _message;

    private bool _micPermissionGranted = false;

    public SpeechToTextService(string name, uint priority, BaseMixedRealityExtensionServiceProfile profile) : base(name, priority, profile) { }
    
    public async Task<string> RecognizeSpeech() {
        using (var recognizer = new SpeechRecognizer(_config)) {
            lock (_threadLocker) {
                waitingForReco = true;
            }

            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            string newMessage = string.Empty;
            if (result.Reason == ResultReason.RecognizedSpeech) {
                newMessage = result.Text;
            } else if (result.Reason == ResultReason.NoMatch) {
                newMessage = "NOMATCH: Speech could not be recognized.";
            } else if (result.Reason == ResultReason.Canceled) {
                var cancellation = CancellationDetails.FromResult(result);
                newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
            }

            lock (_threadLocker) {
                _message = newMessage;
                waitingForReco = false;
            }

            return _message;
        }
    }
}