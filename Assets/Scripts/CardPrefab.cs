using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPrefab : MonoBehaviour {
    [SerializeField] Image Img;
    public Toggle SelectToggle;
    [SerializeField] Image CoverImg;
    [SerializeField] ParticleSystem MatchEffect;


    public void SetImg(Sprite _sprite) {
        Img.sprite = _sprite;
    }
    public void EnableCoverImg(bool _enable) {
        CoverImg.enabled = _enable;
    }
    public void PlayMatchEffect() {
        MatchEffect.Play();
    }


}
