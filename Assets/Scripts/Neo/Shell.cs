using UnityEngine;

public class Shell : MonoBehaviour
{
    public ParticleSystem TrailParticleSystem;
    [HideInInspector] public Warship Warship
    {
        get => _warship;
        set { _warship = value; }
    }

    private Warship _warship;
    private bool m_Hit = false;

    // Start is called before the first frame update
    void Start()
    {
        TrailParticleSystem.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        Vector3 targetPosition = Warship.target.transform.position;
        float distance = (targetPosition - transform.position).magnitude;
        float punishment = m_Hit ? 0f : -distance / 10000f;
        Debug.Log($"Shell({tag}): {transform.position} -> {targetPosition} (punishment: {punishment})");
        Warship.AddReward(punishment);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Shell.OnCollisionEnter: {tag} -> {collision.collider.tag} ({collision.collider.name})");
        if (collision.collider.tag.Equals("Player"))
        {
            m_Hit = true;
            //(collision.collider as Warship).TakeDamage();
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Shell.OnTriggerEnter: {tag} -> {other.tag} ({other.name})");
        if (other.tag.StartsWith("Bullet") || other.name.StartsWith("Dominion"))
        {
            return;
        }
        if (other.tag.Equals("Player"))
        {
            m_Hit = true;
            //(other as Warship).TakeDamage();
        }

        Destroy(gameObject);
    }
}
