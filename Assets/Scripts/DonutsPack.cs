using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonutsPack : MonoBehaviour
{

    internal const float distBetweenDonuts = 0.25f;
    internal Donut[] donuts;

    public void DonutsInit(Donut[] newDonuts)
    {
        donuts = new Donut[3];
        for (int i = 0; i < newDonuts.Length;i++)
        {
            donuts[i] = newDonuts[i];
            donuts[i].transform.parent = transform;
            donuts[i].transform.position = transform.position + new Vector3(0, distBetweenDonuts * i, 0);
            donuts[i].currentPack = this;
        }
        ShakeInit();
    }

    //donuts checking
    public Donut[] GetUpperSameColorElements(int colorId, out int anotherDonutsBelow)
    {
        Stack<Donut> foundDonuts = new Stack<Donut>();
        for (int num = 2; num >= 0 ; num--)
        {
            if (donuts[num].colorId == colorId)
            {
                foundDonuts.Push(donuts[num]);
            }
            else
            {
                break;
            }
        }
        anotherDonutsBelow = 3 - foundDonuts.Count;
        return foundDonuts.ToArray();
    }

    public int GetUpperColorId()
    {
        for (int num = 2; num >= 0; num--)
        {
            if (donuts[num] != null)
            {
                return donuts[num].colorId;
            }
        }
        return -1;
    }

    //donuts manage


    //safe manage(for best matches founding algoritm)
    internal Donut[] safeRemoveDonutsWithColor(int colorId)
    {

        List<Donut> removed = new List<Donut>();
        for (int i = 2; i >= 0;i--)
        {
            if (donuts[i] != null)
            {
                if (donuts[i].colorId == colorId)
                {
                    removed.Add(donuts[i]);
                    donuts[i] = null;
                }
                else if (removed.Count > 0)
                {
                    break;
                }
            }
        }

        return removed.ToArray();
    }

    internal Donut[] safeRemoveDonutsCount(int count)
    {

        List<Donut> removed = new List<Donut>();
        for (int i = 2; i >= 0; i--)
        {
            if (donuts[i] != null)
            {
                removed.Add(donuts[i]);
                count--;
                donuts[i] = null;
                if (count <= 0)
                {
                    break;
                }

            }

        }

        return removed.ToArray();
    }

    internal bool safeDestroyCheck(out int theeMatch)
    {
        theeMatch = 0;
        int colorId = -1;
        int sameColorCounter = 1;
        if (donuts[2] != null)
        {
            colorId = donuts[2].colorId;
        }

        for (int i = 1; i >= 0; i--)
        {
            if (donuts[i] != null)
            {
                if (colorId == -1)
                {
                    return false;
                }
                else if (donuts[i].colorId == colorId)
                {
                    sameColorCounter++;
                }
            }
        }
        if (sameColorCounter != 3)
        {
            if (colorId != -1)
            {
                return false;
            }

        }
        else
        {
            theeMatch = 3;
        }
        return true;
    }

    internal int getSameColorElementsCount(int colorId)
    {
        int counter = 0;
        for (int i = 2; i >= 0; i--)
        {
            if (donuts[i] != null && donuts[i].colorId == colorId)
            {
                counter++;
            }
            else if(counter > 0)
            {
                break;
            }
        }
        return counter;
    }

    internal int getEmptySpaces()
    {
        int counter = 0;
        for (int i = 2; i >= 0; i--)
        {
            if (donuts[i] == null)
            {
                counter++;
            }
            else
            {
                break;
            }
        }
        return counter;
    }

    internal void safeAddDonuts(Donut[] donutsAdd)
    {
        if (donutsAdd.Length == 0)
        {
            return;
        }
        int num = 0;
        for (int i = 0; i < 3; i++)
        {
            if (donuts[i] == null)
            {
                donuts[i] = donutsAdd[num];
                num++;
                if (num >= donutsAdd.Length)
                {
                    break;
                }
            }
        }
    }

    public bool onlyOneColorCheck(int colorId)
    {
        bool oneColor = false;
        for (int i = 2; i >= 0; i--)
        {
            if (donuts[i] != null && donuts[i].colorId == colorId)
            {
                oneColor = true;
            }
            else if (oneColor == true)
            {
                return false;
            }
        }
        return oneColor;
    }

    internal void HideStick()
    {
        gameObject.SetActive(false);
    }

    //shaking
    Vector3[] shakedPositions;
    Vector3[] defaultPoses;
    const float shakeDrag = 0.03f;

    public void ShakeInit()
    {
        shakedPositions = new Vector3[3];
        defaultPoses = new Vector3[3];
        for (int i = 0; i < 3;i++)
        {
            if (donuts[i] != null)
            {
                defaultPoses[i] = donuts[i].transform.position - transform.position;
                shakedPositions[i] += new Vector3(Random.Range(-shakeDrag, shakeDrag), 0, Random.Range(-shakeDrag, shakeDrag));
            }
            else
            {
                break;
            }
        }
    }

    public void Shake(float coef)
    {
        for (int i = 0; i < donuts.Length; i++)
        {
            if (donuts[i] != null)
            {
                donuts[i].transform.localPosition = defaultPoses[i] + Vector3.Lerp(new Vector3(), shakedPositions[i], coef);
            }
            else
            {
                break;
            }
        }
    }

    public GameObject particleSys;

    public void removeAndDestroy()
    {
        Destroy(gameObject);
        if (gameObject.activeSelf == true)
        {
            particleSys = Instantiate(particleSys);
            particleSys.transform.position = transform.position;
            globalSettings.instance.audio.Play();
        }

    }

}
