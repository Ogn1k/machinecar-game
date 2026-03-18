using System;
using Unity.Mathematics;
using UnityEngine;

public class CarController3 : MonoBehaviour
{
    public GameObject smokePrefab;
    public Rigidbody playerRB;
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;
    public float gasInput;
    public float brakeInput;
    public float steeringInput;
    public float motorPower;
    public float brakePower;
    public float slipAngle;
    private float speed;
    public float slipAllowance;
    public bool debugParticles = false;
    public AnimationCurve steeringCurve;
    public bool inLoop = false;
    public Vector3 loopNormal;
    WheelHit[] wheelHits = new WheelHit[4];

    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        InstantiateSmoke();
    }

    void InstantiateSmoke()
    {
        wheelParticles.FRWheel = Instantiate(smokePrefab, colliders.FRWheel.transform.position-Vector3.up*colliders.FRWheel.radius, smokePrefab.transform.rotation, colliders.FRWheel.transform)
        .GetComponent<ParticleSystem>();
        wheelParticles.FLWheel = Instantiate(smokePrefab, colliders.FLWheel.transform.position-Vector3.up*colliders.FLWheel.radius + Vector3.left/2, smokePrefab.transform.rotation, colliders.FLWheel.transform)
        .GetComponent<ParticleSystem>();
        wheelParticles.BRWheel = Instantiate(smokePrefab, colliders.BRWheel.transform.position-Vector3.up*colliders.BRWheel.radius, smokePrefab.transform.rotation, colliders.BRWheel.transform)
        .GetComponent<ParticleSystem>();
        wheelParticles.BLWheel = Instantiate(smokePrefab, colliders.BLWheel.transform.position-Vector3.up*colliders.BLWheel.radius + Vector3.left/2, smokePrefab.transform.rotation, colliders.BLWheel.transform)
        .GetComponent<ParticleSystem>();
    }
    void Update()
    {
        speed = playerRB.linearVelocity.magnitude;
        CheckInput();
    }
    void FixedUpdate()
    {
        ApplyLoopForce();
        AirControl();
        ApplyDownforce();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        CheckParticles();
        IsGrounded();
        StabilizeLanding();
        ApplyWheelsPosition();
    }

    public float GetSpeed()
    {
        return speed;
    }

    void CheckInput()
    {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        Vector3 vel = playerRB.linearVelocity;
        float forwardVel = Vector3.Dot(transform.forward, vel);

        if (vel.sqrMagnitude > 0.0001f)
            slipAngle = Vector3.Angle(transform.forward, vel);
        else
            slipAngle = 0f;

        if (slipAngle < 120f)
        {
            if(gasInput < 0)
            {
                if (forwardVel > 0.5f)
                {
                    brakeInput = Mathf.Abs(gasInput);
                    gasInput = 0f;
                }
                else
                {
                    // allow reverse torque when car is nearly stopped or moving backwards
                    brakeInput = 0f;
                }
            }
            else
            {
                brakeInput=0; 
            }
        }
        else
        {
            brakeInput=0; 
        }
    }
    void ApplyBrake()
    {
        colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.BRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        colliders.BLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
    }

    void ApplyMotor()
    {
        colliders.BRWheel.motorTorque = motorPower*gasInput;
        colliders.BLWheel.motorTorque = motorPower*gasInput;
    }

    void ApplySteering()
    {
        float baseAngle = steeringInput * steeringCurve.Evaluate(speed);
        Vector3 vel = playerRB.linearVelocity;
        float steeringAngle = baseAngle;

        //float velAngle = 0f;
        if (vel.sqrMagnitude > 0.0001f)
        {
            float forwardVel = Vector3.Dot(transform.forward, vel);
            Vector3 travelDir = (forwardVel >= 0f) ? vel.normalized : -vel.normalized; // use travel direction
            float velAngle = Vector3.SignedAngle(transform.forward, travelDir, Vector3.up);

            // reduce velocity correction when reversing
            float correctionFactor = (forwardVel >= 0f) ? 0.2f : 0.08f;
            steeringAngle = baseAngle + velAngle * correctionFactor;
        }
        // small velocity-based correction, then clamp to realistic steering range
        steeringAngle = Mathf.Clamp(steeringAngle, -30f, 30f);

        if (speed* 3.6f > 60)
            steeringAngle *= 0.5f;

        if (speed* 3.6f > 120)
            steeringAngle *= 0.3f;

        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }

    void ApplyWheelsPosition()
    {
        UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheel(colliders.BRWheel, wheelMeshes.BRWheel);
        UpdateWheel(colliders.BLWheel, wheelMeshes.BLWheel);
    }
    void ApplyDownforce()
    {
        if(IsGrounded() && !inLoop)
        {
            //print("Applying downforce");
            playerRB.AddForce(-transform.up *  speed * 50f);
        }
    }
    void ApplyLoopForce()
    {

        float stiffness = inLoop ? 0.35f : 2f;

        WheelFrictionCurve f = colliders.FRWheel.forwardFriction;
        f.stiffness = stiffness;

        colliders.FRWheel.forwardFriction = f;
        colliders.FLWheel.forwardFriction = f;
        colliders.BRWheel.forwardFriction = f;
        colliders.BLWheel.forwardFriction = f;

        if (inLoop)
        {
            playerRB.useGravity = false;
            float speedFactor = Mathf.Clamp(speed, 20f, 120f);

            float gravity = 28f;

            // прижимаем к дороге
            playerRB.AddForce(-loopNormal * gravity, ForceMode.Acceleration);

            Quaternion targetRotation =
            Quaternion.FromToRotation(transform.up, loopNormal) * playerRB.rotation;

            playerRB.MoveRotation(Quaternion.Slerp(
                playerRB.rotation,
                targetRotation,
                Time.fixedDeltaTime * 6f
            ));

            

        }
        else
            playerRB.useGravity = true;

    }
    void CheckParticles()
    {
        colliders.FRWheel.GetGroundHit(out wheelHits[0]);
        colliders.FLWheel.GetGroundHit(out wheelHits[1]);
        colliders.BRWheel.GetGroundHit(out wheelHits[2]);
        colliders.BLWheel.GetGroundHit(out wheelHits[3]);

        WheelParticleUse(wheelParticles.FRWheel, wheelHits[0]);
        WheelParticleUse(wheelParticles.FLWheel, wheelHits[1]);
        WheelParticleUse(wheelParticles.BRWheel, wheelHits[2]);
        WheelParticleUse(wheelParticles.BLWheel, wheelHits[3]);
    }

    bool IsGrounded()
    {
        colliders.FRWheel.GetGroundHit(out wheelHits[0]);
        colliders.FLWheel.GetGroundHit(out wheelHits[1]);
        colliders.BRWheel.GetGroundHit(out wheelHits[2]);
        colliders.BLWheel.GetGroundHit(out wheelHits[3]);

        return wheelHits[0].collider != null ||
           wheelHits[1].collider != null ||
           wheelHits[2].collider != null ||
           wheelHits[3].collider != null;
    }

    void WheelParticleUse(ParticleSystem wheel, WheelHit wheelHit)
    {
        if (wheel == null)
            return;

        // WheelHit.collider is null when the wheel is not contacting anything
        if (wheelHit.collider == null)
        {
            if (wheel.isPlaying)
                wheel.Stop();
            return;
        }

        float slip = Mathf.Abs(wheelHit.sidewaysSlip) + Mathf.Abs(wheelHit.forwardSlip);
        if (debugParticles)
            Debug.Log($"Wheel slip: {slip:F3} (side:{wheelHit.sidewaysSlip:F3} forward:{wheelHit.forwardSlip:F3})");

        if (slip > slipAllowance)
        {
            if (!wheel.isPlaying)
                wheel.Play();
        }
        else
        {
            if (wheel.isPlaying)
                wheel.Stop();
        }
    }
    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }

    void AirControl()
    {
        if(IsGrounded()) return;
        float airTorque = 3.5f;
        playerRB.AddTorque(transform.right * gasInput * airTorque, ForceMode.Acceleration);
        playerRB.AddTorque(-transform.forward * steeringInput * airTorque, ForceMode.Acceleration);
    }
    void StabilizeLanding()
    {
        if (IsGrounded())
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Quaternion targetRot = Quaternion.LookRotation(flatForward, Vector3.up);

            playerRB.MoveRotation(Quaternion.Slerp(
                playerRB.rotation,
                targetRot,
                Time.fixedDeltaTime * 2.5f
            ));
        }
    }
}
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider BRWheel;
    public WheelCollider BLWheel;
}
[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer BRWheel;
    public MeshRenderer BLWheel;
}
[System.Serializable]
public class WheelParticles
{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem BRWheel;
    public ParticleSystem BLWheel;
}