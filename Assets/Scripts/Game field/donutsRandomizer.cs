using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
internal class donutsRandomizer
{
    public Color[] donutColors;

    public GameObject donutPrefab;
    public GameObject donutPackPrefab;

    int[] colorsGenerated;//donut elevation, colorId
    public int maxDonutsCountInPack = 3;

    public void Initialization()
    {
        colorsGenerated = new int[donutColors.Length];
    }

    public DonutsPack getRandomDonutPack()
    {
        DonutsPack newPack = Object.Instantiate(donutPackPrefab).GetComponent<DonutsPack>();

        int donutsCount = Random.Range(1, maxDonutsCountInPack + 1);
        Color[] colors = new Color[donutsCount];
        int[] colorIds = new int[donutsCount];
        switch (0)
        {
            case 0:
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = getRandomColor(out colorIds[i]);
                }
                goto case 1;
            //3 same colorCheck
            case 1:
                if (donutsCount == 3)
                {
                    int etalon = colorIds[2];
                    for (int i = 1; i < colorIds.Length; i++)
                    {
                        if (etalon != colorIds[i])
                        {
                            goto case 2;
                        }
                    }
                }
                else
                {
                    goto case 2;
                }
                goto case 0;
            //making pack
            case 2:
                Donut[] donuts = new Donut[donutsCount];
                for (int i = 0; i < donutsCount; i++)
                {
                    donuts[i] = Object.Instantiate(donutPrefab).GetComponent<Donut>();
                    donuts[i].GetComponent<MeshRenderer>().material.color = colors[i];
                    donuts[i].colorId = colorIds[i];
                    colorsGenerated[colorIds[i]]++;
                }
                newPack.DonutsInit(donuts);
                break;
        }





        return newPack;
    }

    Color getRandomColor(out int colorId)
    {
        colorId = Random.Range(0, donutColors.Length);
        return donutColors[colorId];
    }

    public Color getColorByID(int colorId)
    {
        return donutColors[colorId];
    }

}
