using UnityEngine;
using UnityEngine.XR.WSA;

public class ArVrPosition : MonoBehaviour {
    [SerializeField] private Vector3 _arPosition = new Vector3(0, 1.7f, 0);
    [SerializeField] private Vector3 _vrPosition = new Vector3(0, 0, 0);

    private void Awake() {
        transform.localPosition = HolographicSettings.IsDisplayOpaque
            ? _vrPosition
            : _arPosition;
    }
}