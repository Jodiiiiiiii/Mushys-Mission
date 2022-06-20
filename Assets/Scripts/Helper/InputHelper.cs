using UnityEngine;

public static class InputHelper
{
    /// <summary>
    /// Returns if a right key is held while a left key is not (supports WASD and arrow keys)
    /// </summary>
    /// <returns></returns>
    public static bool GetRightOnly()
    {
        // if right direction is pressed
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            // if left direction is not pressed
            if(!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.A))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns if a left key is held while a right key is not (supports WASD and arrow keys)
    /// </summary>
    /// <returns></returns>
    public static bool GetLeftOnly()
    {
        // if right direction is pressed
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            // if left direction is not pressed
            if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.D))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns if an up key is held (supports WASD, arrow keys, and space bar)
    /// </summary>
    /// <returns></returns>
    public static bool GetUp()
    {
        // if up directional is held
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns if an up key is pressed (supports WASD, arrow keys, and space bar)
    /// </summary>
    /// <returns></returns>
    public static bool GetUpPress()
    {
        // if up directional is pressed
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            return true;
        }
        return false;
    }
}
