using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;
using UnityEngine.UI;
using TMPro;

public class MainGame : MonoBehaviour
{
    public static MainGame Instance;
    public List<PlayerData> PlayersList = new List<PlayerData>(29);
    public List<TextMeshProUGUI> PlayerPseudo;
    public List<Bubble> bubblesOnBoard = new List<Bubble>();
    public List<Vector2Int> EmptyPos = new List<Vector2Int>();
    public Bubble bubbleOnBoard;

    // Server Stuff
    private Connection pioconnection;
    private List<Message> msgList = new List<Message>(); //  Messsage queue implementation
    private bool joinedroom = false;
    private string infomsg = "";
    public GameObject target;
    // UI stuff
    private Vector2 scrollPosition;
    private ArrayList entries = new ArrayList();
    private string inputField = "";
    enum GameState
    {
        Normal,
        Won,
        Lost
    }

    public Transform BubbleTopLeft;
    public Transform BubbleStart;
    public Transform NextBubbleStart;
    public Transform Board;
    public GameObject[] PrefabBubbles;
    public float BubbleSize = 1.6f;
    public int Lines = 11;
    public int Width = 8;
    public GameObject Canon;
    public float RotationSpeed = Mathf.PI / 4;
    public GameObject Lost;
    public GameObject Won;
    public Transform Death;

    public Bubble[,] BubblesGrid;

