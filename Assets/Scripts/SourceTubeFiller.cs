using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceTubeFiller : MonoBehaviour
{
    public string fluidType;
    private Material fluidMat;
    private float lastFillTime;
    private float secondsUntilRefill;

    private float maxFill; 
    // Start is called before the first frame update
    void Start()
    {
        fluidMat = GetComponent<Renderer>().material;
        fluidMat.SetFloat("_FillAmount", 4f);
        fluidMat.SetFloat("_GlitterPercent", 0);
        fluidMat.SetFloat("_PoisonPercent", 0);
        fluidMat.SetFloat("_ColorPercent", 0);
        fluidMat.SetFloat("_RainbowPercent", 0);

        fluidMat.SetFloat(fluidType, 1);
        lastFillTime = Time.fixedTime;
        secondsUntilRefill = 10f;

        float scale = 1f / fluidMat.GetFloat("_Size");
        maxFill = scale * (0.5f + 1f) - 1f;

        if(fluidType == "_GlitterPercent" || fluidType == "_PoisonPercent"){
            fluidMat.SetColor("_Color", new Color(0, 0, 0, 0));
        }
    }

    IEnumerator FillTube()
    {
        float fillAmount = fluidMat.GetFloat("_FillAmount");
        while(fillAmount < maxFill)
        {
            fillAmount += 0.5f*Time.deltaTime;
            fluidMat.SetFloat("_FillAmount", fillAmount);
            yield return null;
        }
        yield return 0;
    }



    // Update is called once per frame
    void Update()
    {

        if (Time.fixedTime- lastFillTime > secondsUntilRefill)
        {
            lastFillTime = Time.fixedTime;
            StartCoroutine("FillTube");
        }
    }
}
