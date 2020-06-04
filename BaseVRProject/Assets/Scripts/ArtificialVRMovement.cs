using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArtificialVRMovement : MonoBehaviour
{
    public float maxSpeed = 1.0f;
    public InputAction moveAction = new InputAction("moveAction", binding: "<LeftHand XR Controller>/thumbstick");

    public enum MovementType {Head,LeftHand,RightHand}
    public MovementType movementType;
    public bool threeDMovement = false;

    [Header("Required Scene/Child References")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Debug settings")]
    public bool logErrorsAfterInitialFailure = false;

    private bool hasThrownError = false;

    // Update is called once per frame
    void Update()
    {
        if (!head || !leftHand || !rightHand)
        {
            LogError();
            return;
        }

        Vector3 verticalMovementVector;

        switch (movementType)
        {
            case MovementType.LeftHand:
                verticalMovementVector = leftHand.forward;
                break;
            case MovementType.RightHand:
                verticalMovementVector = rightHand.forward;
                break;
            // Using default case to set head so verticalMovementVector counts as assigned
            default:
                verticalMovementVector = head.forward;
                break;
        }

        if (!threeDMovement)
        {
            // Project offhandMovementHand forward axis onto floor plane so that the player doesn't fly
            // Also, normalize (so that lifting controller up and down does not affect magnitude)
            verticalMovementVector = Vector3.ProjectOnPlane(verticalMovementVector, Vector3.up).normalized;
        }

        Vector3 horizontalMovementVector = Vector3.Cross(verticalMovementVector, -Vector3.up);

        Vector2 thumbstickMovementVector = moveAction.ReadValue<Vector2>();

        Debug.Log(thumbstickMovementVector);  // WHY IS THIS ALWAYS 0.0f, 0.0f ?!?

        transform.position += verticalMovementVector * thumbstickMovementVector.y * Time.deltaTime;
        transform.position += horizontalMovementVector * thumbstickMovementVector.x * Time.deltaTime;
    }

    void OnEnable()
    {
        // Enable InputActions so they actually work
        moveAction.Enable();
    }

    void OnDisable()
    {
        // Disable InputActions just-in-case
        moveAction.Disable();
    }

    void LogError()
    {
        if(logErrorsAfterInitialFailure || !hasThrownError)
        {
            Debug.LogError("Not all Required Scene/Child References are set!", gameObject);
            hasThrownError = true;
        }
        return;
    }
}
