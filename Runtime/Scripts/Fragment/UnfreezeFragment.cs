using UnityEngine;
using UnityEngine.Events;

public class UnfreezeFragment : MonoBehaviour
{
    [Tooltip("Options for triggering the fracture")]
    public TriggerOptions triggerOptions;

    [Tooltip("If true, all sibling fragments will be unfrozen if the trigger conditions for this fragment are met.")]
    public bool unfreezeAll = true;

    [Tooltip("This callback is invoked when the fracturing process has been completed.")]
    public UnityEvent onFractureCompleted;

    // True if this fragment has already been unfrozen
    private bool isFrozen = true;
    public bool kinematicAtStart = true;

    public AudioClip debriSound;
    public Vector2 pitchRange = new Vector2(0.8f, 1.2f);
    //Volume of the sound effect scroll bar
    [Range(0, 1)]
    public float volume = 0.6f;

    void OnValidate()
    {
        if (this.kinematicAtStart)
        {
            this.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!this.isFrozen) 
        {
            return;
        }

        if (collision.contactCount > 0)
        {
            // Collision force must exceed the minimum force (F = I / T = F)
            var contact = collision.contacts[0];
            var collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

            // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
            bool colliderTagAllowed = triggerOptions.IsTagAllowed(contact.otherCollider.gameObject.tag);

            // Fragment is unfrozen if the colliding object has the correct tag (if tag filtering is enabled)
            // and the collision force exceeds the minimum collision force.
            if (collisionForce > triggerOptions.minimumCollisionForce &&
                (!triggerOptions.filterCollisionsByTag || colliderTagAllowed))
            {
                this.Unfreeze();
            }
        }
    }
    
    void OnTriggerEnter(Collider collider)
    {
        if (!this.isFrozen) 
        {
            return;
        }

        bool tagAllowed = triggerOptions.IsTagAllowed(collider.gameObject.tag);
        if (!triggerOptions.filterCollisionsByTag || triggerOptions.IsTagAllowed(collider.gameObject.tag))
        {
            this.Unfreeze();
        }
    }

    private void Unfreeze()
    {
        if (this.unfreezeAll)
        {
            foreach(UnfreezeFragment fragment in this.transform.parent.GetComponentsInChildren<UnfreezeFragment>())
            {
                fragment.UnfreezeThis();
            }
        } 
        else 
        {
            UnfreezeThis();
        }

        if (this.onFractureCompleted != null)
        {
            this.onFractureCompleted.Invoke();
        }
    }

    private void UnfreezeThis()
    {
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        this.isFrozen = false;

        if (this.kinematicAtStart)
        {
            this.GetComponent<Rigidbody>().isKinematic = false;
        }   

        if (this.debriSound != null)
        {
            //Play sound clip at the position of the fragment with volume and pitch
            float pitch = Random.Range(pitchRange.x, pitchRange.y);
            //Add AudioSource component to the fragment
            AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
            audioSource.clip = debriSound;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
        }
    }
}
