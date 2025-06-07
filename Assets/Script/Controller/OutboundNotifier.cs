using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using pattern;
using UnityEngine;

public class OutboundUnregister : INotifiedValue
{
    public Transform targetTransform;
}

public class OutboundRegister : INotifiedValue
{
    public Transform targetTransform;
    public Action<Camera> BoundCheckAction;
}

public class OutboundNotifier : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    List<OutboundRegister>  outBoundTargets = new();
    private List<int> unregistList = new();
    private void Awake()
    {
        NotifyLocator.RegistLocate<OutboundRegister>(RegistBoundCheckTransform);
        NotifyLocator.RegistLocate<OutboundUnregister>(UnRegistBoundCheckTransform);
    }

    private void OnDestroy()
    {
        NotifyLocator.UnRegistLocate<OutboundRegister>(RegistBoundCheckTransform);
        NotifyLocator.UnRegistLocate<OutboundUnregister>(UnRegistBoundCheckTransform);
    }

    private void LateUpdate()
    {
        foreach (OutboundRegister outbound in outBoundTargets)
        {
            if (isOutboundary(outbound.targetTransform.position))
            {
                outbound.BoundCheckAction?.Invoke(targetCamera);
            }    
        }

        foreach (int index in unregistList)
        {
            outBoundTargets.RemoveAt(index);
        }
        unregistList.Clear();
    }

    public bool isOutboundary(Vector3 position) 
    {
        Vector3 worldPos = position;
        Vector3 viewPos = targetCamera.WorldToViewportPoint(worldPos);
        if (viewPos.x > 1 || viewPos.x < 0 || viewPos.y > 1 || viewPos.y < 0)
            return true;
        return false;
    }

    private void RegistBoundCheckTransform(INotifiedValue notifiedValue)
    {
        OutboundRegister value = notifiedValue as OutboundRegister;
        if(value is not null)
            outBoundTargets.Add(notifiedValue as OutboundRegister);
    }
    private void UnRegistBoundCheckTransform(INotifiedValue notifiedValue)
    {
        OutboundUnregister value = notifiedValue as OutboundUnregister;
        if (value is not null)
        {
            int index = outBoundTargets.FindIndex(x => x.targetTransform == value.targetTransform);
            if (index != -1)
            {
                if(!unregistList.Contains(index))
                    unregistList.Add(index);
            }
        }
    }
}
