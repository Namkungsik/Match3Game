using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value; // ����� �� (��: ���� �Ǵ� ����)
    public Point index; // ���� ����� ��ġ �ε���

    [HideInInspector]
    public Vector2 pos; // ���� ����� ���� ��ǥ

    [HideInInspector]
    public RectTransform rect; // UI ��ġ �� ũ�� ������ RectTransform

    bool updating; // ��尡 ���� �����̰� �ִ��� ����
    Image img; // ����� �̹��� ������Ʈ

    // ��� �ʱ�ȭ �޼���
    public void Initialize(int v, Point p, Sprite piece)
    {
        img = GetComponent<Image>(); // �̹��� ������Ʈ ��������
        rect = GetComponent<RectTransform>(); // RectTransform ��������

        value = v; // ��� �� ����
        SetIndex(p); // ��ġ �ε��� ����
        img.sprite = piece; // �̹��� ��������Ʈ ����
    }

    // ����� ��ġ �ε����� �����ϴ� �޼���
    public void SetIndex(Point p)
    {
        index = p;
        ResetPosition(); // ��ġ�� �ε����� �°� �ʱ�ȭ
        UpdateName(); // ��� �̸� ������Ʈ
    }

    // ��ġ�� �ε����� �°� �ʱ�ȭ�ϴ� �޼���
    public void ResetPosition()
    {
        pos = new Vector2(32 + (64 * index.x), -32 - (64 * index.y)); // �׸��� ���� ��ġ ���
    }

    // ������ ��ġ�� ��带 �̵���Ű�� �޼���
    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 16f; // �ε巯�� �̵� ����
    }

    // ������ ��ġ�� ��带 �ε巴�� �̵���Ű�� �޼���
    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 16f); // Lerp�� ����� �ε巯�� �̵�
    }

    // ������ ��ǥ ��ġ�� �̵� ������ ������Ʈ�ϴ� �޼���
    public bool UpdatePiece()
    {
        if (Vector3.Distance(rect.anchoredPosition, pos) > 1) // ��ǥ ��ġ���� �Ÿ� Ȯ��
        {
            MovePositionTo(pos); // ��ǥ ��ġ�� �̵�
            updating = true; // ������Ʈ ������ ����
            return true;
        }
        else
        {
            rect.anchoredPosition = pos; // ��ǥ ��ġ�� ����
            updating = false; // ������Ʈ �Ϸ�
            return false;
        }
    }

    // ����� �̸��� ��ġ�� �°� ������Ʈ�ϴ� �޼���
    void UpdateName()
    {
        transform.name = "Node [" + index.x + ", " + index.y + "]";
    }

    // ���콺�� Ŭ������ �� ȣ��Ǵ� �޼���
    public void OnPointerDown(PointerEventData eventData)
    {
        if (updating) return; // �̵� ���̸� Ŭ�� ����
        MovePieces.instance.MovePiece(this); // ��� �̵� ����
    }

    // ���콺�� ������ �� ȣ��Ǵ� �޼���
    public void OnPointerUp(PointerEventData eventData)
    {
        MovePieces.instance.DropPiece(); // ��� �̵� ����
    }
}
