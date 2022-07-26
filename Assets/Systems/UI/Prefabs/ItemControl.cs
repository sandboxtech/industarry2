
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace W
{
    public class ItemControl : MonoBehaviour
	{

        [SerializeField]
        private RectTransform rect;
        [SerializeField]
        private RectTransform textRect;

        [Space]
        [SerializeField]
        private Text text;
        public Text Text => text;

        [SerializeField]
        private Image icon;

        private Func<string> TextDynamic;
        private IEnumerator TextCoroutine() {
            while (true) {
                Text.text = TextDynamic.Invoke();
                yield return null;
            }
        }

        public void EnableSpace(ref Item item) {
            Text.gameObject.SetActive(false);
        }


        private const float textMarginX = 16;
        private const float textMargetY = 16;
        public void EnableText(ref Item item) {
            float textRectSizeDeltaX = textRect.sizeDelta.x;
            if (item.Icon != null) {
                const float movement = 24;
                textRectSizeDeltaX -= movement;
                textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x + movement, textRect.anchoredPosition.y);

                icon.sprite = item.Icon;
                if (item.IconColorDefined) {
                    icon.color = item.IconColor;
                }

                icon.gameObject.SetActive(true);

                Text.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            else {
                Text.horizontalOverflow = HorizontalWrapMode.Wrap;
            }

            string text = item.Text;
            Func<string> textDynamic = item.TextDynamic;
            if (text != null) {
                Text.text = text;
                if (textDynamic == null) {
                    float preferredHeight = Mathf.Floor(Text.preferredHeight / 2) * 2;
                    textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, preferredHeight);
                    rect.sizeDelta = new Vector2(textRect.sizeDelta.x + textMarginX, textRect.sizeDelta.y + textMargetY);
                }
            } else if (textDynamic != null) {
                TextDynamic = textDynamic;
                StartCoroutine(TextCoroutine());
                if (text != null) {
                    Text.text = text;
                } else {
                    Text.text = textDynamic();
                }
            }

            if (item.TextColorDefined) {
                Text.color = item.TextColor;
            }
        }


        [SerializeField]
        private Image back;
        public Image Back => back;

        [SerializeField]
        private Image front;
        public Image Front => front;


        [SerializeField]
        private Button button;
        public Button Button => button;

        private Action ButtonAction;
        public void ButtonCallback() {
            Audio.I.Clip = Audio.I.DefaultSound;
            ButtonAction?.Invoke();
        }

        public void EnableButton(ref Item item) {
            Action buttonAction = item.Button;
            ButtonAction = buttonAction;
            button.gameObject.SetActive(true);
            back.gameObject.SetActive(true);

            if (item.TextColorDefined) {
                back.color = item.TextColor;
            }
        }



        [SerializeField]
        private Slider slider;
        public Slider Slider => slider;

        public Action<float> sliderAction;
        public void SliderCallback() {
            sliderAction?.Invoke(slider.value);
        }

        public void EnableSlider(ref Item item) {
            Action<float> sliderAction = item.Slider;
            this.sliderAction = sliderAction;

            Slider.GetComponent<Image>().raycastTarget = true;
            Slider.interactable = true;
            back.gameObject.SetActive(true);
            front.gameObject.SetActive(true);
            slider.gameObject.SetActive(true);
        }

        public void EnableProgress(ref Item item) {
            Func<float> progress = item.Progress;
            ProgressDynamic = progress;
            StartCoroutine(ProgressCoroutine());

            Slider.GetComponent<Image>().raycastTarget = false;
            Slider.interactable = false;
            back.gameObject.SetActive(true);
            front.gameObject.SetActive(true);
            slider.gameObject.SetActive(true);
        }

        private Func<float> ProgressDynamic;
        private IEnumerator ProgressCoroutine() {
            while (true) {
                slider.value = ProgressDynamic.Invoke();
                yield return null;
            }
        }
    }
}
