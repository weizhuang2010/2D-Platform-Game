using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : UnitySingleton<GameController>
{
    public PlayerController player;
    public GameObject wallBlocks;
    public LayerMask GroundLayer;
    public LayerMask WallLayer;
    public LayerMask MonkeyBarsLayer;

    private void FixedUpdate()
    {
        if (player.groundState == GroundState.ONWALL)
        {
            wallBlocks.SetActive(true);
        }
        // else
        // {
        //     wallBlocks.SetActive(false);
        // }
    }
}
