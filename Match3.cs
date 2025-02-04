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
    public Text timerText; // Ÿ�̸Ӹ� ǥ���� UI �ؽ�Ʈ
    public Text obstacleTimerText;
    public float gameDuration = 10f; // ���� ���� �ð� (�� ����)

    [Header("Audio Settings")]
    public AudioClip moveSound; // ���� �̵� ȿ����
    public AudioClip matchSound; // ���� ��Ī ȿ����
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
            // Ÿ�̸� ������Ʈ
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

        // ������Ʈ�� �Ϸ�� NodePiece ����� ������ ����Ʈ ����
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        for(int i = 0; i < update.Count; i++) // ���� ������Ʈ ����Ʈ�� �ִ� ��� NodePiece�� �˻�
        {
            NodePiece piece = update[i];
            // NodePiece�� ������Ʈ�� �Ϸ��ߴ��� Ȯ��
            if (!piece.UpdatePiece()) finishedUpdating.Add(piece); // �Ϸ� �ƴٸ� ��� �߰�
        }
        // ������Ʈ�� �Ϸ�� ������ ó��
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            // ����� ���������� Ȯ��
            FlippedPieces flip = getFlipped(piece);
            NodePiece flippedPiece = null;
            // ���� ����� x��ǥ�� ������ �� �ش翭���� ����� �ϳ� �پ����� �ݿ�
            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            // ���� ��忡�� ����� ��ϵ��� ã��
            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null); // ������ ��������

            if (wasFlipped) // ���� ����� �������� ������Ʈ �� ���
            {
                flippedPiece = flip.getOtherPiece(piece); // �ٸ� ����� ������
                // ������ ����� ����� ��ϵ� Ȯ���Ͽ� �߰�
                AddPoints(ref connected, isConnected(flippedPiece.index, true));
            }

            if(connected.Count == 0) // ��ġ�� �߻����� �ʾҴٸ�
            {
                if (wasFlipped && flippedPiece != null) // ����� �������� ���
                {
                    FlipPieces(piece.index, flippedPiece.index, false); // �������� ����� �ٽ� �ǵ���
                }
            }
            else // ��ġ�� �߻����� ���
            {
                PlaySound(matchSound);

                // ��ġ�� ��� ���� ���� ���� �߰�
                int matchScroe = connected.Count * 10;
                curScore += matchScroe;
                scoreText.text = "" + curScore;

                // ���� �ð��� �߰�
                if (remainingTime >= 30f)
                    remainingTime = 30f;
                remainingTime += 1f;

                foreach (Point pnt in connected) // ����� ��� �� ����
                {
                    KillPiece(pnt); // �ش� ��ġ�� ��� ����
                    // �ش� ��ġ�� Node ������ �����ͼ� ��� ����
                    Node node = getNodeAtPoint(pnt);
                    NodePiece nodePiece = node.getPiece();
                    if(nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false); // ��� ��Ȱ��ȭ
                        dead.Add(nodePiece); // ����Ʈ�� �߰�
                    }
                    node.SetPiece(null); // �ش� ����� ��� ����
                }

                ApplyGravityToBoard();
            }
            flipped.Remove(flip); // �������� ó���� ������ ����Ʈ���� ����
            update.Remove(piece); // ������Ʈ ����Ʈ������ �ش� ��� ����
        }
    }

    // ������ �� ������ ä��� �Լ�
    void ApplyGravityToBoard()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = (height-1); y >= 0; y--)
            {
                Point p = new Point(x, y);
                Node node = getNodeAtPoint(p);
                int val = getValueAtPoint(p);
                if (val != 0) continue; // ���� ��ġ�� ������� �ƴ϶��
                // ���ʿ��� ����� ã�� �� ������ ä��
                for(int ny = (y-1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = getValueAtPoint(next);
                    if (nextVal == 0)
                        continue;
                    if(nextVal!= -1) // ���ʿ� �ִ� ���� Blank��� �ٽ� Ž��, hole�� �ƴ϶�� ��ȿ�� ���
                    {
                        Node got = getNodeAtPoint(next);
                        NodePiece piece = got.getPiece();

                        // ���� �� ������ ����� ����
                        node.SetPiece(piece);
                        update.Add(piece); // ������Ʈ ����Ʈ�� �߰�

                        // ���� ��ġ�� null�� ����
                        got.SetPiece(null);
                    }
                    else // ���� ���� ���̶��, ���ο� ��� ����
                    {
                        int newVal = fillPiece(); // ���ο� ��� �� ����
                        NodePiece piece;
                        Point fallPnt = new Point(x, (-1 - fills[x])); // �� ����� �������� ���� ��ġ
                        if(dead.Count > 0) // ���� ��Ȱ��ȭ��(dead) ����� �ִٸ� ����
                        {
                            NodePiece revived = dead[0];
                            revived.gameObject.SetActive(true); // �ٽ� Ȱ��ȭ
                            piece = revived;

                            dead.RemoveAt(0); // ����� ��� ����
                        }
                        else // ���ο� ��� ����
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            piece = n;
                        }

                        // �� ����� �ʱ�ȭ�Ͽ� �� ������ ��ġ
                        piece.Initialize(newVal, p, pieces[newVal - 1]);
                        piece.rect.anchoredPosition = getPositionFromPoint(fallPnt); // �ʱ� ��ġ ����

                        // ���� ��ġ�� ����� ��ġ�ϰ� �ʱ�ȭ
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

    // Ư�� NodePiece�� ���� FlippedPieces ��ü�� ã�� �Լ�
    FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null; // ��ȯ�� FlippedPieces ��ü �ʱ�ȭ
        // flipped ����Ʈ�� ��ȸ�ϸ鼭 ���� NodePiece�� ���Ե� FlippedPieces�� ã��
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].getOtherPiece(p) != null) // �ش� NodePiece�� ���Ե� ���
            {
                flip = flipped[i]; // ã�� FlippedPieces ����
                break;
            }
        }
        return flip; // ã�� FlippedPieces�� ��ȯ (������ null ��ȯ)
    }

    // ������ �����ϴ� �Լ�
    void StartGame()
    {
        fills = new int[width]; // �迭 �ʱ�ȭ
        // ���� ����
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        // ���� �� �ʿ��� ����Ʈ �ʱ�ȭ
        update = new List<NodePiece>(); // �̵� ���� ��� ����Ʈ
        flipped = new List<FlippedPieces>(); // �̵��� ��� ����Ʈ
        dead = new List<NodePiece>(); // ���ŵ� ��� ����Ʈ
        killed = new List<KilledPiece>();

        // ���� �ʱ�ȭ
        InitializeBoard();  // �⺻ ���� ����
        VerifyBoard();      // ��ȿ�� �������� �˻�(��ġ�� ������)
        InstantianteBoard();// ���� ���忡 ��� ��ġ

        // Ÿ�̸� �ʱ�ȭ
        remainingTime = gameDuration;
        isGameActive = true;
        UpdateTimerUI();

        // AudioSource �ʱ�ȭ
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ���� ������ �ʱ� ���¸� �����ϴ� �Լ�
    void InitializeBoard()
    {
        board = new Node[width, height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                // boardLayout�� Ȯ���Ͽ� �ش� ��ġ�� ���� �������� Ȯ��, Blank(-1) �Ǵ� �Ϲ� ������� �ʱ�ȭ
                board[x, y] = new Node((boardLayout.rows[y].row[x]) ? -1 : fillPiece(), new Point(x, y));
            }
        }
    }

    // ���忡 �ʱ� ������ ��ϵ��� �ٷ� ��ġ���� �ʵ��� �����ϴ� �Լ�
    void VerifyBoard()
    {
        List<int> remove; // �����ؾ� �� ������ ������ ����Ʈ
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = getValueAtPoint(p);

                // �� ����(-1, 0)�� ��� �ǳʶ�
                if (val <= 0) continue;

                remove = new List<int>();

                // ���� ��ġ�� ��ġ�Ǵ� ��찡 �ִ��� Ȯ��
                while (isConnected(p, true).Count > 0)
                {
                    val = getValueAtPoint(p);
                    if(!remove.Contains(val)) // �̹� ���ŵ� ���̸� ����Ʈ�� �߰����� ����
                    {
                        remove.Add(val);
                    }
                    // ���ο� ������ �����Ͽ� �ٽ� �˻�
                    setValueAtPoint(p, newValue(ref remove));
                }
            }
        }
    }

    // ���忡 ����� �����ϰ� ��ġ�ϴ� �Լ�
    void InstantianteBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = getNodeAtPoint(new Point(x, y));

                // ���忡�� �ش� ��ġ�� ��� ���� ������
                int val = board[x, y].value;

                if (val == -1) // ���� ����
                {
                    GameObject block = Instantiate(nodePiece, gameBoard); // ���� ��� �ǽ��� ���
                    NodePiece blockedPiece = block.GetComponent<NodePiece>();
                    RectTransform blockedRect = block.GetComponent<RectTransform>(); // �̸� ����
                    blockedRect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                    blockedPiece.Initialize(-1, new Point(x, y), blockedSpaceSprite); // ���� ���� ��������Ʈ ����
                    node.SetPiece(blockedPiece);
                    continue;
                }

                if (val <= 0) continue; // �� ������ ��� �ƹ� �۾��� ���� ����
                // �Ϲ� ����� ����
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y)); // ��� ��ġ ����
                piece.Initialize(val, new Point(x, y), pieces[val - 1]); // ��� ������ �ʱ�ȭ
                node.SetPiece(piece); // ���� ��忡 ��� �Ҵ�
            }
        }
    }

    // ����� ���� ��ġ�� �ǵ����� �ٽ� ������Ʈ ����Ʈ�� �߰��ϴ� �Լ�
    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition(); // ����� ��ġ�� ������� �ǵ���
        update.Add(piece); // ������Ʈ ����Ʈ�� �߰��Ͽ� �ٽ� ó���ǵ��� ��
    }

    // �� ����� ��ġ�� ��ȯ�ϴ� �Լ�
    public void FlipPieces(Point one, Point two, bool main)
    {
        // ù ��° ����� ��ȿ�� ������� Ȯ�� (�� �����̸� �������� ����)
        if (getValueAtPoint(one) < 0) return;

        // ù ��° ����� ���� ��������
        Node nodeOne = getNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        // �� ��° ����� ��ȿ�� ������� Ȯ��
        if (getValueAtPoint(two) > 0)
        {
            // �� ��° ����� ���� ��������
            Node nodeTwo = getNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();
            // �� ����� ��ġ�� ���� ����
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            // ����ڰ� ���� ����� �̵��� ���, `flipped` ����Ʈ�� �߰��Ͽ� ���
            if (main)
            {
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));
            }

            // �� ����� ������Ʈ ����Ʈ�� �߰��Ͽ� �ִϸ��̼� ó��
            update.Add(pieceOne);
            update.Add(pieceTwo);

            // �̵� ȿ���� ���
            PlaySound(moveSound);
        }
        else // �� ��° ����� �� ������ ���, ���� ��ġ�� �ǵ���
        {
            ResetPiece(pieceOne);
        }
    }

    // Ư�� ��ġ�� ����� �����ϰ� ������� �ִϸ��̼��� �����ϴ� �Լ�
    void KillPiece(Point p)
    {
        List<KilledPiece> available = new List<KilledPiece>();
        // �̹� ����� ��� �߿��� ����� �� �ִ� ���� ã��
        for (int i = 0; i < killed.Count; i++)
        {
            if (!killed[i].falling) available.Add(killed[i]);
        }
        KilledPiece set = null;
        if(available.Count > 0)  // ��� ������ ����� ������ ����
        {
            set = available[0];
        }
        else // ��� ������ ����� ������ ���� ����
        {
            GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
            KilledPiece kPiece = kill.GetComponent<KilledPiece>();
            set = kPiece;
            killed.Add(kPiece);
        }

        // ������ ����� �� (�迭 �ε����� �°� -1)
        int val = getValueAtPoint(p) - 1;

        // ����� ��ȿ�� ���� ���� ������ �ִϸ��̼� ����
        if (set != null && val >= 0 && val < pieces.Length)
        {
            set.Initialize(pieces[val], getPositionFromPoint(p));
        }    
    }

    List<Point> isConnected(Point p, bool main)
    {
        List<Point> connected = new List<Point>(); // ����� ����� ������ ����Ʈ
        int val = getValueAtPoint(p); // ���� ����� �� ��������
        Point[] directions =
        {
            Point.up,   // ����
            Point.right,// ������
            Point.down, // �Ʒ���
            Point.left  // ����
        };

        foreach(Point dir in directions)
        {
            List<Point> line = new List<Point>(); // �� �������� Ž���� ��� ����Ʈ

            int same = 0;
            for(int i = 1; i < 3; i++) // �ִ� 2ĭ���� ���� ����� �ִ��� Ȯ��
            {
                Point check = Point.add(p, Point.mult(dir, i));
                if(getValueAtPoint(check) == val) // ���� ���̸� ����Ʈ�� �߰�
                {
                    line.Add(check);
                    same++;
                }
            }

            if(same > 1) // ���� ����� 2�� �̻��̸� ������ ��ġ
            {
                AddPoints(ref connected, line);
            }
        }

        for (int i = 0; i < 2; i++) // o x o
        {
            List<Point> line = new List<Point>();

            int same = 0;
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[i + 2]) };
            foreach(Point next in check) // ���ʿ� ���� ����� �ִ��� Ȯ��
            {
                if (getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if(same > 1) // ���ʿ��� ���� ����� ������ ��ȿ�� ��ġ
            {
                AddPoints(ref connected, line);
            }    
        }

        for(int i = 0; i < 4; i++) // 4���⿡�� 2x2 ���� Ȯ��
        {
            List<Point> square = new List<Point>();

            int same = 0;
            int next = i + 1;
            if(next >= 4)
            {
                next -= 4;
            }
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[next]), Point.add(p, Point.add(directions[i], directions[next])) };
            foreach(Point pnt in check) // ���� ����̸� ����Ʈ�� �߰�
            {
                if (getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if(same > 2) // 2x2 ������ �ϼ��Ǿ����� �߰�
            {
                AddPoints(ref connected, square);
            }
        }

        if(main) // ���� �˻� ���� ����� ó�� �˻��� ����̶��
        {
            for(int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, isConnected(connected[i], false)); // ���ȣ��
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
                if(points[i].Equals(p)) // �̹� ����Ʈ�� �����ϸ� �߰����� ����
                {
                    doAdd = false;
                    break;
                }
            }

            if(doAdd)
            {
                points.Add(p); // �ߺ����� �ʴ� ��쿡�� �߰�
            }
        }
    }

    int fillPiece() // ���ο� ��� ���� �� ����
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return val;
    }

    int getValueAtPoint(Point p) // �ش� ��ġ�� ��� ���� �������� �Լ�
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) // ���� ���̸� -1 ��ȯ
            return -1;
        return board[p.x, p.y].value; // �ش� ��ġ�� �� ��ȯ
    }

    void setValueAtPoint(Point p, int v) // �ش� ��ġ�� ��� ���� v�� ����
    {
        board[p.x, p.y].value = v;
    }

    Node getNodeAtPoint(Point p) // �ش� ��ġ�� node ��ü ��ȯ�ϴ� �Լ�
    {
        return board[p.x, p.y];
    }

    int newValue(ref List<int> remove) // ����� �� �ִ� ���ο� ��� ���� �����ϴ� �Լ�
    {
        List<int> available = new List<int>();
        for(int i = 0; i < pieces.Length; i++)
        {
            available.Add(i + 1); // 1���� Length���� �߰�
        }
        foreach(int i in remove)
        {
            available.Remove(i); // �����ؾ� �ϴ� �� ����
        }
        if (available.Count <= 0) return 0; // ����� ���� ������ 0 ��ȯ
        return available[random.Next(0, available.Count)]; // ���� �� ����
    }

    string getRandomSeed() // ������ �õ� ���ڿ��� �����ϴ� �Լ�
    {
        // ������ ���ӽõ带 ���鶧 ���
        // ������ �õ带 ����ϸ� ���� �������� ������ ���۵�
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklnmopqrstuvwxyz1234567890!@#$%^&*()";
        for(int i = 0; i < 20; i++)
        {
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        }
        return seed;
    }

    // ������ Ư�� Point(x,y) ��ǥ�� Vector2�� ��ȯ�ϴ� �Լ�
    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }

    // ������ ��ֹ��� �����ϴ� �Լ�
    void SpawnObstacle()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);

        Point randomPoint = new Point(x, y);

        // �̹� ��ֹ��̰ų� �� ������ ��� �ǳʶ�
        if (getValueAtPoint(randomPoint) == -1 || getValueAtPoint(randomPoint) == 0)
            return;

        // ��ֹ� ����
        Node node = getNodeAtPoint(randomPoint);
        node.value = -1;

        GameObject block = Instantiate(nodePiece, gameBoard); // ���� ��� �ǽ��� ���
        NodePiece blockedPiece = block.GetComponent<NodePiece>();
        RectTransform blockedRect = block.GetComponent<RectTransform>();
        blockedRect.anchoredPosition = getPositionFromPoint(randomPoint);
        blockedPiece.Initialize(-1, randomPoint, blockedSpaceSprite); // ��ֹ� ��������Ʈ ����
        node.SetPiece(blockedPiece);

        obstacleBlock++;
        blockText.text = "" + obstacleBlock;
        Debug.Log($"Obstacle spawned at ({x}, {y})");
    }

    // ���� Ÿ�̸� UI������Ʈ �Լ�
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        int obstacleTime = Mathf.FloorToInt(obstacleSpawnTimer);
        obstacleTimerText.text = "" + obstacleSpawnTimer;
    }

    // ȿ������ ����ϴ� �Լ�
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
        // �߰������� ���� ���� ȭ���� ǥ���ϰų� ����� ��ư�� Ȱ��ȭ�� �� ����.
    }
}

// ���� ���� �� ĭ�� ��Ÿ����, �ش� ĭ�� ���� ���� ���� ������Ʈ ����
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

// �� �ǽ��� ���� ��ȯ�� �� ���
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
