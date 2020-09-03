using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-100)]
public class InputControllScript : MonoBehaviour
{
    public float verticalThreshold = 0.5f;
   // public Thumbstic thumbstic;
    //public TouchButton jumpBtn;

    [HideInInspector] public float horizontal;
    [HideInInspector] public bool jumpHeld;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool crouchHeld;
    [HideInInspector] public bool crouchPressed;

    bool dPadCrouchPrev;
    bool readyToClear;

    void Update()
    {
        ClearInput();

       // if (GameManager.IsGameOver())
       // return;

        ProcessInputs();
       // ProcessTouchInputs();

        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
    }

    void FixedUpdate()
    {
        readyToClear = true;
    }

    void ClearInput()
    {
        if (!readyToClear)
        return;

        horizontal      = 0f;
        jumpPressed     = false;
        jumpHeld        = false;
        crouchPressed   = false;
        crouchHeld      = false;

        readyToClear    = false;
    }

    void ProcessInputs()
    {
        horizontal      += Input.GetAxis("Horizontal");

        jumpPressed     = jumpPressed || Input.GetButtonDown("Jump");
        jumpHeld        = jumpHeld || Input.GetButton("Jump");

        crouchPressed   = crouchPressed || Input.GetButtonDown("Crouch");
        crouchHeld      = crouchHeld || Input.GetButton("Crouch");
    }

}
