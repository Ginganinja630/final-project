using UnityEngine;
using UnityEngine.InputSystem;   // NEW input system

public class OrbitCamera : MonoBehaviour
{
    public Transform target;        // what we orbit around (ViewCenter)
    public float distance = 70f;    // starting distance
    public float minDistance = 20f;
    public float maxDistance = 200f;

    public float rotateSpeed = 100f;  // how fast we rotate with the mouse
    public float zoomSpeed = 40f;     // how fast we zoom with the wheel

    public float minPitch = 10f;      // up/down angle limits
    public float maxPitch = 80f;

    private float yaw = 0f;           // left-right angle
    private float pitch = 30f;        // up-down angle

    private void Start()
    {
        if (target != null)
        {
            // Initialize angles based on current camera position
            Vector3 dir = transform.position - target.position;
            distance = dir.magnitude;

            Quaternion rot = Quaternion.LookRotation(dir);
            yaw = rot.eulerAngles.y;
            pitch = rot.eulerAngles.x;
        }
    }

    private void LateUpdate()
    {
        // if no target or no mouse, do nothing
        if (target == null || Mouse.current == null)
            return;

        var mouse = Mouse.current;

        // ----- ZOOM with scroll wheel -----
        float scroll = mouse.scroll.ReadValue().y;  // positive = wheel up
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // ----- ROTATE with right mouse drag -----
        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();   // mouse movement this frame

            yaw   += delta.x * rotateSpeed * Time.deltaTime;
            pitch -= delta.y * rotateSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // ----- APPLY position + rotation -----
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = target.position + offset;
        transform.rotation = rotation;
    }
}