    public class ChatEntry
    {
        public string text = "";
        public bool mine = true;
    }
    Bubble _currentBubble;
    Bubble _nextBubble;
    float _angle;
    GameState _state;
    void handlemessage(object sender, Message m)
    {
        msgList.Add(m);
    }
    public void Start()
    {
        Application.runInBackground = true;

        // Create a random userid 
        System.Random random = new System.Random();
        string userid = "Guest" + random.Next(0, 10000);
        Debug.Log("Starting");


        PlayerIO.Authenticate(
            "bubblemultiplayer-zatagsqclku6yvvgdwkwtq",
            "public",                               //Your connection id
            new Dictionary<string, string> {        //Authentication arguments
				{ "userId", userid },
            },
            null,                                   //PlayerInsight segments
            delegate (Client client) {
                Debug.Log("Successfully connected to Player.IO");
                infomsg = "Successfully connected to Player.IO";

                target.transform.name = userid;

                Debug.Log("Create ServerEndpoint");
                // Comment out the line below to use the live servers instead of your development server
                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                Debug.Log("CreateJoinRoom");
                //Create or join the room 
                client.Multiplayer.CreateJoinRoom(
                    "",                    //Room id. If set to null a random roomid is used
                    "BubbleMulti",                   //The room type started on the server
                    true,                               //Should the room be visible in the lobby?
                    null,
                    null,
                    delegate (Connection connection) {
                        Debug.Log("Joined Room.");
                        infomsg = "Joined Room.";
                        // We successfully joined a room so set up the message handler
                        pioconnection = connection;
                        pioconnection.OnMessage += handlemessage;
                        joinedroom = true;
                    },
                    delegate (PlayerIOError error) {
                        Debug.Log("Error Joining Room: " + error.ToString());
                        infomsg = error.ToString();
                    }
                );
            },
            delegate (PlayerIOError error) {
                Debug.Log("Error connecting: " + error.ToString());
                infomsg = error.ToString();
            });
        Instance = this;

        BubblesGrid = new Bubble[Width, Lines];

        //for (int y = 0; y < 3; y++)
        //{
        //    int xCount = y % 2 == 0 ? 8 : 7;
        //    float xOffset = y % 2 == 0 ? 0 : BubbleSize / 2.0f;

        //    for (int x = 0; x < xCount; x++)
        //    {
        //        int rnd = Random.Range(0, PrefabBubbles.Length);
        //        GameObject go = GameObject.Instantiate(PrefabBubbles[rnd], GridToWorld(x, y), Quaternion.identity);
        //        go.transform.parent = Board;
        //    }
        //}

        SpawnNewBubble();
    }
    void FixedUpdate()
    {
        // process message queue
        foreach (Message m in msgList)
        {
            switch (m.Type)
            {
                case "PlayerJoined":
                    PlayerData newPlayer = new PlayerData();
                    newPlayer.playerName = m.GetString(0);
                    newPlayer.playerBoard = GameObject.Instantiate(target) as GameObject;
                    PlayersList.Add(newPlayer);
                    newPlayer.playerBoard.transform.Find("NameTag").GetComponent<TextMesh>().text = m.GetString(0);
                    break;
                case "PlayerLeft":
                    // remove characters from the scene when they leave
                    GameObject playerd = GameObject.Find(m.GetString(0));
                    //PlayersList.Remove(playerd);
                    Destroy(playerd);
                    break;
                case "Chat":
                    if (m.GetString(0) != "Server")
                    {
                        GameObject chatplayer = GameObject.Find(m.GetString(0));
                        chatplayer.transform.Find("Chat").GetComponent<TextMesh>().text = m.GetString(1);
                        chatplayer.transform.Find("Chat").GetComponent<MeshRenderer>().material.color = Color.white;
                        chatplayer.transform.Find("Chat").GetComponent<ChatClear>().lastupdate = Time.time;
                    }
                    ChatText(m.GetString(0) + " says: " + m.GetString(1), false);
                    break;
                case "Lose":
                    GameObject losePlayer = GameObject.Find(m.GetString(0));
                    break;
            }
        }

        // clear message queue after it's been processed
        msgList.Clear();
    }
    void ChatText(string str, bool own)
    {
        var entry = new ChatEntry();
        entry.text = str;
        entry.mine = own;

        entries.Add(entry);

        if (entries.Count > 50)
            entries.RemoveAt(0);

        scrollPosition.y = 1000000;
    }
    void GlobalChatWindow(int id)
    {

        if (!joinedroom)
        {
            return;
        }

        GUI.FocusControl("Chat input field");

        // Begin a scroll view. All rects are calculated automatically - 
        // it will use up any available screen space and make sure contents flow correctly.
        // This is kept small with the last two parameters to force scrollbars to appear.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (ChatEntry entry in entries)
        {
            GUILayout.BeginHorizontal();
            if (!entry.mine)
            {
                GUILayout.Label(entry.text);
            }
            else
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label(entry.text);
                GUI.contentColor = Color.white;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(3);

        }
        // End the scrollview we began above.
        GUILayout.EndScrollView();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && inputField.Length > 0)
        {

            GameObject chatplayer = GameObject.Find(target.transform.name);
            chatplayer.transform.Find("Chat").GetComponent<TextMesh>().text = inputField;
            chatplayer.transform.Find("Chat").GetComponent<MeshRenderer>().material.color = Color.white;
            chatplayer.transform.Find("Chat").GetComponent<ChatClear>().lastupdate = Time.time;

            ChatText(target.transform.name + " says: " + inputField, true);
            pioconnection.Send("Chat", inputField);
            inputField = "";
        }
        GUI.SetNextControlName("Chat input field");
        inputField = GUILayout.TextField(inputField);

