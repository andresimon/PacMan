using UnityEngine;

public class BonusItem : MonoBehaviour
{
    public BonusItemData data;

    float randomLifeExpectancy;

    protected GameObject pacMan;

    void Start()
    {
        this.name = "BonusItem";

        randomLifeExpectancy = Random.Range(9f, 10f);

        pacMan = GameManager.instance.pacMan;
    }

    void Update()
    {
        Destroy(gameObject, randomLifeExpectancy);

        CheckCollision();
    }

    void CheckCollision()
    {
        Rect itemRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);

        if (itemRect.Overlaps(pacManRect))
        {
            pacMan.GetComponent<PacMan>().ConsumeBonusItem(gameObject);
        }
    }
}
