using UnityEngine;

public class ShurikenManager : MonoBehaviour
{
    public ShurikenBaseClass[] shurikenPrefabs;
    public int[] shurikenCounts;
    public int selectedIndex = 0;

    public ShurikenBaseClass GetSelectedShurikenPrefab()
    {
        return shurikenPrefabs[selectedIndex];
    }

    public bool CanThrowSelected()
    {
        return shurikenCounts[selectedIndex] > 0;
    }

    public void RemoveShuriken()
    {
        if (shurikenCounts[selectedIndex] > 0)
            shurikenCounts[selectedIndex]--;
    }

    public void AddShuriken(int index, int count)
    {
        if (index >= 0 && index < shurikenCounts.Length)
            shurikenCounts[index] += count;
    }

    public void SelectShuriken(int index)
    {
        if (index >= 0 && index < shurikenCounts.Length && shurikenCounts[index] > 0)
            selectedIndex = index;
    }

    public void CycleShuriken()
    {
        int originalIndex = selectedIndex;
        do
        {
            selectedIndex = (selectedIndex + 1) % shurikenCounts.Length;
        }
        while (shurikenCounts[selectedIndex] == 0 && selectedIndex != originalIndex);
    }
}
