using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePieces : MonoBehaviour
{
    public static MovePieces instance; // MovePiece 클래스의 싱글톤 인스턴스
    Match3 game; // 게임 로직을 다루는 객체

    NodePiece moving;    // 현재 움직이고 있는 NodePiece
    Point newIndex;      // 새로운 위치 인덱스
    Vector2 mouseStart;  // 마우스를 클릭한 시작 위치

    private void Awake()
    {
        instance = this; // 싱글톤 적용
    }

    void Start()
    {
        game = GetComponent<Match3>();
    }

    void Update()
    {
        if(moving != null) // 움직이고 있는 NodePiece가 있다면
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart); // 마우스 시작 위치와 현재 위치 간 벡터 계산
            Vector2 nDir = dir.normalized; // 방향 벡터 정규화
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y)); // 절대값 적용하여 x, y 이동 거리 비교

            newIndex = Point.clone(moving.index); // 현재 위치 복사
            Point add = Point.zero; // 추가 이동 방향 초기화

            if(dir.magnitude > 32) // 마우스가 시작 위치에서 32픽셀 이상 이동했다면
            {
                // x 방향 이동이 더 크면 좌우이동, y 방향 이동이 더 크면 상하 이동 결정
                if(aDir.x > aDir.y)
                {
                    add = (new Point((nDir.x > 0) ? 1 : -1, 0)); // x 방향 이동 설정
                }
                else if(aDir.y > aDir.x)
                {
                    add = (new Point(0, (nDir.y > 0) ? -1 : 1)); // y 방향 이동 설정
                }
            }
            newIndex.add(add); // 새로운 위치에 이동 방향 추가

            Vector2 pos = game.getPositionFromPoint(moving.index); // 현재 위치의 월드 좌표 가져오기
            if(!newIndex.Equals(moving.index)) // 새로운 위치가 현재 위치와 다르면
            {
                pos += Point.mult(new Point(add.x, -add.y), 16).ToVector(); // 이동 방향에 따른 위치 보정
            }
            moving.MovePositionTo(pos); // 움직이고 있는 조각의 위치 업데이트
        }
    }

    // NodePiece를 선택하여 이동 준비
    public void MovePiece(NodePiece piece)
    {
        if (moving != null || piece.value == -1) return; // 이미 움직이고 있거나, 빈 공간(-1)인 경우 무시
        moving = piece; // 현재 움직이는 조각 설정
        mouseStart = Input.mousePosition; // 마우스 시작 위치 저장
    }

    // NodePiece를 놓았을 때 호출되는 함수
    public void DropPiece()
    {
        if (moving == null) return; // 움직이는 조각이 없으면 리턴

        if (!newIndex.Equals(moving.index)) // 새로운 위치로 이동했으면
        {
            game.FlipPieces(moving.index, newIndex, true); // 두 조각의 위치 교체
        }
        else
        {
            game.ResetPiece(moving); // 원래 위치로 되돌림
        }
        moving = null; // 이동 조각 초기화
    }
}
