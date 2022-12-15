using UnityEngine;

public class TileEditorCameraController : MonoBehaviour
{
    public float LookSpeedH = 2f;
    public float LookSpeedV = 2f;
    public float MoveSpeed = 10f;
    public float FastMoveSpeed = 20f;
     
    private float _yaw = 0f;
    private float _pitch = 0f;
         
    void Update ()
    {
        if (!Input.GetMouseButton(1))
            return;
        
        _yaw += LookSpeedH * Input.GetAxis("Mouse X");
        _pitch -= LookSpeedV * Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);

        var translation = Vector3.zero;
        if (Input.GetKey(KeyCode.S))
            translation += Vector3.back;
        if (Input.GetKey(KeyCode.W))
            translation += Vector3.forward;
        if (Input.GetKey(KeyCode.D))
            translation += Vector3.right;
        if (Input.GetKey(KeyCode.A))
            translation += Vector3.left;
        if (Input.GetKey(KeyCode.Q))
            translation += Vector3.down;
        if (Input.GetKey(KeyCode.E))
            translation += Vector3.up;
        if (translation.sqrMagnitude > 0)
        {
            var translationDelta = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? FastMoveSpeed : MoveSpeed);
            transform.Translate(translation * translationDelta, Space.Self);
        }
    }
}
