/*
 * Copyright (c) 2017 Gaël Vanhalst
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Tools._2DObjectPlacer
{
    class ObjectPlacerInput
    {
        private const float DefaultRotationAngle = 10.0f;
        private const float AltRotationValueMultiplier = 0.2f;
        private const float DefaultScaleValue = 1f;
        private const float AltScaleValueMultiplier = 0.1f;
        private readonly ObjectPlacer _objectPlacer;

        private bool _rightMouseDown = false;

        public ObjectPlacerInput(ObjectPlacer objectPlacer)
        {
            _objectPlacer = objectPlacer;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void Disable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            //Fix for left click up not working
            if (Event.current.type == EventType.layout && _objectPlacer.PlaceMode)
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));


            Event currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.MouseUp:
                    ProcessMouseClickUp(currentEvent);
                    break;
                case EventType.MouseDown:
                    ProcessMouseClickDown(currentEvent);
                    break;
                case EventType.MouseDrag:
                case EventType.MouseMove:
                    ProcessMouseMovement(sceneView, currentEvent);
                    break;
                case EventType.ScrollWheel:
                    ProcessScroll(currentEvent, sceneView);
                    break;
                case EventType.KeyDown:
                    ProcessKeyDown(currentEvent);
                    break;
            }
        }

        private void ProcessMouseClickDown(Event currentEvent)
        {
            switch (currentEvent.button)
            {
                case 0:
                    break;
                case 1:
                    if (_objectPlacer.PlaceMode)
                    {
                        currentEvent.Use();
                    }
                    _rightMouseDown = true;
                    break;
            }
        }

        private void ProcessMouseClickUp(Event currentEvent)
        {
            switch (currentEvent.button)
            {
                case 0:
                    if (_objectPlacer.HasPreviewObject)
                    {
                        _objectPlacer.PlaceObject();
                        currentEvent.Use();
                    }
                    break;
                case 1:
                    if (_objectPlacer.PlaceMode)
                    {
                        currentEvent.Use();
                    }
                    _rightMouseDown = false;
                    break;
                case 2:
                    if (_objectPlacer.HasPreviewObject)
                    {
                        if (currentEvent.control)
                        {
                            _objectPlacer.MirrorPreviewObject();
                            currentEvent.Use();
                        }
                        else if(_rightMouseDown)
                        {
                            _objectPlacer.SelectNextPrefabInLine();
                            currentEvent.Use();
                        }
                    }
                    break;
            }
        }

        private void ProcessMouseMovement(SceneView sceneView, Event currentEvent)
        {
            if (_objectPlacer.HasPreviewObject)
            {
                Vector2 mousePos = currentEvent.mousePosition;
                mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
                mousePos = sceneView.camera.ScreenToWorldPoint(mousePos);
                _objectPlacer.SetPositionObject(mousePos);
                sceneView.Repaint();
            }
        }

        private void ProcessScroll(Event currentEvent, SceneView sceneView)
        {
            if (currentEvent.control)
            {
                if (_objectPlacer.HasPreviewObject)
                {
                    float angle = -DefaultRotationAngle * Mathf.Sign(currentEvent.delta.y);
                    if (currentEvent.alt)
                    {
                        angle *= AltRotationValueMultiplier;
                    }
                    _objectPlacer.RotatePreviewObject(angle);

                    currentEvent.Use();
                }
            }
            else if (currentEvent.shift)
            {
                if (_objectPlacer.HasPreviewObject)
                {
                    float scaleValue = -DefaultScaleValue * Mathf.Sign(currentEvent.delta.y);
                    if (currentEvent.alt)
                    {
                        scaleValue *= AltScaleValueMultiplier;
                    }
                    _objectPlacer.ScalePreviewObject(scaleValue);

                    currentEvent.Use();
                }
            }
            else if (_rightMouseDown)
            {
                if (_objectPlacer.PlaceMode)
                {
                    if (currentEvent.delta.y < 0)
                    {
                        _objectPlacer.IndexSelectedPrefab--;
                    }
                    else
                    {
                        _objectPlacer.IndexSelectedPrefab++;
                    }
                    sceneView.Repaint();
                }
                currentEvent.Use();
            }
        }

        private void ProcessKeyDown(Event currentEvent)
        {
            switch (currentEvent.keyCode)
            {
                case KeyCode.R:
                    if (_objectPlacer.HasPreviewObject)
                    {
                        _objectPlacer.ResetPreviewObject();
                        currentEvent.Use();
                    }
                    break;
                case KeyCode.P:
                    _objectPlacer.PlaceMode = !_objectPlacer.PlaceMode;
                    currentEvent.Use();
                    break;
                case KeyCode.Escape:
                    if (_objectPlacer.PlaceMode)
                    {
                        _objectPlacer.PlaceMode = false;
                        currentEvent.Use();
                    }
                    break;
            }
        }
    }
}
