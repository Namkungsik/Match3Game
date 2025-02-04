using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value; // 노드의 값 (예: 색상 또는 종류)
    public Point index; // 현재 노드의 위치 인덱스

    [HideInInspector]
    public Vector2 pos; // 현재 노드의 월드 좌표

    [HideInInspector]
    public RectTransform rect; // UI 위치 및 크기 조정용 RectTransform

    bool updating; // 노드가 현재 움직이고 있는지 여부
    Image img; // 노드의 이미지 컴포넌트

    // 노드 초기화 메서드
    public void Initialize(int v, Point p, Sprite piece)
    {
        img = GetComponent<Image>(); // 이미지 컴포넌트 가져오기
        rect = GetComponent<RectTransform>(); // RectTransform 가져오기

        value = v; // 노드 값 설정
        SetIndex(p); // 위치 인덱스 설정
        img.sprite = piece; // 이미지 스프라이트 설정
    }

    // 노드의 위치 인덱스를 설정하는 메서드
    public void SetIndex(Point p)
    {
        index = p;
        ResetPosition(); // 위치를 인덱스에 맞게 초기화
        UpdateName(); // 노드 이름 업데이트
    }

    // 위치를 인덱스에 맞게 초기화하는 메서드
    public void ResetPosition()
    {
        pos = new Vector2(32 + (64 * index.x), -32 - (64 * index.y)); // 그리드 기준 위치 계산
    }

    // 지정된 위치로 노드를 이동시키는 메서드
    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 16f; // 부드러운 이동 적용
    }

    // 지정된 위치로 노드를 부드럽게 이동시키는 메서드
    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 16f); // Lerp를 사용한 부드러운 이동
    }

    // 조각이 목표 위치로 이동 중인지 업데이트하는 메서드
    public bool UpdatePiece()
    {
        if (Vector3.Distance(rect.anchoredPosition, pos) > 1) // 목표 위치와의 거리 확인
        {
            MovePositionTo(pos); // 목표 위치로 이동
            updating = true; // 업데이트 중으로 설정
            return true;
        }
        else
        {
            rect.anchoredPosition = pos; // 목표 위치에 정렬
            updating = false; // 업데이트 완료
            return false;
        }
    }

    // 노드의 이름을 위치에 맞게 업데이트하는 메서드
    void UpdateName()
    {
        transform.name = "Node [" + index.x + ", " + index.y + "]";
    }

    // 마우스를 클릭했을 때 호출되는 메서드
    public void OnPointerDown(PointerEventData eventData)
    {
        if (updating) return; // 이동 중이면 클릭 무시
        MovePieces.instance.MovePiece(this); // 노드 이동 시작
    }

    // 마우스를 놓았을 때 호출되는 메서드
    public void OnPointerUp(PointerEventData eventData)
    {
        MovePieces.instance.DropPiece(); // 노드 이동 종료
    }
}
