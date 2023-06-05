using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Game : MonoBehaviour
{
    [SerializeField] TextMeshPro minesText;

    [SerializeField, Min(1)]
    int rows = 8, columns = 21;

    [System.Flags]
    public enum CellState
    {   // Grid is hexagonal, max 6 neighbours 
        Zero, One, Two, Three, Four, Five, Six,
        // store this information in the unused bits of CellState.   |
        // Values up to six require three bits,                      |
        // so we can use the fourth bit as a flag to indicate a mine V
        Mine = 1 << 3,
        MarkedSure = 1 << 4,
        MarkedUnsure = 1 << 5,
        Revealed = 1 << 6,
        // combined masks to make checking the cell states easier
        Marked = MarkedSure | MarkedUnsure,
        MarkedOrRevealed = Marked | Revealed,
        MarkedSureOrMine = MarkedSure | Mine
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
