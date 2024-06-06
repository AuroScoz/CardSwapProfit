using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Scoz.Func {
    [Serializable]
    public struct BloomSetting {
        public float Threshold;
        public float Intensity;
        public Color TintColor;
    }

    public class PostProcessingManager : MonoBehaviour {
        private Volume _myVolume;


        public static PostProcessingManager Instance;

        public enum PlayEffectType { DarkStorm }

        public void Start() {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _myVolume = GetComponent<Volume>();

        }


        private void OnDestroy() {

        }
        public Bloom GetBloom() {
            if (_myVolume == null) return null;
            Bloom bloom;
            _myVolume.profile.TryGet(out bloom);
            return bloom;
        }
        public ColorAdjustments GetColorAdjustments() {
            if (_myVolume == null) return null;
            ColorAdjustments colorAdjustments;
            _myVolume.profile.TryGet(out colorAdjustments);
            return colorAdjustments;
        }
        public void SetBrightness(float _value) {
            _value = Mathf.Clamp(_value, 0, 1);
            var intensity = (_value - 0.5f) * 0.2f / 0.5f;//亮度調整用HDR白色的Intensity在-0.2~0.2之間
            var colorAdjustments = GetColorAdjustments();
            if (colorAdjustments == null) return;
            float factor = Mathf.Pow(2, intensity);
            Color color = new Color(1 * factor, 1 * factor, 1 * factor);
            colorAdjustments.colorFilter.value = color;
        }
        public void SetChromaticAberrationDecayEffect(float _intensity, float _duration) {
            if (_myVolume == null) return;

            ChromaticAberration chromaticAberration;
            _myVolume.profile.TryGet(out chromaticAberration);
            if (chromaticAberration == null) return;

            UniTask.Void(async () => {
                chromaticAberration.intensity.value = _intensity;
                float interval = 0.04f;
                float decay = -_intensity / (_duration / interval);
                chromaticAberration.intensity.value = _intensity;
                while (_duration > 0) {
                    await UniTask.Delay(TimeSpan.FromSeconds(interval));
                    chromaticAberration.intensity.value += decay;
                    _duration -= interval;
                }
                chromaticAberration.intensity.value = 0;
            });
        }

    }
}