using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // ⛔ Bloquear todo input si el juego está en pausa
        if (GamePauseManager.IsPaused)
            return;

        // Seguridad extra (Input System)
        if (Keyboard.current == null)
            return;

        Vector3 movement = Vector3.zero;

        if (Keyboard.current.upArrowKey.isPressed)
            movement.z += 1f;

        if (Keyboard.current.downArrowKey.isPressed)
            movement.z -= 1f;

        if (Keyboard.current.leftArrowKey.isPressed)
            movement.x -= 1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            movement.x += 1f;

        movement.Normalize();

        transform.Translate(
            movement * moveSpeed * Time.deltaTime,
            Space.World
        );
    }
}