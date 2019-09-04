using Photon.Pun;

public class Babelfish : MonoBehaviourPun {

    private void Start() {
        // TODO: Get Textsync from object to set text

        // TODO: Get reference to speech to text service
        // TODO: add handlers for successful recognition result
    }

    private void OnDestroy() {
        // Stop recognition
        // Remove handler
    }

    public async void OnStartSpeech() {
        // Start speech recognition by calling service
    }

    public void OnStopSpeech() {
        // Stop speech recognition by calling service
    }
}