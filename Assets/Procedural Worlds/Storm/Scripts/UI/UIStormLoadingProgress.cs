using UnityEngine;
using UnityEngine.UI;
using ProceduralWorlds.Storm.Core;
using TMPro;

namespace ProceduralWorlds.Storm.UI
{
    public class UIStormLoadingProgress : MonoBehaviour
    {
        public GameObject[] DisableWhenLoading = new GameObject[0];
        public GameObject[] EnableWhenLoading = new GameObject[0];
        public TMP_Text ProgressText = null;
        public string ProgressTextString = "{0:P}";
        public Image ProgressSprite = null;
        public Slider ProgressSlider = null;

        private void OnEnable()
        {
            StormDataCache.OnBeginLoading -= BeginLoad;
            StormDataCache.OnBeginLoading += BeginLoad;
            StormDataCache.OnFinishedLoading -= FinishLoad;
            StormDataCache.OnFinishedLoading += FinishLoad;
            StormDataCache.OnLoadingUpdate -= UpdateLoad;
            StormDataCache.OnLoadingUpdate += UpdateLoad;
            if (StormDataCache.IsLoading)
                BeginLoad();
            else
                FinishLoad();
            if (ProgressSprite != null && ProgressSprite.sprite == null)
            {
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.SetPixels32(new Color32[] { Color.white, Color.white, Color.white, Color.white });
                tex.Apply(false, true);
                ProgressSprite.sprite = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f, 0.5f));
                ProgressSprite.type = Image.Type.Filled;
            }

        }

        private void OnDisable()
        {
            StormDataCache.OnBeginLoading -= BeginLoad;
            StormDataCache.OnFinishedLoading -= FinishLoad;
            StormDataCache.OnLoadingUpdate -= UpdateLoad;
        }

        private void UpdateLoad(float progress)
        {
            //Debug.Log("Updated Loading: " + progress);
            if (ProgressText != null)
            {
                if (string.IsNullOrWhiteSpace(ProgressTextString))
                    ProgressText.text = progress.ToString("P1");
                else
                    ProgressText.text = string.Format(ProgressTextString, progress);
            }
            if (ProgressSprite != null)
                ProgressSprite.fillAmount = progress;
            if (ProgressSlider != null)
                ProgressSlider.value = progress * ProgressSlider.maxValue;
        }

        private void FinishLoad()
        {
            //Debug.Log("Finished Loading");
            foreach (GameObject t in DisableWhenLoading)
                if (t != null)
                    t.SetActive(true);
            foreach (GameObject t in EnableWhenLoading)
                if (t != null)
                    t.SetActive(false);
        }

        private void BeginLoad()
        {
            //Debug.Log("Started Loading");
            foreach (GameObject t in DisableWhenLoading)
                if (t != null)
                    t.SetActive(false);
            foreach (GameObject t in EnableWhenLoading)
                if (t != null)
                    t.SetActive(true);
        }
    }
}