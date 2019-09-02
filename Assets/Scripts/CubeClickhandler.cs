using UnityEngine;
using XRTK.Utilities;
using XRTK.Utilities.Async;

public class CubeClickhandler : MonoBehaviour {
    private Renderer _renderer;
    private Color _active = new Color(0.8f,0.8f,0.9f);
    private Color _listening = new Color(0.3f,0.4f,1f);

    private void Start() {
        _renderer = gameObject.GetComponent<Renderer>();
        _renderer.material.color = _active;
    }

    public async void OnCubeClicked() {
        _renderer.material.color = _listening;
        Babbelfish babbelfish = CameraCache.Main.GetComponentInChildren<Babbelfish>();
        babbelfish.OnStartSpeech();
        await new WaitForSeconds(2);
        _renderer.material.color = _active;
    }
}