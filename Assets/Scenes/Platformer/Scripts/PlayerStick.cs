using UnityEngine;

public class PlayerStick : MonoBehaviour
{

    // components
    private BoxCollider2D box;

    private void Start()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(transform); // set this object as parent

            if(collision.collider.transform.localPosition.y < box.bounds.extents.y) // checks if player is on top of moving platform so parentage is only set for top collisions
            {
                collision.collider.transform.SetParent(null); // un-set this object as parent if player is not above 
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null); // un-set this object as parent
        }
    }
}
