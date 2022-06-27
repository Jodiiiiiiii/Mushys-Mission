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
    /// Returns if an up key is just pressed (supports WASD, arrow keys, and space bar)
    /// </summary>
    /// <returns></returns>
    public static bool GetSpacePress()
    {
        // if up directional is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns if a shift key is just pressed (supports left or right shift)
    /// </summary>
    /// <returns></returns>
    public static bool GetShiftPress()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns octodirection formed by directional buttons held
    /// </summary>
    /// <returns></returns>
    public static OctoDirection GetOctoDirectionHeld()
    {
        // increment variables based on key presses
        int vertical = 0;
        int horizontal = 0;
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            vertical++;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            vertical--;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horizontal++;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horizontal--;

        // determine and return OctoDirection
        if (vertical == 1 && horizontal == 0)
            return OctoDirection.Up;
        else if (vertical == 1 && horizontal == 1)
            return OctoDirection.UpRight;
        else if (vertical == 0 && horizontal == 1)
            return OctoDirection.Right;
        else if (vertical == -1 && horizontal == 1)
            return OctoDirection.DownRight;
        else if (vertical == -1 && horizontal == 0)
            return OctoDirection.Down;
        else if (vertical == -1 && horizontal == -1)
            return OctoDirection.DownLeft;
        else if (vertical == 0 && horizontal == -1)
            return OctoDirection.Left;
        else if (vertical == 1 && horizontal == -1)
            return OctoDirection.UpLeft;
        else
            return OctoDirection.None;
    }

    /// <summary>
    /// Enumeration including eight directions and "None"
    /// </summary>
    public enum OctoDirection
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
        None
    }
}
