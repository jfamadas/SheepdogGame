using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeRun : MonoBehaviour
{

    void Update()
    {
        if  (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float fadeTime = 2;
        float startAlpha = GetComponent<Text>().color.a;
        float rate = 1.0f / fadeTime;
        float progress = 0.0f;

        while (progress < 1.0)
        {
            Color tmpColor = GetComponent<Text>().color;

            GetComponent<Text>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));

            progress += rate * Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
        
    }

}
