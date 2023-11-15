using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class GameManager : MonoBehaviour
{
    public GameObject[] numbers = new GameObject[17];
    GameObject quitUI;
    [SerializeField] Text scoreTxt, bestScoreTxt, plusTxt;

    int x, y;
    int i, j, k, l;  // k: 빈칸의 개수
    int score;
    bool wait, move, isGameOver;
    Vector3 firstPos, gap;
    GameObject[,] Square = new GameObject[4, 4];

    private void Start()
    {
        numbers = Resources.LoadAll("Prefabs", typeof(GameObject)).Cast<GameObject>().ToArray();
        Debug.Log("numbers: " + numbers.Length);
        SortNumbers();

        quitUI = GameObject.Find("Quit");
        quitUI.SetActive(false);
        bestScoreTxt.text = PlayerPrefs.GetInt("BestScore").ToString();

        Spawn();
        Spawn();
    }

    private void Update()
    {
        /* 뒤로가기 */
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (isGameOver) return;

        /* 모바일과 PC에서 드래그 감지 */
        if (Input.GetMouseButtonDown(0))
        {
            firstPos = Input.mousePosition;
            wait = true;
        }
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            firstPos = Input.GetTouch(0).position;
            wait = true;
        }

        if (Input.GetMouseButton(0))
        {
            gap = Input.mousePosition - firstPos;
            if (gap.magnitude < 100) return;
            gap.Normalize();

            if (wait)
            {
                wait = false;
                if (gap.y > 0f && gap.x > -0.5f && gap.x < 0.5f)
                {
                    // Up
                    Debug.Log("Up");
                    for (x = 0; x <= 3; x++)
                        for (y = 0; y <= 2; y++)
                            for (i = 3; i >= y + 1; i--)
                                MoveOrCombine(x, i - 1, x, i);
                }
                else if (gap.y < 0f && gap.x > -0.5f && gap.x < 0.5f)
                {
                    // Down
                    Debug.Log("Down");
                    for (x = 0; x <= 3; x++)
                        for (y = 3; y >= 1; y--)
                            for (i = 0; i <= y - 1; i++)
                                MoveOrCombine(x, i + 1, x, i);
                }
                else if (gap.x > 0f && gap.y > -0.5f && gap.y < 0.5f)
                {
                    // Right
                    Debug.Log("Right");
                    for (y = 0; y <= 3; y++)
                        for (x = 0; x <= 2; x++)
                            for (i = 3; i >= x + 1; i--)
                                MoveOrCombine(i - 1, y, i, y);
                }
                else if (gap.x < 0f && gap.y > -0.5f && gap.y < 0.5f)
                {
                    // Left
                    Debug.Log("Left");
                    for (y = 0; y <= 3; y++)
                        for (x = 3; x >= 1; x--)
                            for (i = 0; i <= x - 1; i++)
                                MoveOrCombine(i + 1, y, i, y);
                }
                else return;

                if (move)
                {
                    move = false;
                    Spawn();
                    k = 0;
                    l = 0;

                    UpdateScore();

                    for (x = 0; x <= 3; x++)
                        for (y = 0; y <= 3; y++)
                        {
                            if (Square[x, y] == null)
                            {
                                k++;
                                continue;
                            }
                            if (Square[x, y].tag.Contains("Combine")) Square[x, y].tag = "Untagged";
                        }
                    // Debug.Log("Empty Square: " + k);
                    if (k <= 0)
                    {
                        if (CheckSameNumberOnBorder())
                        {
                            isGameOver = true;
                            quitUI.SetActive(true);
                            return;
                        }
                    }
                }
            }
        }
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            gap = (Vector3)Input.GetTouch(0).position - firstPos;
            if (gap.magnitude < 100) return;
            gap.Normalize();

            if (wait)
            {
                wait = false;
                Debug.Log(gap);
            }
        }
    }

    void SortNumbers()
    {
        for (int i = 0; i < numbers.Length - 1; i++)
        {
            for (int j = i + 1; j < numbers.Length; j++)
            {
                if (Int32.Parse(Regex.Replace(numbers[i].name, @"\D", "")) > 
                    Int32.Parse(Regex.Replace(numbers[j].name, @"\D", "")))
                {
                    GameObject tempNum = numbers[j];
                    numbers[j] = numbers[i];
                    numbers[i] = tempNum;
                }
            }
        }
    }

    /* [x1, y1]: 이동 전 좌표
     * [x2, y2]: 이동할 좌표
     */
    void MoveOrCombine(int x1, int y1, int x2, int y2)
    {
        /* 이동 전 좌표에 값이 존재하고 이동할 좌표에 값이 없으면 이동 */
        if (Square[x2, y2] == null && Square[x1, y1] != null)
        {
            move = true;
            Square[x1, y1].GetComponent<Moving>().Move(x2, y2, false);
            Square[x2, y2] = Square[x1, y1];
            Square[x1, y1] = null;
        }
        /* 같은 숫자일 때 결합 */
        else if (Square[x1, y1] != null && Square[x2, y2] != null
            && Square[x1, y1].name == Square[x2, y2].name
            && !Square[x1, y1].tag.Contains("Combine") && !Square[x2, y2].tag.Contains("Combine"))
        {
            move = true;
            for (j = 0; j <= 16; j++)
                if (Square[x2, y2].name == numbers[j].name + "(Clone)") break;

            Square[x1, y1].GetComponent<Moving>().Move(x2, y2, true);
            Destroy(Square[x2, y2]);
            Square[x1, y1] = null;
            Square[x2, y2] = Instantiate(
                numbers[j + 1],
                new Vector3(x2 * 1.2f + -1.8f, y2 * 1.2f + -1.8f, 0),
                Quaternion.identity
                );
            Square[x2, y2].tag = "Combine";
            Square[x2, y2].GetComponent<Animator>().SetTrigger("Combine");
            score += (int)Mathf.Pow(2, j + 2);
            // scoreTxt.text = score.ToString();
        }
    }

    /* 새 숫자 생성 */
    void Spawn()
    {
        while (true)
        {
            x = UnityEngine.Random.Range(0, 4);
            y = UnityEngine.Random.Range(0, 4);
            if (Square[x, y] == null)
                break;
        }
        // Debug.Log("x: " + x + ", y: " + y);

        // 1/8 확률로 4를 스폰, 나머지는 2를 스폰
        Square[x, y] = Instantiate(
            UnityEngine.Random.Range(0, 8) > 0 ? numbers[0] : numbers[1],
            new Vector3(x * 1.2f + -1.8f, y * 1.2f + -1.8f, 0),
            Quaternion.identity
            );

        Square[x, y].GetComponent<Animator>().SetTrigger("Spawn");

    }

    void UpdateScore()
    {
        if (score > 0)
        {
            plusTxt.text = "+" + score.ToString() + "    ";
            plusTxt.GetComponent<Animator>().SetTrigger("PlusBack");
            plusTxt.GetComponent<Animator>().SetTrigger("Plus");
            scoreTxt.text = (int.Parse(scoreTxt.text) + score).ToString();
            if (PlayerPrefs.GetInt("BestScore", 0) < int.Parse(scoreTxt.text))
            {
                PlayerPrefs.SetInt("BestScore", int.Parse(scoreTxt.text));
            }
            bestScoreTxt.text = PlayerPrefs.GetInt("BestScore").ToString();
            score = 0;
        }
    }

    bool CheckSameNumberOnBorder()
    {
        for (y = 0; y <= 3; y++)
            for (x = 0; x < 2; x++)
                if (Square[x, y].name.Equals(Square[x + 1, y].name)) l++;
        for (x = 0; x <= 3; x++)
            for (y = 0; y <= 2; y++)
                if (Square[x, y].name.Equals(Square[x, y + 1].name)) l++;
        // Debug.Log("Same Square: " + l);

        if (l <= 0) return true;
        else return false;
    }

    /* 게임 재시작 */
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
