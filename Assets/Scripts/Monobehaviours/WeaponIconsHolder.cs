using UnityEngine;
using UnityEngine.UI;

public class WeaponIconsHolder : MonoBehaviour
{
    public Sprite[] Icons;
    public Image Img;

    private void Start() => Img = GetComponent<Image>();

    public void SetIcon(int i)
    {
        Img.sprite = Icons[i];
    }
}
