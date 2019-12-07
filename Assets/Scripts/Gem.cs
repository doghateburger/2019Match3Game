using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    private SpriteRenderer sprite;
    [SerializeField] float reduceRate = 2f;
    Color color;
    private bool toDisable = false;

    private void Awake()
    {
        //get the default colour of the sprite
        color = GetComponent<SpriteRenderer>().color;
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (toDisable)
        {
            sprite.color -= new Color(0, 0, 0, reduceRate * Time.deltaTime);

            if (sprite.color.a < 0f)
            {
                transform.parent = null;
                gameObject.SetActive(false);
            }
        }
    }

    public void DisableObj()
    {
        toDisable = true;
    }

    private void OnEnable()
    {
        //Whenever the object is re enable, it'll reset it's settings
        toDisable = false;
        sprite.color = color;
    }
}
