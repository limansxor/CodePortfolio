using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSpriteAnimation : MonoBehaviour
{
    public float delayTime = 0.1f;
    private float sumTime = 0;
    private int index = 0;
    public Sprite[] sprites;
    // Start is called before the first frame update
    private void Start()
    {
        
    }
    private void Update()
    {
        if(sumTime> delayTime)
        {
            GetComponent<Image>().sprite = sprites[index];

            if(sprites.Length ==++index)
            {
                index = 0;
            }
            sumTime = 0;
        }
        sumTime += Time.deltaTime;
    }


}
