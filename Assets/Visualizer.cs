using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;
using Klak.TestTools;
using UnityEngine.Profiling;

namespace UltraFace
{
    public sealed class Visualizer : MonoBehaviour
    {
        [SerializeField] ImageSource _source = null;
        [SerializeField, Range(0, 1)] float _threshold = 0.5f;
        [SerializeField] ResourceSet _resources = null;
        [SerializeField] UI.RawImage _previewUI = null;
        [SerializeField] private UI.Text memoryText;

        FaceDetector _detector;

        private float previewWidth;
        private float previewHeight;
        private List<UI.RawImage> boxes = new();

        void Start()
        {
            _detector = new FaceDetector(_resources);
            RectTransform previewRect = _previewUI.rectTransform;
            previewWidth = previewRect.rect.width; //Preview height is fixed
            previewHeight = previewRect.rect.width; //Preview height is fixed
        }

        void Update()
        {
            _detector.ProcessImage(_source.Texture, _threshold);
            
            _previewUI.texture = _source.Texture;

            DestroyAllBoxes();
            VisualizeResults();
            ShowMemoryUsage();
        }

        #region Core functions
        void VisualizeResults()
        {
            foreach (var detection in _detector.Detections)
            {
                var box = CreateRedBox(_previewUI.transform); // Create new red box
                boxes.Add(box);
                RenderBox(detection, box); // Update the red box position and size based on the detection result.
            }
        }

        UI.RawImage CreateRedBox(Transform parent)
        {
            GameObject redBox = new GameObject("RedBox", typeof(RectTransform), typeof(UI.RawImage));
            redBox.transform.SetParent(parent, false);
            // Get RectTransform and adjust settings
            RectTransform rect = redBox.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);

            UI.RawImage image = redBox.GetComponent<UI.RawImage>();
            image.color = Color.red; // Set red color

            return image;
        }

        void RenderBox(Detection detection, UI.RawImage box)
        {
            if (_previewUI == null) return;
            
            box.gameObject.SetActive(true); //Display the box
            float x1 = detection.x1;
            float y1 = detection.y1;
            float x2 = detection.x2;
            float y2 = detection.y2;

            float posX = x1 * previewWidth;
            float posY = -y1 * previewHeight;
            float w = (x2 - x1) * previewWidth;
            float h = (y2 - y1) * previewHeight;


            // Set the new box position
            RectTransform boxRect = box.rectTransform;
            boxRect.anchoredPosition = new Vector2(posX, posY);
            boxRect.sizeDelta = new Vector2(w, h);
        }

        #endregion

        void DestroyAllBoxes()
        {
            foreach (var box in boxes)
            {
                Destroy(box.gameObject);
            }

            boxes.Clear();
        }

        void ShowMemoryUsage()
        {
            memoryText.text = $"Total Allocated Memory: {Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)} MB";
        }
        
        void OnDestroy()
        {
            _detector?.Dispose();
        }
    }
} // namespace UltraFace