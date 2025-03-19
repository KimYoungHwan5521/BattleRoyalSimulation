using UnityEngine;

public class Blood : CustomObject
{
    [SerializeField] Sprite[] sprites;

    public override void MyStart()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
