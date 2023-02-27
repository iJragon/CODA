// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity {
    public class MultiHandLandmarkListAnnotationController : AnnotationController<MultiHandLandmarkListAnnotation> {
        [SerializeField] private bool _visualizeZ = false;
        [SerializeField] private HandDetection detection;


        private IList<NormalizedLandmarkList> _currentHandLandmarkLists;
        private IList<ClassificationList> _currentHandedness;
        public static int handCount;

        public void DrawNow(IList<NormalizedLandmarkList> handLandmarkLists, IList<ClassificationList> handedness = null) {
            _currentHandLandmarkLists = handLandmarkLists;
            _currentHandedness = handedness;
            SyncNow();
        }

        public void DrawLater(IList<NormalizedLandmarkList> handLandmarkLists) {
            UpdateCurrentTarget(handLandmarkLists, ref _currentHandLandmarkLists);
        }

        public void DrawLater(IList<ClassificationList> handedness) {
            UpdateCurrentTarget(handedness, ref _currentHandedness);
        }

        protected override void SyncNow() {
            isStale = false;
            annotation.Draw(_currentHandLandmarkLists, _visualizeZ);

            if (_currentHandedness != null) {
                annotation.SetHandedness(_currentHandedness);
            }
            if (_currentHandedness != null) {
                if (_currentHandedness.Count == 2) {
                    HandDetection.hands["left"] = true;
                    HandDetection.hands["right"] = true;
                } else {
                    if (_currentHandedness[0].Classification[0].Label.Equals("Left")) {
                        HandDetection.hands["left"] = true;
                        HandDetection.hands["right"] = false;
                    } else if (_currentHandedness[0].Classification[0].Label.Equals("Right")) {
                        HandDetection.hands["left"] = false;
                        HandDetection.hands["right"] = true;
                    }
                }
                handCount = _currentHandedness.Count;
            } else {
                HandDetection.hands["left"] = false;
                HandDetection.hands["right"] = false;
            }

            _currentHandedness = null;

            detection.DetectSymbol(_currentHandLandmarkLists);
            
        }
    }
}