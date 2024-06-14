using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpotlightDetector : MonoBehaviour
{
    [SerializeField] private Transform monsterTransform;
    [SerializeField] private Light spotlight;

    public bool isIlluminated = false;
    void Update()
    {
        isIlluminated = IsInSpotlight();
    }

    bool IsInSpotlight()
    {
        if (Vector3.Distance(spotlight.transform.position, monsterTransform.position) > spotlight.range)
            return false;

        Vector3 directionToTarget = monsterTransform.position - spotlight.transform.position;
        float angle = Vector3.Angle(spotlight.transform.forward, directionToTarget);

        if (angle > spotlight.spotAngle / 2)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(spotlight.transform.position, directionToTarget, out hit, spotlight.range))
        {
            if (hit.transform == monsterTransform)
            {
                return true;
            }
        }
        return false;
    }
}
