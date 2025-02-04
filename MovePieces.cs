using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePieces : MonoBehaviour
{
    public static MovePieces instance; // MovePiece Ŭ������ �̱��� �ν��Ͻ�
    Match3 game; // ���� ������ �ٷ�� ��ü

    NodePiece moving;    // ���� �����̰� �ִ� NodePiece
    Point newIndex;      // ���ο� ��ġ �ε���
    Vector2 mouseStart;  // ���콺�� Ŭ���� ���� ��ġ

    private void Awake()
    {
        instance = this; // �̱��� ����
    }

    void Start()
    {
        game = GetComponent<Match3>();
    }

    void Update()
    {
        if(moving != null) // �����̰� �ִ� NodePiece�� �ִٸ�
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart); // ���콺 ���� ��ġ�� ���� ��ġ �� ���� ���
            Vector2 nDir = dir.normalized; // ���� ���� ����ȭ
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y)); // ���밪 �����Ͽ� x, y �̵� �Ÿ� ��

            newIndex = Point.clone(moving.index); // ���� ��ġ ����
            Point add = Point.zero; // �߰� �̵� ���� �ʱ�ȭ

            if(dir.magnitude > 32) // ���콺�� ���� ��ġ���� 32�ȼ� �̻� �̵��ߴٸ�
            {
                // x ���� �̵��� �� ũ�� �¿��̵�, y ���� �̵��� �� ũ�� ���� �̵� ����
                if(aDir.x > aDir.y)
                {
                    add = (new Point((nDir.x > 0) ? 1 : -1, 0)); // x ���� �̵� ����
                }
                else if(aDir.y > aDir.x)
                {
                    add = (new Point(0, (nDir.y > 0) ? -1 : 1)); // y ���� �̵� ����
                }
            }
            newIndex.add(add); // ���ο� ��ġ�� �̵� ���� �߰�

            Vector2 pos = game.getPositionFromPoint(moving.index); // ���� ��ġ�� ���� ��ǥ ��������
            if(!newIndex.Equals(moving.index)) // ���ο� ��ġ�� ���� ��ġ�� �ٸ���
            {
                pos += Point.mult(new Point(add.x, -add.y), 16).ToVector(); // �̵� ���⿡ ���� ��ġ ����
            }
            moving.MovePositionTo(pos); // �����̰� �ִ� ������ ��ġ ������Ʈ
        }
    }

    // NodePiece�� �����Ͽ� �̵� �غ�
    public void MovePiece(NodePiece piece)
    {
        if (moving != null || piece.value == -1) return; // �̹� �����̰� �ְų�, �� ����(-1)�� ��� ����
        moving = piece; // ���� �����̴� ���� ����
        mouseStart = Input.mousePosition; // ���콺 ���� ��ġ ����
    }

    // NodePiece�� ������ �� ȣ��Ǵ� �Լ�
    public void DropPiece()
    {
        if (moving == null) return; // �����̴� ������ ������ ����

        if (!newIndex.Equals(moving.index)) // ���ο� ��ġ�� �̵�������
        {
            game.FlipPieces(moving.index, newIndex, true); // �� ������ ��ġ ��ü
        }
        else
        {
            game.ResetPiece(moving); // ���� ��ġ�� �ǵ���
        }
        moving = null; // �̵� ���� �ʱ�ȭ
    }
}
