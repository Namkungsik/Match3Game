using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KilledPiece : MonoBehaviour
{
    // �������� ������ ���� ������
    public bool falling; // ������ �������� �ִ��� ����
    float speed = 16f; // ������ �ӵ�
    float gravity = 32f; // �߷� ��
    Vector2 moveDir; // �̵� ����
    RectTransform rect; // UI ��ġ �� ũ�� ������ RectTransform
    Image img; // �̹��� ������Ʈ

    // ���� �ʱ�ȭ �޼���
    public void Initialize(Sprite piece, Vector2 start)
    {
        falling = true; // ������ �������� ���·� ����

        moveDir = Vector2.up; // ó������ ���� �������� �̵�
        moveDir.x = Random.Range(-1.0f, 1.0f); // �¿� ������ ��鸲 �߰�
        moveDir *= speed / 2; // �ӵ� ����

        img = GetComponent<Image>(); // �̹��� ������Ʈ ��������
        rect = GetComponent<RectTransform>(); // RectTransform ��������
        img.sprite = piece; // ��������Ʈ ����
        rect.anchoredPosition = start; // ���� ��ġ ����
    }

    // �� �����Ӹ��� ȣ��Ǵ� ������Ʈ �޼���
    void Update()
    {
        if (!falling) return; // ������ �������� ���°� �ƴϸ� �������� ����

        moveDir.y -= Time.deltaTime * gravity; // �߷� ���� (y ���� �ӵ� ����)
        moveDir.x = Mathf.Lerp(moveDir.x, 0, Time.deltaTime); // �¿� ��鸲�� ���� �����ϵ��� ����
        rect.anchoredPosition += moveDir * Time.deltaTime * speed; // ��ġ ������Ʈ

        // ȭ�� ������ ������ ������ ����
        if (rect.position.x < -64f || rect.position.x > Screen.width + 64f || rect.position.y < -64f || rect.position.y > Screen.height + 64f)
            falling = false; // �������� ���� ���� (��, �����)
    }
}
