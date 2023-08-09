using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatPropChanger : MonoBehaviour
{
    public Material m_mat;
    public float min, max;
    public float seconds;
    public string property;

    private bool flip, canFlip;
    private float amount;

    void Start()
    {
        amount = 0;
        flip = true;
        canFlip = true;
    }

    void Update()
    {
        if (canFlip)
        {
            if (flip)
                amount += Time.deltaTime / seconds;
            else
                amount -= Time.deltaTime / seconds;

            if (amount >= max || amount <= min)
            {
                canFlip = false;
                StartCoroutine(FlipCO());
            }
        }

        amount = Mathf.Clamp01(amount);
        m_mat.SetFloat(property, amount);
    }

    private IEnumerator FlipCO()
    {
        if (!flip)
            yield return new WaitForSeconds(2f);
        else
            yield return new WaitForEndOfFrame();

        flip = !flip;
        canFlip = true;
    }
}
