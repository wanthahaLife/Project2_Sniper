using System;
using System.Collections;
using UnityEngine;

public class HitPosition : MonoBehaviour, IHiitable
{
    float colorChangeTime = 0.2f;
    public Action<float, AttackType> onHit;
    MeshRenderer mesh;
    WaitForSeconds colorChangeWaitTime;
    Color basicColor;
    Color hitColor;
    Color pursuitColor;

    private void Awake()
    {
        mesh = GetComponentInChildren<MeshRenderer>();
        colorChangeWaitTime = new WaitForSeconds(colorChangeTime);
        basicColor = mesh.material.color;
        hitColor = new Color(1.0f, 0f, 0f, basicColor.a);
        pursuitColor = new Color(1.0f, 0.498f, 0f, basicColor.a);

    }

    public void HandleHit(float damage, AttackType attackType = AttackType.Normal)
    {
        StartCoroutine(ColorChange());
        onHit?.Invoke(damage, attackType);
    }

    public void SetPursuitColor()
    {
        mesh.material.color = pursuitColor;
    }

    public void SetBasicColor()
    {
        mesh.material.color = basicColor;
    }

    IEnumerator ColorChange()
    {
        mesh.material.color = hitColor;
        yield return colorChangeWaitTime;
        mesh.material.color = pursuitColor;
    }
}
