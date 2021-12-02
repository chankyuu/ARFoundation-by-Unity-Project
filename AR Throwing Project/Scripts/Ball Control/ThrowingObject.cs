using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowingObject : MonoBehaviour
{
    public float forceFactor;
    private Vector3 forceDirection;
    private Vector2 strength;
    private Vector2 strengthFactor;

    public CameraAxes forceDirectionExtra = CameraAxes.CameraMainTransformUp;
    private Vector3 forceDirectionExtraVector3;

    public enum CameraAxes
    {
        CameraMainTransformUp,
        CameraMainTransformForward,
        CameraMainTransformRight,
        CameraMainTransformUpRight,
        CameraMainTransformLeft,
        CameraMainTransformUpLeft
    }

    [Header("Torque")]
    public CameraAxes torqueAxis = CameraAxes.CameraMainTransformRight;
    private Vector3 torqueAxisVector3;
    private float torqueAngleBasic;
    public float torqueAngle;
    public float torqueFactor;
    private Quaternion torqueRotation;

    [Tooltip("It clamps Torque")]
    public float maxAngularVelocityAtAwake = 7f;

    [Header("Center Of Mass")]
    [Tooltip("Base point for selection of Custom Center Of Mass")]
    public bool isCenterOfMassByDefaultLoggedAtAwake;
    public bool isCenterOfMassCustomUsedAtAwake;
    public Vector3 centerOfMassCustomAtAwake;

    private Quaternion rotationByDefault;

    public enum RotationsForNextThrow
    {
        Default,
        Random,
        Custom
    }

    [Header("Position")]
    [Tooltip("Middle is in the bottom of the screen: (0.5f, 0.1f)"
    + "\nY must be less Y of Input Position Fixed."
    + "\n\nLinked with Input Sensitivity.")]
    public Vector2 positionInViewportOnReset = new Vector2(0.5f, 0.1f);

    [Tooltip("Used for Z coordinate in Reset() & OnTouchForFlick() ")]
    public float cameraNearClipPlaneFactorOnReset = 7.5f;

    [Header("Rotation")]
    public bool isObjectRotatedInThrowDirection = true;
    public RotationsForNextThrow rotationOnReset = RotationsForNextThrow.Default;
    public Vector3 rotationOnResetCustom = new Vector3(0f, 90f, 0f);
    public Color color;

    [HideInInspector]
    public bool isThrown = false;

    [HideInInspector]
    public Rigidbody rigidbody3D;
    private Collider[] colliders3D;

    public bool temp = false;
    private void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        rigidbody3D.maxAngularVelocity = maxAngularVelocityAtAwake;

        if (isCenterOfMassCustomUsedAtAwake)
        {
            rigidbody3D.centerOfMass = centerOfMassCustomAtAwake;
        }

        colliders3D = GetComponentsInChildren<Collider>();
        rotationByDefault = rigidbody3D.rotation;
    }
    public void ResetPosition(Camera cameraMain)
    {
        rigidbody3D.position =
            cameraMain.ViewportToWorldPoint(
                new Vector3(
                    positionInViewportOnReset.x,
                    positionInViewportOnReset.y,
                    cameraMain.nearClipPlane * cameraNearClipPlaneFactorOnReset));
    }
    public void ResetRotation(Transform parent)
    {
        //print(rotationByDefault.eulerAngles);

        switch (rotationOnReset)
        {
            case RotationsForNextThrow.Default:

                if (parent)
                {
                    rigidbody3D.rotation = parent.rotation * rotationByDefault;
                }
                else
                {
                    rigidbody3D.rotation = rotationByDefault;
                }

                break;

            case RotationsForNextThrow.Custom:

                if (parent)
                {
                    rigidbody3D.rotation = parent.rotation * Quaternion.Euler(rotationOnResetCustom);
                }
                else
                {
                    rigidbody3D.rotation = Quaternion.Euler(rotationOnResetCustom);
                }

                break;
        }
    }

    public void Throw(
        Vector2 inputPositionFirst,
        Vector2 inputPositionLast,
        Vector2 inputSensitivity,
        Transform cameraMain,
        int screenHight,
        float forceFactorExtra,
        float torqueFactorExtra,
        float torqueAngleExtra)
    {
        strengthFactor = inputPositionLast - inputPositionFirst;

        if (inputPositionLast.y < screenHight / 2 && Mathf.Abs(strengthFactor.y) > 0f)
        {
            strengthFactor.x *= inputPositionLast.y / strengthFactor.y;
        }

        strengthFactor /= screenHight;

        strength.y = inputSensitivity.y * strengthFactor.y;
        strength.x = inputSensitivity.x * strengthFactor.x;

        forceDirection = new Vector3(strength.x, 0f, 1f);
        forceDirection = cameraMain.transform.TransformDirection(forceDirection);

        torqueAngleBasic = Mathf.Sign(strengthFactor.x)
            * Vector3.Angle(cameraMain.transform.forward, forceDirection);

        torqueRotation = Quaternion.AngleAxis(
            torqueAngleBasic + torqueAngle + torqueAngleExtra,
            cameraMain.transform.up);

        rigidbody3D.useGravity = true;

        forceDirectionExtraVector3 = GetCameraAxis(cameraMain, forceDirectionExtra);

        rigidbody3D.AddForce(
            (forceDirection + forceDirectionExtraVector3)
            * (forceFactor + forceFactorExtra)
            * strength.y * 0.2f);

        if (isObjectRotatedInThrowDirection)
        {
            rigidbody3D.rotation =
                Quaternion.AngleAxis(
                    Mathf.Sign(strengthFactor.x) * Vector3.Angle(cameraMain.transform.forward, forceDirection),
                    cameraMain.transform.up)
                * rigidbody3D.rotation;
        }

        torqueAxisVector3 = GetCameraAxis(cameraMain, torqueAxis);

        rigidbody3D.AddTorque(torqueRotation * torqueAxisVector3 * (torqueFactor + torqueFactorExtra));

        //Debug.Log("**************************");
        //Debug.Log(forceDirection);
        //Debug.Log(forceDirectionExtraVector3);
        //Debug.Log((forceDirection + forceDirectionExtraVector3)
        //    * (forceFactor + forceFactorExtra)
        //    * strength.y * 0.5f);
        //Debug.Log("**************************");
    }
    private Vector3 GetCameraAxis(Transform cameraMain, CameraAxes cameraAxis)
    {
        switch (cameraAxis)
        {
            case CameraAxes.CameraMainTransformUp:

                return cameraMain.transform.up;

            case CameraAxes.CameraMainTransformForward:

                return cameraMain.transform.forward;

            case CameraAxes.CameraMainTransformRight:

                return cameraMain.transform.right;

            case CameraAxes.CameraMainTransformUpRight:

                return cameraMain.transform.right + cameraMain.transform.up;

            case CameraAxes.CameraMainTransformLeft:

                return cameraMain.transform.right * -1f;

            case CameraAxes.CameraMainTransformUpLeft:

                return cameraMain.transform.right * -1f + cameraMain.transform.up;

            default:

                return Vector3.zero;
        }
    }
}
