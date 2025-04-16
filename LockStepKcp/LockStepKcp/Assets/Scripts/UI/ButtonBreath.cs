using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBreath : MonoBehaviour
{
    public float moveDistance = 10f; // �ƶ��ľ���
    public float speed = 2f; // �����ٶ�

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
        StartCoroutine(BreathingAnimation());
    }

    private IEnumerator BreathingAnimation()
    {
        while (true)
        {
            // �����ƶ�
            float elapsedTime = 0f;
            Vector3 targetPosition = originalPosition + new Vector3(0, moveDistance, 0);

            while (elapsedTime < speed)
            {
                transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / speed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // �����ƶ�
            elapsedTime = 0f;

            while (elapsedTime < speed)
            {
                transform.position = Vector3.Lerp(targetPosition, originalPosition, (elapsedTime / speed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}