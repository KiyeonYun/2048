using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving : MonoBehaviour
{
    bool move, _combine;
    int _x2, _y2;

    private void Update()
    {
        if (move) Move(_x2, _y2, _combine);
        if (transform.position == new Vector3(_x2 * 1.2f + -1.8f, _y2 * 1.2f + -1.8f, 0.3f))
            move = false;
    }

    public void Move(int x2, int y2, bool combine)
    {
        move = true;
        _combine = combine;
        _x2 = x2;
        _y2 = y2;
        transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(x2 * 1.2f + -1.8f, y2 * 1.2f + -1.8f, 0.5f),
                10000
                );
        if (combine)
        {
            _combine = false;
            Destroy(gameObject);
        }
    }
}
