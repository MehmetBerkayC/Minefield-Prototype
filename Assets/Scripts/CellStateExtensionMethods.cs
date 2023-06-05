using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game;

public static class CellStateExtensionMethods
{
    public static bool Is(this CellState s, CellState mask) => (s & mask) != 0;

    public static bool IsNot(this CellState s, CellState mask) => (s & mask) == 0;

    public static CellState With(this CellState s, CellState mask) => s | mask;

    public static CellState Without(this CellState s, CellState mask) => s & ~mask;
 
}
