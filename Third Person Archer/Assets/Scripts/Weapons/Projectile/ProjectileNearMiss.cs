using UnityEngine;

/// <summary>
/// An enemy projectile that just misses the player whizzes past the ear: a short sound
/// when it leaves the closest point of its pass, so a miss reads as a near one.
/// </summary>
public class ProjectileNearMiss : MonoBehaviour
{
    [SerializeField] private Projectile _projectile;
    [Tooltip("Pass closer than this to the camera (m) to be heard.")]
    [SerializeField] private float _radius = 3f;
    [SerializeField] private AudioClip _clip;
    [SerializeField, Range(0f, 1f)] private float _volume = 0.7f;
    [SerializeField] private Vector2 _pitch = new Vector2(1.1f, 1.35f);

    private Transform _listener;
    private float _closest = float.MaxValue;
    private bool _done;

    private void Awake()
    {
        if (_projectile == null)
            _projectile = GetComponentInParent<Projectile>();

        if (_projectile != null)
            _projectile.OnHited.AddListener(Finish);
    }

    private void OnDestroy()
    {
        if (_projectile != null)
            _projectile.OnHited.RemoveListener(Finish);
    }

    private void Finish() => _done = true;

    private void Update()
    {
        if (_done || _clip == null || _projectile == null || !_projectile.IsShooted)
            return;

        if (_listener == null)
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;
            _listener = camera.transform;
        }

        float distance = Vector3.Distance(transform.position, _listener.position);
        if (distance < _closest)
        {
            _closest = distance;
            return;
        }

        // Moving away again: the pass is over.
        if (_closest <= _radius)
            Play(_closest);

        _done = true;
    }

    private void Play(float closest)
    {
        var go = new GameObject("Near Miss Whiz");
        go.transform.position = transform.position;

        var source = go.AddComponent<AudioSource>();
        source.clip = _clip;
        source.volume = _volume * Mathf.Lerp(1f, 0.55f, closest / _radius);
        source.pitch = Random.Range(_pitch.x, _pitch.y);
        source.spatialBlend = 0.6f;
        source.dopplerLevel = 0f;
        source.Play();

        Destroy(go, _clip.length / Mathf.Max(0.1f, source.pitch) + 0.1f);
    }
}
