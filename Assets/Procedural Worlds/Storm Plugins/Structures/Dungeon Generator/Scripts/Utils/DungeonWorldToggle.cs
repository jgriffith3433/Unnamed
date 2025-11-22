using System;
using UnityEngine;

public class DungeonWorldToggle : MonoBehaviour
{

    public LayerMask playerLayer;
    public bool worldActive = true;

    public static event EventHandler<OnWorldActivatedArgs> OnWorldActivated;
    public class OnWorldActivatedArgs : EventArgs
    {
        public GameObject activator;
        public OnWorldActivatedArgs(GameObject activator)
        {
            this.activator = activator;
        }
    }

    public static event EventHandler<OnWorldDeactivatedArgs> OnWorldDeactivated;
    public class OnWorldDeactivatedArgs : EventArgs
    {
        public GameObject deactivator;

        public OnWorldDeactivatedArgs(GameObject deactivator)
        {
            this.deactivator = deactivator;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if((playerLayer & (1 << other.gameObject.layer)) != 0)
        {
            worldActive = !worldActive;
            if (worldActive)
            {
                OnWorldActivated?.Invoke(this, new OnWorldActivatedArgs(other.gameObject));
            }
            else
            {
                OnWorldDeactivated?.Invoke(this, new OnWorldDeactivatedArgs(other.gameObject));
            }
        }
    }
}
