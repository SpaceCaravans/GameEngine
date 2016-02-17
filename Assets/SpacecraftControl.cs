using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Rigidbody))]

public class SpacecraftControl : MonoBehaviour {

    public float MaxEnginePower = 40f;
    public float RollEffect = 50f;
    public float PitchEffect = 50f;
    public float YawEffect = 50f;
    public float BankedTurnEffect = 0.5f;
    public float AutoTurnPitch = 0.5f;
    public float AutoRollLevel = 0.1f;
    public float AutoPitchLevel = 0.1f;
    public float AirBrakesEffect = 3f;
    public float ThrottleChangeSpeed = 0.3f;
    public float DragIncreaseFactor = 0.001f;

    private float Throttle;
    private bool AirBrakes;
    private float ForwardSpeed;
    private float EnginePower;
    private float CurrentMaxEnginePower;
    private float RollAngle;
    private float PitchAngle;

    private float RollInput;
    private float PitchInput;
    private float YawInput;
    private float ThrottleInput;
    private float VerticalInput;

    private float OriginalDrag;
    private float OriginalAngularDrag;
    private float AeroFactor=1;
    private bool Immobilized=false;
    private float BankedTurnAmount;
    private Rigidbody ShipRigidBody;
    WheelCollider[] cols;

    void Start()
    {
        ShipRigidBody = GetComponent<Rigidbody> ();

        OriginalDrag = ShipRigidBody.drag;
        OriginalAngularDrag = ShipRigidBody.angularDrag;

        for(int i = 0; i < transform.childCount; i++)
        {
            foreach(var componentsInChild in transform.GetChild(i).GetComponentsInChildren<WheelCollider>())
            {
                componentsInChild.motorTorque = 0.18f;
            }
        }
    }

    public void Move(float rollInput, float pitchInput, float yawInput, float throttleInput, bool airBrakes, float verticalInput)
    {
        this.RollInput = rollInput;
        this.PitchInput = pitchInput;
        this.YawInput = yawInput;
        this.ThrottleInput = throttleInput;
        this.VerticalInput = verticalInput;
        this.AirBrakes = airBrakes;

        ClampInput();
        CalculateRollAndPitchAngles();
        //AutoLevel();
        CalculateForwardSpeed();
        ControlThrottle();
        CalculateDrag();
        CalculateLinearForces();
        CalculateTorque();

        if(Throttle<0.1f)
        {
            Vector3 currentVelocity = ShipRigidBody.velocity;
            Vector3 newVelocity = currentVelocity * Time.deltaTime;
            ShipRigidBody.velocity = currentVelocity - newVelocity;
        }
    }

    void ClampInput()
    {
        RollInput = Mathf.Clamp(RollInput, -1, 1);
        PitchInput = Mathf.Clamp(PitchInput, -1, 1);
        YawInput = Mathf.Clamp(YawInput, -1, 1);
        ThrottleInput = Mathf.Clamp(ThrottleInput, -1, 1);
    }

    void CalculateRollAndPitchAngles()
    {
        Vector3 flatForward = transform.forward;
        flatForward.y = 0;

        if (flatForward.sqrMagnitude > 0)
        {
            flatForward.Normalize();

            Vector3 localFlatForward = transform.InverseTransformDirection(flatForward);
            PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);

            Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward);

            Vector3 localFlatRight = transform.InverseTransformDirection(flatRight);
            RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);

        }
    }

    void AutoLevel()
    {
        BankedTurnAmount = Mathf.Sin(RollAngle);

        if(RollInput == 0f)
        {
            RollInput = -RollAngle * AutoRollLevel;
        }

        if(PitchInput == 0f)
        {
            PitchInput = -PitchInput * AutoPitchLevel;
            PitchInput -= Mathf.Abs(BankedTurnAmount * BankedTurnAmount * AutoTurnPitch);
        }
    }

    void CalculateForwardSpeed()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(ShipRigidBody.velocity);

        ForwardSpeed = Mathf.Max(0, localVelocity.z);
    }

    void ControlThrottle()
    {
        if (Immobilized)
        {
            ThrottleInput = -0.5f;
        }

        Throttle = Mathf.Clamp01(Throttle + ThrottleInput * Time.deltaTime * ThrottleChangeSpeed); 

        EnginePower = Throttle * MaxEnginePower;
    }

    void CalculateDrag()
    {
        float extraDrag = ShipRigidBody.velocity.magnitude * DragIncreaseFactor;
        ShipRigidBody.drag = (AirBrakes ? (OriginalDrag + extraDrag)* AirBrakesEffect : OriginalDrag + extraDrag);

        ShipRigidBody.angularDrag = OriginalAngularDrag * ForwardSpeed / 1000 + OriginalAngularDrag;
    }

    void CalculateLinearForces()
    {
        Vector3 forces = Vector3.zero;

        forces += EnginePower * transform.forward;

        forces += VerticalInput * 50 * transform.up;

        ShipRigidBody.AddForce(forces);

    }

    void CalculateTorque()
    {
        Vector3 torque = Vector3.zero;

        torque += PitchInput * PitchEffect * transform.right;
        torque += YawInput * YawEffect * transform.up;
        torque += -RollInput * RollEffect * transform.forward;
        torque += BankedTurnAmount * BankedTurnEffect * transform.up;
        
        ShipRigidBody.AddTorque(torque * AeroFactor);
    }

    public void Immobilize()
    {
        Immobilized = true;
    }

    public void Reset()
    {
        Immobilized = false;
    }
}
