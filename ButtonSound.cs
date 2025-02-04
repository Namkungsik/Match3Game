using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioClip buttonClickSound; // ��ư Ŭ�� ȿ����
    private AudioSource audioSource;  // AudioSource ������Ʈ

    void Start()
    {
        // AudioSource ������Ʈ �������� �Ǵ� �߰�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayButtonSound()
    {
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
