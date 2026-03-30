using System.Collections.Generic;
using UnityEngine;

public class RankPoint : MonoBehaviour
{
    float[] _times;

    int _currNumOfItems;
    readonly int MAX_SIZE = 5;

    private void Start()
    {
        _times = new float[MAX_SIZE];
        _currNumOfItems = 0;
    }

    public void AddTimeToList(float time)
    {
        if (_currNumOfItems < MAX_SIZE)
        {
            _times[_currNumOfItems++] = time;
        } else
        {
            return;
        }
    }
}
