using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GameState
{
    public int[,] board;
    public int score;

    public GameState(int[,] board, int score)
    {
        this.board = (int[,])board.Clone();
        this.score = score;
    }
}


