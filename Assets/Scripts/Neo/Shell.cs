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
        if (m_Hit)
        {
            return;
        }

        Vector3 position3D = transform.position;
        Warship target = Warship.Target;
        if (target != null)
        {
            Vector3 targetPosition3D = Warship.Target.transform.position;
            Vector2 position = new Vector2(position3D.x, position3D.z);
            Vector2 targetPosition = new Vector2(targetPosition3D.x, targetPosition3D.z);
            float distance = Vector2.Distance(position, targetPosition);
            float encouragement = 1 / (1 + Mathf.Pow(distance, 2f));
            // Debug.Log($"Shell({tag}): {transform.position} -> {targetPosition} (encouragement: {encouragement:F8})");
            //Warship.AddReward(encouragement);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"Shell.OnCollisionEnter: {tag} -> {collision.collider.tag} ({collision.collider.name})");
        if (collision.collider.tag.Equals("Player"))
        {
            m_Hit = true;
            /*
            //(collision.collider as Warship).TakeDamage();
            GameObject obj = GameObject.Find(collision.collider.name); // collision.collider as Warship;
            Warship warship = obj.GetComponent<Warship>();
            Debug.Log($"Shell.OnCollisionEnter: {tag} -> {collision.collider.name}(TeamId={warship.TeamId})");
            if (!tag.EndsWith(warship.TeamId.ToString()))
            {
                warship.OnDamageTaken();
            }
            */
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Shell.OnTriggerEnter: {tag} -> {other.tag} ({other.name})");
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
