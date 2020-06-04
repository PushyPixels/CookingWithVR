using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class WorldGrabMovement : MonoBehaviour
{
    [Header("Don't forget to set Rigidbodies under Left/Right hand!")]

    public HandInfo leftHand = new HandInfo(new InputAction("LeftGrip", binding: "<LeftHand XR Controller>/grip"));
    public HandInfo rightHand = new HandInfo(new InputAction("RightGrip", binding: "<RightHand XR Controller>/grip"));
    public LayerMask grabbableObjectLayerMask = -1;

    [System.Serializable]
    public class HandInfo
    {
        [Header("Required Component References")]
        public Rigidbody rigidbody;
        public InputAction gripAction;

        [Header("Debug Info")]
        public HandState state;
        public Vector3 grabOffset;
        public Rigidbody grippedObject;
        public Vector3 previousPostion;
        public bool skip;

        public HandInfo(InputAction newGripAction)
        {
            gripAction = newGripAction;
        }
    }
    public enum HandState { Empty, MovementGrab, ObjectGrab }

    [Header("Required Component References")]
    public new Rigidbody rigidbody;

	// Update is called once per frame
	void Update ()
    {
        ObjectGrabbingLogic(leftHand);
        ObjectGrabbingLogic(rightHand);

        WorldMovementLogic(leftHand,rightHand);
        WorldMovementLogic(rightHand,leftHand);

        if(leftHand.skip && leftHand.gripAction.ReadValue<float>() < 0.25f)
        {
            leftHand.skip = false;
        }
        if(rightHand.skip && rightHand.gripAction.ReadValue<float>() < 0.25f)
        {
            rightHand.skip = false;
        }

        leftHand.previousPostion = leftHand.rigidbody.transform.position;
        rightHand.previousPostion = rightHand.rigidbody.transform.position;
    }

    void ObjectGrabbingLogic(HandInfo hand)
    {
        Collider[] colliders = Physics.OverlapSphere(hand.previousPostion, 0.05f*transform.localScale.x, grabbableObjectLayerMask);
        if (hand.gripAction.ReadValue<float>() >= 0.25f)
        {
            if (colliders.Length > 0 && hand.state == HandState.Empty)
            {
                Collider firstColliderFound = colliders[0];
                Rigidbody rb = firstColliderFound.GetComponent<Rigidbody>();

                if(rb != null)
                {
                    rb.isKinematic = false;
                    hand.grabOffset = firstColliderFound.transform.position - hand.rigidbody.transform.position;
                    hand.grippedObject = rb;
                    FixedJoint joint = rb.gameObject.AddComponent<FixedJoint>();
                    joint.enablePreprocessing = false;
                    joint.connectedBody = hand.rigidbody;
                    hand.state = HandState.ObjectGrab;
                    hand.rigidbody.GetComponentInChildren<MeshRenderer>().enabled = false;
                }
            }
        }
        if (hand.state == HandState.ObjectGrab && hand.gripAction.ReadValue<float>() < 0.25f)
        {
            Rigidbody rb = hand.grippedObject;
            FixedJoint joint = rb.GetComponent<FixedJoint>();
            Destroy(joint);
            
            if (rb != null)
            {
                rb.velocity = (hand.rigidbody.transform.position - hand.previousPostion) / Time.deltaTime;
            }

            hand.state = HandState.Empty;
            hand.rigidbody.GetComponentInChildren<MeshRenderer>().enabled = true;
        }
    }

    void WorldMovementLogic(HandInfo hand, HandInfo oppositeHand)
    {
        if (!hand.skip && hand.state != HandState.ObjectGrab)
        {
            if (hand.gripAction.ReadValue<float>() >= 0.25f)
            {
                rigidbody.velocity = Vector3.zero;
                transform.position += hand.previousPostion - hand.rigidbody.transform.position;
                hand.state = HandState.MovementGrab;

                if (oppositeHand.state == HandState.MovementGrab)
                {
                    oppositeHand.skip = true;
                    oppositeHand.state = HandState.Empty;
                }
            }
            if (hand.state == HandState.MovementGrab && hand.gripAction.ReadValue<float>() < 0.25f)
            {
                rigidbody.velocity = (hand.previousPostion - hand.rigidbody.transform.position) / Time.deltaTime;
                hand.state = HandState.Empty;
            }
        }
    }

    void OnEnable()
    {
        // Enable InputActions so they actually work
        leftHand.gripAction.Enable();
        rightHand.gripAction.Enable();
    }

    void OnDisable()
    {
        // Disable InputActions just-in-case
        leftHand.gripAction.Disable();
        rightHand.gripAction.Disable();
    }

    void OnValidate()
    {
        rigidbody = GetComponent<Rigidbody>();
	}
}
