using UnityEngine;

/// <summary>
/// Procedural flinch for humanoid enemies: a hit throws the spine and head away from the projectile on a spring,
/// on top of whatever the animator plays, so every enemy reacts the same way whatever weapon it holds.
/// A headshot snaps the head back harder. After the first hit the body keeps a slight lean towards the wound.
/// <see cref="Startle"/> is the same jolt, smaller, away from a noise, with the head and chest turning to it for a moment.
/// Lives on the model next to the Animator; does nothing once the animator is off (ragdoll).
/// </summary>
[DefaultExecutionOrder(10000)]
public class HitFlinch : MonoBehaviour
{
    [SerializeField] private UnityEngine.Animator _animator;

    [Header("Kick (degrees at peak)")]
    [SerializeField] private float _bodyKick = 16f;
    [SerializeField] private float _headKick = 28f;
    [Tooltip("Twist (degrees) when the hit lands off the body's centre line.")]
    [SerializeField] private float _twist = 10f;

    [Header("Spring")]
    [SerializeField, Min(0.1f)] private float _frequency = 3.5f;
    [SerializeField, Range(0.05f, 1f)] private float _damping = 0.4f;

    [Header("Wounded")]
    [Tooltip("Lean (degrees) the body keeps towards the wound after the first hit that didn't kill.")]
    [SerializeField] private float _woundedLean = 7f;
    [SerializeField, Min(0.01f)] private float _woundedBlendTime = 0.6f;

    [Header("Startle")]
    [SerializeField] private float _startleKick = 9f;
    [Tooltip("Furthest the upper body turns (degrees) to look at a noise.")]
    [SerializeField] private float _maxLook = 70f;
    [SerializeField, Min(0.01f)] private float _lookTime = 0.15f;

    // Spine chain from the hips up and how much of the body kick each bone takes; the total is about 1.
    private static readonly (HumanBodyBones Bone, float Body, float Head)[] Chain =
    {
        (HumanBodyBones.Spine, 0.25f, 0f),
        (HumanBodyBones.Chest, 0.3f, 0.05f),
        (HumanBodyBones.UpperChest, 0.2f, 0.05f),
        (HumanBodyBones.Neck, 0.1f, 0.3f),
        (HumanBodyBones.Head, 0.15f, 0.6f),
    };

    // Share of a look turn each bone of the chain takes; the total is 1.
    private static readonly float[] LookWeights = { 0.1f, 0.2f, 0.1f, 0.25f, 0.35f };

    private Transform[] _bones;
    private Vector3 _body;
    private Vector3 _bodyVelocity;
    private Vector3 _head;
    private Vector3 _headVelocity;
    private Vector3 _woundAxis;
    private float _wounded;
    private bool _isWounded;
    private float _look;
    private float _lookVelocity;
    private float _lookTarget;
    private float _lookUntil;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<UnityEngine.Animator>();

        _bones = new Transform[Chain.Length];
        if (_animator != null && _animator.isHuman)
            for (int i = 0; i < Chain.Length; i++)
                _bones[i] = _animator.GetBoneTransform(Chain[i].Bone);
    }

    /// <param name="hitPoint">Where the projectile struck.</param>
    /// <param name="direction">Projectile travel direction.</param>
    /// <param name="critical">Head hit.</param>
    public void Flinch(Vector3 hitPoint, Vector3 direction, bool critical)
    {
        Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
        if (flat.sqrMagnitude < 0.0001f)
            return;
        flat.Normalize();

        // Rotating "up" towards the travel direction: the body bends away from the shooter.
        Vector3 axis = Vector3.Cross(Vector3.up, flat);

        // A hit to one side of the centre line also turns the body.
        Transform centre = _bones[1] != null ? _bones[1] : transform;
        float side = Vector3.Dot(hitPoint - centre.position, axis);
        float twist = Mathf.Clamp(side / 0.25f, -1f, 1f) * _twist;

        float omega = 2f * Mathf.PI * _frequency;
        _bodyVelocity += (axis * (critical ? _bodyKick * 0.5f : _bodyKick) - Vector3.up * twist) * omega;
        if (critical)
            _headVelocity += axis * (_headKick * omega);

        if (!_isWounded)
        {
            _isWounded = true;
            _woundAxis = axis;
        }
    }

    /// <param name="source">Where the noise came from.</param>
    /// <param name="hold">Seconds the upper body keeps looking at it.</param>
    /// <param name="strength">Scales the jolt and the turn, e.g. lower while the enemy is busy aiming.</param>
    public void Startle(Vector3 source, float hold, float strength = 1f)
    {
        Vector3 away = Vector3.ProjectOnPlane(transform.position - source, Vector3.up);
        if (away.sqrMagnitude < 0.0001f)
            return;
        away.Normalize();

        float omega = 2f * Mathf.PI * _frequency;
        _bodyVelocity += Vector3.Cross(Vector3.up, away) * (_startleKick * strength * omega);

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        float maxLook = _maxLook * strength;
        _lookTarget = Mathf.Clamp(Vector3.SignedAngle(forward, -away, Vector3.up), -maxLook, maxLook);
        _lookUntil = Time.time + hold;
    }

    private void LateUpdate()
    {
        if (_animator == null || !_animator.enabled)
            return;

        float dt = Time.deltaTime;
        Step(ref _body, ref _bodyVelocity, dt);
        Step(ref _head, ref _headVelocity, dt);

        if (Time.time > _lookUntil)
            _lookTarget = 0f;
        _look = Mathf.SmoothDamp(_look, _lookTarget, ref _lookVelocity, _lookTime, Mathf.Infinity, Mathf.Min(dt, 0.1f));

        if (_isWounded)
            _wounded = Mathf.MoveTowards(_wounded, 1f, dt / _woundedBlendTime);

        Vector3 lean = _woundAxis * (_woundedLean * _wounded);

        for (int i = 0; i < _bones.Length; i++)
        {
            Transform bone = _bones[i];
            if (bone == null)
                continue;

            Vector3 rotation = (_body + lean) * Chain[i].Body + _head * Chain[i].Head + Vector3.up * (_look * LookWeights[i]);
            float angle = rotation.magnitude;
            if (angle > 0.01f)
                bone.rotation = Quaternion.AngleAxis(angle, rotation / angle) * bone.rotation;
        }
    }

    private void Step(ref Vector3 value, ref Vector3 velocity, float dt)
    {
        // Small fixed sub-steps: a stiff spring blows up on a long frame (hitch, editor stall).
        const float MaxStep = 1f / 120f;
        dt = Mathf.Min(dt, 0.1f);
        int steps = Mathf.CeilToInt(dt / MaxStep);
        float h = dt / Mathf.Max(steps, 1);
        float omega = 2f * Mathf.PI * _frequency;

        for (int i = 0; i < steps; i++)
        {
            velocity += (-omega * omega * value - 2f * _damping * omega * velocity) * h;
            value += velocity * h;
        }
    }
}
