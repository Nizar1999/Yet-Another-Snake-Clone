using UnityEngine;

public class HeadController : MonoBehaviour
{
    public event System.Action AteFood;

    [SerializeField] Sprite normalHead;
    [SerializeField] Sprite tongueHead;

    private SpriteRenderer head;

    private void Awake()
    {
        head = GetComponent<SpriteRenderer>();    
    }

    private void Start()
    {
        InvokeRepeating("ReleaseTongue", 1.5f, 1.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "food")
        {
            Destroy(collision.gameObject);
            HatFood();
        } else {
            GameManager.instance.EndGame();
        }
    }

    private void HatFood()
    {
        if (GameManager.instance.state != GameState.Playing)
            return;
        AteFood?.Invoke();
        GameManager.instance.UpdateScore();
    }

    private void ReleaseTongue()
    {
        head.sprite = tongueHead;
        Invoke("HideTongue", .5f);
    }

    private void HideTongue()
    {
        head.sprite = normalHead;
    }
}