        GUI.DragWindow();
    }
    void SpawnNewBubble()
    {
        int rnd = Random.Range(0, PrefabBubbles.Length);
        GameObject go;
        if (_nextBubble == null)
        {
            go = GameObject.Instantiate(PrefabBubbles[rnd], BubbleStart.transform.position, Quaternion.identity);
            _currentBubble = go.GetComponent<Bubble>();
        }
        else
        {
            _currentBubble = _nextBubble;
            _currentBubble.transform.position = BubbleStart.transform.position;
        }

        rnd = Random.Range(0, PrefabBubbles.Length);

        go = GameObject.Instantiate(PrefabBubbles[rnd], NextBubbleStart.transform.position, Quaternion.identity);
        _nextBubble = go.GetComponent<Bubble>();
    }

    public void Update()
    {
        for (int i = 0; i < PlayerPseudo.Count; i++)
        {
            if (PlayersList[i] != null)
            {
                PlayerPseudo[i].text = PlayersList[i].playerName;
            }

        }
        if (_state != GameState.Normal)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _currentBubble.Move(new Vector2(Mathf.Cos(_angle + Mathf.PI / 2.0f), Mathf.Sin(_angle + Mathf.PI / 2.0f)));
            SpawnNewBubble();

        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (_angle < Mathf.PI / 3.0f)
                _angle += RotationSpeed * Time.deltaTime;
            else
                _angle = Mathf.PI / 3.0f;

            Canon.transform.rotation = Quaternion.Euler(0, 0, _angle * Mathf.Rad2Deg);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (_angle > -Mathf.PI / 3.0f)
                _angle -= RotationSpeed * Time.deltaTime;
            else
                _angle = -Mathf.PI / 3.0f;

            Canon.transform.rotation = Quaternion.Euler(0, 0, _angle * Mathf.Rad2Deg);

        }
    }



    public Vector3 GridToWorld(int x, int y)
    {
        int xCount = y % 2 == 0 ? Width : Width - 1;
        float xOffset = y % 2 == 0 ? 0 : BubbleSize / 2.0f;

        return new Vector3(BubbleTopLeft.transform.position.x + BubbleSize * x + xOffset, BubbleTopLeft.transform.position.y - y * BubbleSize, BubbleTopLeft.transform.position.z);
    }

    public Vector2Int WorldToGrid(Vector3 world)
    {
        Vector2Int bestPosition = new Vector2Int(-1, -1);
        float bestDistanceSq = float.MaxValue;

        for (int y = 0; y < Lines; y++)
        {
            int xCount = y % 2 == 0 ? Width : Width - 1;
            for (int x = 0; x < xCount; x++)
            {
                if (BubblesGrid[x, y] != null)
                    continue;

                float distanceSq = (world - GridToWorld(x, y)).sqrMagnitude;
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    bestPosition = new Vector2Int(x, y);
                }

            }
        }
        return bestPosition;
    }

    public Vector3 WorldToWorldAligned(Vector3 world, out Vector2Int grid)
    {
        grid = WorldToGrid(world);
        return GridToWorld(grid.x, grid.y);
    }
    public void PlayerLose(GameObject target)
    {
        Lost.SetActive(true);
        pioconnection.Send("Lose", target.name);

    }
    public void FixBubble(Bubble bubble)
    {
        if(bubble == null)
        { return; }
        if (bubble.transform.position.y < Death.transform.position.y)
        {
            _state = GameState.Lost;
            Lost.SetActive(true);

            return;
        }

        bubble.transform.position = WorldToWorldAligned(bubble.transform.position, out Vector2Int grid);
        MainGame.Instance.BubblesGrid[grid.x, grid.y] = bubble;

        BubbleColor[,] colorGrid = new BubbleColor[Width, Lines];

        FillNeighbourColor(colorGrid, bubble.Color, grid);

        int count = 0;

        for (int y = 0; y < Lines; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (colorGrid[x, y] == bubble.Color)
                    count++;
            }
        }

        if (count >= 3)
        {
            
            for (int y = 0; y < Lines; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (colorGrid[x, y] == bubble.Color)
                    {
                        Bubble bubbleToDestroy = BubblesGrid[x, y];
                        bubbleToDestroy.DestroyBubble();
                        BubblesGrid[x, y] = null;
                    }
                }
            }
            


            for (int y = 0; y < Lines; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (BubblesGrid[x, y] != null)
                    {
                        BubblesGrid[x, y].Attached = y == 0;
                    }
                }
            }

            bool[,] bubbleChecked = new bool[Width, Lines];
            int countAttached = 0;
            for (int x = 0; x < Width; x++)
            {
                if (BubblesGrid[x, 0] != null)
                {
                    countAttached++;
                    CheckAttached(bubbleChecked, new Vector2Int(x, 0));
                }
            }

            for (int y = 0; y < Lines; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (BubblesGrid[x, y] != null && BubblesGrid[x, y].Attached == false)
                    {
                        Bubble bubbleToDestroy = BubblesGrid[x, y];
                        bubbleToDestroy.DestroyBubble();

                        BubblesGrid[x, y] = null;
                    }
                }
            }
            SendBubble(count - 2);

            if (countAttached == 0)
            {
                _state = GameState.Won;
                Won.SetActive(true);
            }
        }



    }

    void SendBubble(int numberToSend)
    {
        if (numberToSend > 8)
        {
            numberToSend = 8;
        }
        int rnd = Random.Range(0, PrefabBubbles.Length);
        GameObject go;

        for (int j = 0; j < Lines; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                if (BubblesGrid[i, j] != null)
                {
                    bubblesOnBoard.Add(BubblesGrid[i, j]);
                }
                else
                {

                }
            }
        }
        for (int i = 0; i < numberToSend; i++)
        {
            int randomInt = Random.Range(0, bubblesOnBoard.Count-i);
            //Bubble bubbleOnBoard = bubblesOnBoard[randomInt];
            bubbleOnBoard = bubblesOnBoard[randomInt];
            Vector2Int bubbleOnBoardVector = WorldToGrid(bubbleOnBoard.transform.position);
            //List<Vector2Int> emptyPos = new List<Vector2Int>();
            FindEmptyNeighbour(BubblesGrid, bubbleOnBoardVector);
            int randomEmptyPos = Random.Range(0, EmptyPos.Count);
            go = GameObject.Instantiate(PrefabBubbles[rnd], GridToWorld(EmptyPos[randomEmptyPos].x, EmptyPos[randomEmptyPos].y), Quaternion.identity);
            go.transform.position = WorldToWorldAligned(GridToWorld(EmptyPos[randomEmptyPos].x, EmptyPos[randomEmptyPos].y),out Vector2Int grid);
            go.name = "Bubble send" + i;
            go.GetComponent<CircleCollider2D>().enabled = true;
            //BubblesGrid[grid.x, grid.y] = bubbleOnBoard;
            //Debug.Log(grid);
            FixBubble(go.GetComponent<Bubble>());
            bubblesOnBoard.Remove(bubbleOnBoard);
        }
        
        bubblesOnBoard.Clear();
        EmptyPos.Clear();
    }
    public void FixAllBubble()
    {
        foreach(Bubble bubble in BubblesGrid)
        {
            FixBubble(bubble);
        }
    }
    void CheckAttached(bool[,] bubbleChecked, Vector2Int position)
    {
        int xCount = position.y % 2 == 0 ? Width : Width - 1;

        if (position.x >= xCount || position.x < 0)
            return;

        if (position.y >= Lines || position.y < 0)
            return;

        if (bubbleChecked[position.x, position.y] == true)
            return;

        bubbleChecked[position.x, position.y] = true;

        if (BubblesGrid[position.x, position.y] == null)
        {
            return;
        }

        BubblesGrid[position.x, position.y].Attached = true;

        if (position.y % 2 == 0)
        {
            CheckAttached(bubbleChecked, position + new Vector2Int(-1, -1));
            CheckAttached(bubbleChecked, position + new Vector2Int(0, -1));
            CheckAttached(bubbleChecked, position + new Vector2Int(-1, 0));
            CheckAttached(bubbleChecked, position + new Vector2Int(1, 0));
            CheckAttached(bubbleChecked, position + new Vector2Int(-1, 1));
            CheckAttached(bubbleChecked, position + new Vector2Int(0, 1));
        }
        else
        {
            CheckAttached(bubbleChecked, position + new Vector2Int(0, -1));
            CheckAttached(bubbleChecked, position + new Vector2Int(1, -1));
            CheckAttached(bubbleChecked, position + new Vector2Int(-1, 0));
            CheckAttached(bubbleChecked, position + new Vector2Int(1, 0));
            CheckAttached(bubbleChecked, position + new Vector2Int(0, 1));
            CheckAttached(bubbleChecked, position + new Vector2Int(1, 1));
        }
    }
    void NeigtbourCheck(Bubble[,] grid, Vector2Int position, Vector2Int offset)
    {
        int xCount = position.y % 2 == 0 ? Width : Width - 1;
        if (position.x +offset.x >= xCount || position.x + offset.x < 0)
            return;

        if (position.y + offset.y >= Lines || position.y + offset.y < 0)
            return;
       
        if (grid[position.x + offset.x, position.y + offset.y] != null)
        {
            Vector2Int emptyPosition = new Vector2Int(position.x + offset.x, position.y + offset.y);
            Debug.Log("EmptyPos");
            EmptyPos.Add(emptyPosition);
        }
        else
        {

        }
    }
    void FindEmptyNeighbour(Bubble[,] grid, Vector2Int position)
    {
        int xCount = position.y % 2 == 0 ? Width : Width - 1;

        if (position.x >= xCount || position.x < 0)
            return;

        if (position.y >= Width || position.y < 0)
            return;


        if (position.y % 2 == 0)
        {
            NeigtbourCheck(grid, position, new Vector2Int(-1, -1));
            NeigtbourCheck(grid, position, new Vector2Int(0, -1));
            NeigtbourCheck(grid, position, new Vector2Int(-1, 0));
            NeigtbourCheck(grid, position, new Vector2Int(0, -1));
            NeigtbourCheck(grid, position, new Vector2Int(-1, 1));
            NeigtbourCheck(grid, position, new Vector2Int(0, 1));
        }
        if(position.y % 2 == 1)
        {
            NeigtbourCheck(grid, position, new Vector2Int(0, -1));
            NeigtbourCheck(grid, position, new Vector2Int(1, -1));
            NeigtbourCheck(grid, position, new Vector2Int(-1, 0));
            NeigtbourCheck(grid, position, new Vector2Int(0, -1));
            NeigtbourCheck(grid, position, new Vector2Int(0, 1));
            NeigtbourCheck(grid, position, new Vector2Int(1, 1));
        }
        
    }
    void FillNeighbourColor(BubbleColor[,] grid, BubbleColor color, Vector2Int position)
    {
        int xCount = position.y % 2 == 0 ? Width : Width - 1;

        if (position.x >= xCount || position.x < 0)
            return;

        if (position.y >= Lines || position.y < 0)
            return;

        if (grid[position.x, position.y] == color || grid[position.x, position.y] == BubbleColor.Explored)
            return;

        if (BubblesGrid[position.x, position.y] != null && BubblesGrid[position.x, position.y].Color == color)
        {
            grid[position.x, position.y] = color;
        }
        else
        {
            grid[position.x, position.y] = BubbleColor.Explored;
            return;

        }

        if (position.y % 2 == 0)
        {
            FillNeighbourColor(grid, color, position + new Vector2Int(-1, -1));
            FillNeighbourColor(grid, color, position + new Vector2Int(0, -1));
            FillNeighbourColor(grid, color, position + new Vector2Int(-1, 0));
            FillNeighbourColor(grid, color, position + new Vector2Int(1, 0));
            FillNeighbourColor(grid, color, position + new Vector2Int(-1, 1));
            FillNeighbourColor(grid, color, position + new Vector2Int(0, 1));
        }
        else
        {
            FillNeighbourColor(grid, color, position + new Vector2Int(0, -1));
            FillNeighbourColor(grid, color, position + new Vector2Int(1, -1));
            FillNeighbourColor(grid, color, position + new Vector2Int(-1, 0));
            FillNeighbourColor(grid, color, position + new Vector2Int(1, 0));
            FillNeighbourColor(grid, color, position + new Vector2Int(0, 1));
            FillNeighbourColor(grid, color, position + new Vector2Int(1, 1));
        }

    }

    public void Win()
    {
        Debug.Log(PlayersList[0].playerName + " a gagné");
    }
    public void KillPlayer()
    {

    }

}
