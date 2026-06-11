using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FunnyScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image funnyImage;

    [Header("Sprites")]
    [SerializeField] private List<Sprite> funnySprites = new();

    [Header("SFX")]
    [SerializeField] private AudioSource funnyAudioSource;
    [SerializeField]private List<AudioClip> funnySfx = new();

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.25f;
    [SerializeField] private float visibleTime = 1f;
    [SerializeField] private float fadeOutTime = 0.25f;

    Coroutine currentFunny;

    private void Awake()
    {
        funnyImage.gameObject.SetActive(false);
    }

    public void PlayFunny()
    {
        if (funnyAudioSource != null && funnySfx.Count > 0)
        {
            funnyAudioSource.PlayOneShot(
                funnySfx[Random.Range(0, funnySfx.Count)]
            );
        }

        if (funnySprites.Count == 0 || funnyImage == null)
            return;

        if (currentFunny != null)
            StopCoroutine(currentFunny);

        currentFunny = StartCoroutine(PlayFunnyRoutine());
    }

    IEnumerator PlayFunnyRoutine()
    {
        funnyImage.sprite = funnySprites[Random.Range(0, funnySprites.Count)];

        Color color = funnyImage.color;
        color.a = 0;
        funnyImage.color = color;

        funnyImage.gameObject.SetActive(true);

        // Fade In
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            funnyImage.color = color;

            yield return null;
        }

        color.a = 1f;
        funnyImage.color = color;

        // Wait
        yield return new WaitForSeconds(visibleTime);

        // Fade Out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
            funnyImage.color = color;

            yield return null;
        }

        color.a = 0f;
        funnyImage.color = color;

        funnyImage.gameObject.SetActive(false);

        currentFunny = null;
    }
}