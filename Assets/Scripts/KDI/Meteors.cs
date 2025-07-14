using System.Collections;
using UnityEngine;

public class Meteors : MonoBehaviour
{
    int childCount = 0;
    int currentChild = 0;
    float term = 1;
    private void OnEnable()
    {
        childCount = transform.childCount; //자식으로 있는 운석 개수
        StartCoroutine(MeteorCoroutine());
    }
    IEnumerator MeteorCoroutine()
    {
        while (true)
        {
            transform.GetChild(currentChild).GetChild(0).GetComponent<Animator>().SetTrigger("Fall");
            term = Random.Range(0.5f, 2);
            yield return new WaitForSecondsRealtime(term);
            currentChild = (currentChild + 1) % childCount;
        }
    }
}
