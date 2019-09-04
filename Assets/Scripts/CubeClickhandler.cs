using UnityEngine;
using XRTK.Utilities;

public class CubeClickhandler : MonoBehaviour {
    private Color _active = new Color(0.7f, 0.7f, 0.7f);
    private Color _listening = new Color(0.1f, 0.4f, 0.8f);
    private Renderer _renderer;

    private void Start() {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = _active;
    }

    public void OnStartListening() {
        _renderer.material.color = _listening;
        CameraCache.Main.GetComponentInChildren<Babelfish>().OnStartSpeech();
    }

    public void OnStopListening() {
        _renderer.material.color = _active;
        CameraCache.Main.GetComponentInChildren<Babelfish>().OnStopSpeech();
    }
}