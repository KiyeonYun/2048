using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GameObject[] numbers = new GameObject[17];

    int x, y;
    bool wait;
    Vector3 firstPos, gap;
    GameObject[,] Square = new GameObject[4, 4];

    Animator anim;

    private void Start()
    {
        numbers = Resources.LoadAll("Prefabs", typeof(GameObject)).Cast<GameObject>().ToArray();
        Debug.Log("numbers: " + numbers.Length);

        anim = GetComponent<Animator>();

        Spawn();
        Spawn();
    }

    private void Update()
    {
        /* 뒤로가기 */
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        /* 모바일과 PC에서 모두 동작하도록 */
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
                }
                else if (gap.y < 0f && gap.x > -0.5f && gap.x < 0.5f)
                {
                    // Down
                    Debug.Log("Down");
                }
                else if (gap.x > 0f && gap.y > -0.5f && gap.y < 0.5f)
                {
                    // Right
                    Debug.Log("Right");
                }
                else if (gap.x < 0f && gap.y > -0.5f && gap.y < 0.5f)
                {
                    // Left
                    Debug.Log("Left");
                }
                else return;
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

    void Spawn()
    {
        while (true)
        {
            x = Random.Range(0, 4);
            y = Random.Range(0, 4);
            if (Square[x, y] == null)
                break;
        }
        // Debug.Log("x: " + x + ", y: " + y);

        // 1/8 확률로 4를 스폰, 나머지는 2를 스폰
        Square[x, y] = Instantiate(
            Random.Range(0, 8) > 0 ? numbers[0] : numbers[1],
            new Vector3(x * 1.2f + -1.8f, y * 1.2f + -1.8f, 0),
            Quaternion.identity
            );

        Square[x, y].GetComponent<Animator>().SetTrigger("Spawn");

    }

    /* 게임 재시작 */
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
