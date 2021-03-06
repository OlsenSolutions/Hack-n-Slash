﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public Vector3 targetPosition;
    public RectTransform pointerRectTransform;
    public Camera Camera;
    private void Awake()
    {
        pointerRectTransform = transform.Find("Point").GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (pointerRectTransform != null&&targetPosition!=null)
        {
            var targetPosLocal = Camera.transform.InverseTransformPoint(targetPosition);
            var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;
            pointerRectTransform.eulerAngles = new Vector3(0, 0, targetAngle);
        }
    }

}
