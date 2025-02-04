using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KilledPiece : MonoBehaviour
{
    // 떨어지는 조각을 위한 변수들
    public bool falling; // 조각이 떨어지고 있는지 여부
    float speed = 16f; // 조각의 속도
    float gravity = 32f; // 중력 값
    Vector2 moveDir; // 이동 방향
    RectTransform rect; // UI 위치 및 크기 조정용 RectTransform
    Image img; // 이미지 컴포넌트

    // 조각 초기화 메서드
    public void Initialize(Sprite piece, Vector2 start)
    {
        falling = true; // 조각이 떨어지는 상태로 설정

        moveDir = Vector2.up; // 처음에는 위쪽 방향으로 이동
        moveDir.x = Random.Range(-1.0f, 1.0f); // 좌우 랜덤한 흔들림 추가
        moveDir *= speed / 2; // 속도 조정

        img = GetComponent<Image>(); // 이미지 컴포넌트 가져오기
        rect = GetComponent<RectTransform>(); // RectTransform 가져오기
        img.sprite = piece; // 스프라이트 설정
        rect.anchoredPosition = start; // 시작 위치 설정
    }

    // 매 프레임마다 호출되는 업데이트 메서드
    void Update()
    {
        if (!falling) return; // 조각이 떨어지는 상태가 아니면 실행하지 않음

        moveDir.y -= Time.deltaTime * gravity; // 중력 적용 (y 방향 속도 감소)
        moveDir.x = Mathf.Lerp(moveDir.x, 0, Time.deltaTime); // 좌우 흔들림이 점점 감소하도록 보간
        rect.anchoredPosition += moveDir * Time.deltaTime * speed; // 위치 업데이트

        // 화면 밖으로 나가면 조각을 제거
        if (rect.position.x < -64f || rect.position.x > Screen.width + 64f || rect.position.y < -64f || rect.position.y > Screen.height + 64f)
            falling = false; // 떨어지는 상태 해제 (즉, 사라짐)
    }
}
