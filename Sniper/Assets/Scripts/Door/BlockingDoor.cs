using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingDoor : DoorBase
{
    public enum UserPosition : byte
    { 
        Up = 0,
        Down,
        Left,
        Right,
    }

    public UserPosition userPosition = UserPosition.Up;

    bool isActivate = true;

    protected override void OnEnterAction(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (isActivate && player != null)
        {
            base.OnEnterAction(other);
        }
    }
    protected override void OnExitAction(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            bool isClose = false;
            Vector3 position = player.transform.position;
            switch (userPosition)
            {
                case UserPosition.Up:
                    if (position.z > transform.position.z)
                    {
                        isClose = true;
                    }
                    break;
                case UserPosition.Down:
                    if (position.z < transform.position.z)
                    {
                        isClose = true;
                    }
                    break;
                case UserPosition.Left:
                    if (position.x > transform.position.x)
                    {
                        isClose = true;
                    }
                    break;
                case UserPosition.Right:
                    if (position.x < transform.position.x)
                    {
                        isClose = true;
                    }
                    break;
            }
            if (isClose)
            {
                base.OnExitAction(other);
                isActivate = false;
            }
        }
        
    }
}
