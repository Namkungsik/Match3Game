using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    
    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;
    public RectTransform killedBoard;
    public Text scoreText;
    public Text blockText;

    [Header("Special Sprites")]
    public Sprite blockedSpaceSprite;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;

    [Header("Timer Settings")]
    public Text timerText; // 타이머를 표시할 UI 텍스트
    public Text obstacleTimerText;
    public float gameDuration = 10f; // 게임 제한 시간 (초 단위)

    [Header("Audio Settings")]
    public AudioClip moveSound; // 퍼즐 이동 효과음
    public AudioClip matchSound; // 퍼즐 매칭 효과음
    private AudioSource audioSource;

    public GameObject ResultPanel;

    private float obstacleSpawnTimer = 10f;
    private float remainingTime;
    private bool isGameActive = true;
    public Text FinalScoreText;

    int width = 9;
    int height = 14;
    int curScore = 0;
    int obstacleBlock = 0;
    int[] fills;
    Node[,] board;

    List<NodePiece> update;
    List<FlippedPieces> flipped;
    List<NodePiece> dead;
    List<KilledPiece> killed;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameActive)
        {
            // 타이머 업데이트
            remainingTime -= Time.deltaTime;
            obstacleSpawnTimer -= Time.deltaTime;
            UpdateTimerUI();

            if(obstacleSpawnTimer <= 0f)
            {
                SpawnObstacle();
                obstacleSpawnTimer = 10f;
            }

            if (remainingTime <= 0)
            {
                GameOver();
                return;
            }
        }

        // 업데이트가 완료된 NodePiece 목록을 저장할 리스트 생성
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        for(int i = 0; i < update.Count; i++) // 현재 업데이트 리스트에 있는 모든 NodePiece를 검사
        {
            NodePiece piece = update[i];
            // NodePiece가 업데이트를 완료했는지 확인
            if (!piece.UpdatePiece()) finishedUpdating.Add(piece); // 완료 됐다면 노드 추가
        }
        // 업데이트가 완료된 노드들을 처리
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            // 블록을 움직였는지 확인
            FlippedPieces flip = getFlipped(piece);
            NodePiece flippedPiece = null;
            // 현재 노드의 x좌표를 가져온 뒤 해당열에서 블록이 하나 줄었음을 반영
            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            // 현재 노드에서 연결된 블록들을 찾음
            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null); // 움직인 상태인지

            if (wasFlipped) // 만약 블록을 움직여서 업데이트 한 경우
            {
                flippedPiece = flip.getOtherPiece(piece); // 다른 블록을 가져옴
                // 뒤집힌 블록의 연결된 블록도 확인하여 추가
                AddPoints(ref connected, isConnected(flippedPiece.index, true));
            }

            if(connected.Count == 0) // 매치가 발생하지 않았다면
            {
                if (wasFlipped && flippedPiece != null) // 블록을 움직였던 경우
                {
                    FlipPieces(piece.index, flippedPiece.index, false); // 움직였던 블록을 다시 되돌림
                }
            }
            else // 매치가 발생했을 경우
            {
                PlaySound(matchSound);

                // 매치된 블록 수에 따라 점수 추가
                int matchScroe = connected.Count * 10;
                curScore += matchScroe;
                scoreText.text = "" + curScore;

                // 남은 시간을 추가
                if (remainingTime >= 30f)
                    remainingTime = 30f;
                remainingTime += 1f;

                foreach (Point pnt in connected) // 연결된 모든 블럭 삭제
                {
                    KillPiece(pnt); // 해당 위치의 블록 제거
                    // 해당 위치의 Node 정보를 가져와서 블록 제거
                    Node node = getNodeAtPoint(pnt);
                    NodePiece nodePiece = node.getPiece();
                    if(nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false); // 블록 비활성화
                        dead.Add(nodePiece); // 리스트에 추가
                    }
                    node.SetPiece(null); // 해당 노드의 블록 제거
                }

                ApplyGravityToBoard();
            }
            flipped.Remove(flip); // 움직임이 처리된 정보를 리스트에서 제거
            update.Remove(piece); // 업데이트 리스트에서도 해당 블록 제거
        }
    }

    // 보드의 빈 공간을 채우는 함수
    void ApplyGravityToBoard()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = (height-1); y >= 0; y--)
            {
                Point p = new Point(x, y);
                Node node = getNodeAtPoint(p);
                int val = getValueAtPoint(p);
                if (val != 0) continue; // 현재 위치가 빈공간이 아니라면
                // 위쪽에서 블록을 찾아 빈 공간을 채움
                for(int ny = (y-1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = getValueAtPoint(next);
                    if (nextVal == 0)
                        continue;
                    if(nextVal!= -1) // 위쪽에 있는 블럭이 Blank라면 다시 탐색, hole이 아니라면 유효한 블록
                    {
                        Node got = getNodeAtPoint(next);
                        NodePiece piece = got.getPiece();

                        // 현재 빈 공간에 블록을 설정
                        node.SetPiece(piece);
                        update.Add(piece); // 업데이트 리스트에 추가

                        // 원래 위치를 null로 설정
                        got.SetPiece(null);
                    }
                    else // 만약 보드 끝이라면, 새로운 블록 생성
                    {
                        int newVal = fillPiece(); // 새로운 블록 값 설정
                        NodePiece piece;
                        Point fallPnt = new Point(x, (-1 - fills[x])); // 새 블록이 떨어지는 시작 위치
                        if(dead.Count > 0) // 만약 비활성화된(dead) 블록이 있다면 재사용
                        {
                            NodePiece revived = dead[0];
                            revived.gameObject.SetActive(true); // 다시 활성화
                            piece = revived;

                            dead.RemoveAt(0); // 사용한 블록 제거
                        }
                        else // 새로운 블록 생성
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            piece = n;
                        }

                        // 새 블록을 초기화하여 빈 공간에 배치
                        piece.Initialize(newVal, p, pieces[newVal - 1]);
                        piece.rect.anchoredPosition = getPositionFromPoint(fallPnt); // 초기 위치 설정

                        // 현재 위치에 블록을 배치하고 초기화
                        Node hole = getNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[x]++;
                    }
                    break;
                }
            }
        }
    }

    // 특정 NodePiece가 속한 FlippedPieces 객체를 찾는 함수
    FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null; // 반환할 FlippedPieces 객체 초기화
        // flipped 리스트를 순회하면서 현재 NodePiece가 포함된 FlippedPieces를 찾음
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].getOtherPiece(p) != null) // 해당 NodePiece가 포함된 경우
            {
                flip = flipped[i]; // 찾은 FlippedPieces 저장
                break;
            }
        }
        return flip; // 찾은 FlippedPieces를 반환 (없으면 null 반환)
    }

    // 게임을 시작하는 함수
    void StartGame()
    {
        fills = new int[width]; // 배열 초기화
        // 난수 생성
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        // 게임 중 필요한 리스트 초기화
        update = new List<NodePiece>(); // 이동 중인 블록 리스트
        flipped = new List<FlippedPieces>(); // 이동된 블록 리스트
        dead = new List<NodePiece>(); // 제거된 블록 리스트
        killed = new List<KilledPiece>();

        // 보드 초기화
        InitializeBoard();  // 기본 보드 생성
        VerifyBoard();      // 유효한 보드인지 검사(매치가 없는지)
        InstantianteBoard();// 실제 보드에 블록 배치

        // 타이머 초기화
        remainingTime = gameDuration;
        isGameActive = true;
        UpdateTimerUI();

        // AudioSource 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 게임 보드의 초기 상태를 설정하는 함수
    void InitializeBoard()
    {
        board = new Node[width, height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                // boardLayout을 확인하여 해당 위치가 막힌 공간인지 확인, Blank(-1) 또는 일반 블록으로 초기화
                board[x, y] = new Node((boardLayout.rows[y].row[x]) ? -1 : fillPiece(), new Point(x, y));
            }
        }
    }

    // 보드에 초기 생성된 블록들이 바로 매치되지 않도록 검증하는 함수
    void VerifyBoard()
    {
        List<int> remove; // 제거해야 할 값들을 저장할 리스트
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = getValueAtPoint(p);

                // 빈 공간(-1, 0)일 경우 건너뜀
                if (val <= 0) continue;

                remove = new List<int>();

                // 현재 위치가 매치되는 경우가 있는지 확인
                while (isConnected(p, true).Count > 0)
                {
                    val = getValueAtPoint(p);
                    if(!remove.Contains(val)) // 이미 제거된 값이면 리스트에 추가하지 않음
                    {
                        remove.Add(val);
                    }
                    // 새로운 값으로 변경하여 다시 검사
                    setValueAtPoint(p, newValue(ref remove));
                }
            }
        }
    }

    // 보드에 블록을 생성하고 배치하는 함수
    void InstantianteBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = getNodeAtPoint(new Point(x, y));

                // 보드에서 해당 위치의 블록 값을 가져옴
                int val = board[x, y].value;

                if (val == -1) // 막힌 공간
                {
                    GameObject block = Instantiate(nodePiece, gameBoard); // 기존 노드 피스를 사용
                    NodePiece blockedPiece = block.GetComponent<NodePiece>();
                    RectTransform blockedRect = block.GetComponent<RectTransform>(); // 이름 변경
                    blockedRect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                    blockedPiece.Initialize(-1, new Point(x, y), blockedSpaceSprite); // 막힌 공간 스프라이트 설정
                    node.SetPiece(blockedPiece);
                    continue;
                }

                if (val <= 0) continue; // 빈 공간일 경우 아무 작업도 하지 않음
                // 일반 블록을 생성
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y)); // 블록 위치 설정
                piece.Initialize(val, new Point(x, y), pieces[val - 1]); // 블록 데이터 초기화
                node.SetPiece(piece); // 보드 노드에 블록 할당
            }
        }
    }

    // 블록을 원래 위치로 되돌리고 다시 업데이트 리스트에 추가하는 함수
    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition(); // 블록의 위치를 원래대로 되돌림
        update.Add(piece); // 업데이트 리스트에 추가하여 다시 처리되도록 함
    }

    // 두 블록의 위치를 교환하는 함수
    public void FlipPieces(Point one, Point two, bool main)
    {
        // 첫 번째 블록이 유효한 블록인지 확인 (빈 공간이면 동작하지 않음)
        if (getValueAtPoint(one) < 0) return;

        // 첫 번째 블록의 정보 가져오기
        Node nodeOne = getNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        // 두 번째 블록이 유효한 블록인지 확인
        if (getValueAtPoint(two) > 0)
        {
            // 두 번째 블록의 정보 가져오기
            Node nodeTwo = getNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();
            // 두 블록의 위치를 서로 변경
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            // 사용자가 직접 블록을 이동한 경우, `flipped` 리스트에 추가하여 기록
            if (main)
            {
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));
            }

            // 두 블록을 업데이트 리스트에 추가하여 애니메이션 처리
            update.Add(pieceOne);
            update.Add(pieceTwo);

            // 이동 효과음 재생
            PlaySound(moveSound);
        }
        else // 두 번째 블록이 빈 공간일 경우, 원래 위치로 되돌림
        {
            ResetPiece(pieceOne);
        }
    }

    // 특정 위치의 블록을 제거하고 사라지는 애니메이션을 실행하는 함수
    void KillPiece(Point p)
    {
        List<KilledPiece> available = new List<KilledPiece>();
        // 이미 사라진 블록 중에서 사용할 수 있는 것을 찾음
        for (int i = 0; i < killed.Count; i++)
        {
            if (!killed[i].falling) available.Add(killed[i]);
        }
        KilledPiece set = null;
        if(available.Count > 0)  // 사용 가능한 블록이 있으면 재사용
        {
            set = available[0];
        }
        else // 사용 가능한 블록이 없으면 새로 생성
        {
            GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
            KilledPiece kPiece = kill.GetComponent<KilledPiece>();
            set = kPiece;
            killed.Add(kPiece);
        }

        // 제거할 블록의 값 (배열 인덱스에 맞게 -1)
        int val = getValueAtPoint(p) - 1;

        // 블록이 유효한 범위 내에 있으면 애니메이션 실행
        if (set != null && val >= 0 && val < pieces.Length)
        {
            set.Initialize(pieces[val], getPositionFromPoint(p));
        }    
    }

    List<Point> isConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>(); // 연결된 블록을 저장할 리스트
        int val = getValueAtPoint(p); // 현재 블록의 값 가져오기
        Point[] directions =
        {
            Point.up,   // 위쪽
            Point.right,// 오른쪽
            Point.down, // 아래쪽
            Point.left  // 왼쪽
        };

        foreach(Point dir in directions)
        {
            List<Point> line = new List<Point>(); // 한 방향으로 탐색된 블록 리스트

            int same = 0;
            for(int i = 1; i < 3; i++) // 최대 2칸까지 같은 블록이 있는지 확인
            {
                Point check = Point.add(p, Point.mult(dir, i));
                if(getValueAtPoint(check) == val) // 같은 값이면 리스트에 추가
                {
                    line.Add(check);
                    same++;
                }
            }

            if(same > 1) // 같은 블록이 2개 이상이면 유요한 매치
            {
                AddPoints(ref connected, line);
            }
        }

        for (int i = 0; i < 2; i++) // o x o
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[i + 2]) };
            foreach(Point next in check) // 양쪽에 같은 블록이 있는지 확인
            {
                if (getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if(same > 1) // 양쪽에서 같은 블록이 있으면 유효한 매치
            {
                AddPoints(ref connected, line);
            }    
        }

        for(int i = 0; i < 4; i++) // 4방향에서 2x2 패턴 확인
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if(next >= 4)
            {
                next -= 4;
            }
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[next]), Point.add(p, Point.add(directions[i], directions[next])) };
            foreach(Point pnt in check) // 같은 블록이면 리스트에 추가
            {
                if (getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if(same > 2) // 2x2 패턴이 완성되었으면 추가
            {
                AddPoints(ref connected, square);
            }
        }

        if(main) // 현재 검사 중인 블록이 처음 검사한 블록이라면
        {
            for(int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, isConnected(connected[i], false)); // 재귀호출
            }
        }

        return connected;
    }

    void AddPoints(ref List<Point> points, List<Point> add)
    {
        foreach(Point p in add)
        {
            bool doAdd = true;

            for(int i = 0; i < points.Count; i++)
            {
                if(points[i].Equals(p)) // 이미 리스트에 존재하면 추가하지 않음
                {
                    doAdd = false;
                    break;
                }
            }

            if(doAdd)
            {
                points.Add(p); // 중복되지 않는 경우에만 추가
            }
        }
    }

    int fillPiece() // 새로운 블록 랜덤 값 생성
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return val;
    }

    int getValueAtPoint(Point p) // 해당 위치의 블록 값을 가져오는 함수
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) // 보드 밖이면 -1 반환
            return -1;
        return board[p.x, p.y].value; // 해당 위치의 값 반환
    }

    void setValueAtPoint(Point p, int v) // 해당 위치의 블록 값을 v로 설정
    {
        board[p.x, p.y].value = v;
    }

    Node getNodeAtPoint(Point p) // 해당 위치의 node 객체 반환하는 함수
    {
        return board[p.x, p.y];
    }

    int newValue(ref List<int> remove) // 사용할 수 있는 새로운 블록 값을 생성하는 함수
    {
        List<int> available = new List<int>();
        for(int i = 0; i < pieces.Length; i++)
        {
            available.Add(i + 1); // 1부터 Length까지 추가
        }
        foreach(int i in remove)
        {
            available.Remove(i); // 제거해야 하는 값 제거
        }
        if (available.Count <= 0) return 0; // 사용할 값이 없으면 0 반환
        return available[random.Next(0, available.Count)]; // 랜덤 값 선택
    }

    string getRandomSeed() // 무작위 시드 문자열을 생성하는 함수
    {
        // 랜덤한 게임시드를 만들때 사용
        // 동일한 시드를 사용하면 같은 패턴으로 게임이 시작됨
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklnmopqrstuvwxyz1234567890!@#$%^&*()";
        for(int i = 0; i < 20; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }
        return seed;
    }

    // 보드의 특정 Point(x,y) 좌표를 Vector2로 변환하는 함수
    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }

    // 무작위 장애물을 생성하는 함수
    void SpawnObstacle()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);

        Point randomPoint = new Point(x, y);

        // 이미 장애물이거나 빈 공간인 경우 건너뜀
        if (getValueAtPoint(randomPoint) == -1 || getValueAtPoint(randomPoint) == 0)
            return;

        // 장애물 생성
        Node node = getNodeAtPoint(randomPoint);
        node.value = -1;

        GameObject block = Instantiate(nodePiece, gameBoard); // 기존 노드 피스를 사용
        NodePiece blockedPiece = block.GetComponent<NodePiece>();
        RectTransform blockedRect = block.GetComponent<RectTransform>();
        blockedRect.anchoredPosition = getPositionFromPoint(randomPoint);
        blockedPiece.Initialize(-1, randomPoint, blockedSpaceSprite); // 장애물 스프라이트 설정
        node.SetPiece(blockedPiece);

        obstacleBlock++;
        blockText.text = "" + obstacleBlock;
        Debug.Log($"Obstacle spawned at ({x}, {y})");
    }

    // 게임 타이머 UI업데이트 함수
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        int obstacleTime = Mathf.FloorToInt(obstacleSpawnTimer);
        obstacleTimerText.text = "" + obstacleSpawnTimer;
    }

    // 효과음을 재생하는 함수
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void GameOver()
    {
        isGameActive = false;
        ResultPanel.SetActive(true);
        FinalScoreText.text = "" + curScore;
        // 추가적으로 게임 오버 화면을 표시하거나 재시작 버튼을 활성화할 수 있음.
    }
}

// 보드 상의 각 칸을 나타내며, 해당 칸에 속한 값과 게임 오브젝트 관리
[System.Serializable]
public class Node
{
    public int value; // 0 = blank, 1 = Fire, 2 = Water, 3 = Thunder, 4 = Earth, 5 = Wind, -1 = hole
    public Point index;
    NodePiece piece;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }

    public NodePiece getPiece()
    {
        return piece;
    }
}

// 두 피스를 서로 교환할 때 사용
[System.Serializable]
public class FlippedPieces
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece getOtherPiece(NodePiece p)
    {
        if (p == one)
            return two;
        else if (p == two)
            return one;
        else
            return null;
    }

}
