using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NFHGame.DialogueSystem {
    public class TextVertexAnimator {
        public static bool UseUnscaledTime = true;

        public bool textAnimating = false;
        private bool _stopAnimating = false;
        private bool _stopAnimation = false;

        private readonly TMP_Text _textBox;

        public TextVertexAnimator(TMP_Text textBox) {
            _textBox = textBox;
        }

        private const float k_CharAnimTime = .3f;

        public IEnumerator AnimateTextIn(List<DialogueCommand> commands, string processedMessage, Action OnFinish) {
            _stopAnimation = false;
            textAnimating = true;
            float secondsPerCharacter = 1f / DialogueUtility.DefaultScrollSpeed;
            float timeOfLastCharacter = 0;

            TextAnimInfo[] textAnimInfo = SeparateOutTextAnimInfo(commands);
            TMP_TextInfo textInfo = _textBox.textInfo;
            for (int i = 0; i < textInfo.meshInfo.Length; i++) { //Clear the mesh 
                TMP_MeshInfo meshInfer = textInfo.meshInfo[i];
                if (meshInfer.vertices != null) {
                    for (int j = 0; j < meshInfer.vertices.Length; j++) {
                        meshInfer.vertices[j] = Vector3.zero;
                    }
                }
            }

            _textBox.text = processedMessage;
            _textBox.ForceMeshUpdate();

            if (_textBox.textInfo.characterCount == 0) {
                OnFinish?.Invoke();
                yield break;
            }

            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
            Color32[][] originalColors = new Color32[textInfo.meshInfo.Length][];
            for (int i = 0; i < originalColors.Length; i++) {
                Color32[] theColors = textInfo.meshInfo[i].colors32;
                originalColors[i] = new Color32[theColors.Length];
                Array.Copy(theColors, originalColors[i], theColors.Length);
            }
            int charCount = textInfo.characterCount;
            float[] charAnimStartTimes = new float[charCount];
            for (int i = 0; i < charCount; i++)
                charAnimStartTimes[i] = -1; //indicate the character as not yet started animating.

            int visableCharacterIndex = 0;
            while (!_stopAnimation) {
                if (_stopAnimating) {
                    for (int i = visableCharacterIndex; i < charCount; i++)
                        charAnimStartTimes[i] = GetTime();

                    visableCharacterIndex = charCount;
                    FinishAnimating(OnFinish);
                }
                if (ShouldShowNextCharacter(secondsPerCharacter, timeOfLastCharacter)) {
                    if (visableCharacterIndex <= charCount) {
                        ExecuteCommandsForCurrentIndex(commands, visableCharacterIndex, ref secondsPerCharacter, ref timeOfLastCharacter);
                        if (visableCharacterIndex < charCount && ShouldShowNextCharacter(secondsPerCharacter, timeOfLastCharacter)) {
                            charAnimStartTimes[visableCharacterIndex] = GetTime();
                            visableCharacterIndex++;
                            timeOfLastCharacter = GetTime();
                            if (visableCharacterIndex == charCount)
                                FinishAnimating(OnFinish);
                        }
                    }
                }
                for (int j = 0; j < charCount; j++) {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[j];
                    if (charInfo.isVisible) //Invisible characters have a vertexIndex of 0 because they have no vertices and so they should be ignored to avoid messing up the first character in the string whic also has a vertexIndex of 0
                    {
                        int vertexIndex = charInfo.vertexIndex;
                        int materialIndex = charInfo.materialReferenceIndex;
                        Color32[] destinationColors = textInfo.meshInfo[materialIndex].colors32;
                        Color32 theColor = j < visableCharacterIndex ? originalColors[materialIndex][vertexIndex] : new Color32(0, 0, 0, 0);
                        for (int k = 0; k < 4; k++)
                            destinationColors[vertexIndex + k] = theColor;

                        Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                        Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                        float charSize = 0;
                        float charAnimStartTime = charAnimStartTimes[j];
                        if (charAnimStartTime >= 0) {
                            float timeSinceAnimStart = GetTime() - charAnimStartTime;
                            charSize = Mathf.Min(1, timeSinceAnimStart / k_CharAnimTime);
                        }

                        Vector3 animPosAdjustment = GetAnimPosAdjustment(textAnimInfo, j, _textBox.fontSize, GetTime());
                        Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) * .5f;
                        destinationVertices[vertexIndex + 0] = ((sourceVertices[vertexIndex + 0] - offset) * charSize) + offset + animPosAdjustment;
                        destinationVertices[vertexIndex + 1] = ((sourceVertices[vertexIndex + 1] - offset) * charSize) + offset + animPosAdjustment;
                        destinationVertices[vertexIndex + 2] = ((sourceVertices[vertexIndex + 2] - offset) * charSize) + offset + animPosAdjustment;
                        destinationVertices[vertexIndex + 3] = ((sourceVertices[vertexIndex + 3] - offset) * charSize) + offset + animPosAdjustment;
                    }
                }
                _textBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                for (int i = 0; i < textInfo.meshInfo.Length; i++) {
                    TMP_MeshInfo theInfo = textInfo.meshInfo[i];
                    theInfo.mesh.vertices = theInfo.vertices;
                    _textBox.UpdateGeometry(theInfo.mesh, i);
                }
                yield return null;
            }
        }

        public IEnumerator AnimateText(List<DialogueCommand> commands, string processedMessage) {
            _stopAnimation = false;
            textAnimating = true;

            TextAnimInfo[] textAnimInfo = SeparateOutTextAnimInfo(commands);
            TMP_TextInfo textInfo = _textBox.textInfo;
            for (int i = 0; i < textInfo.meshInfo.Length; i++) { //Clear the mesh 
                TMP_MeshInfo meshInfer = textInfo.meshInfo[i];
                if (meshInfer.vertices != null) {
                    for (int j = 0; j < meshInfer.vertices.Length; j++) {
                        meshInfer.vertices[j] = Vector3.zero;
                    }
                }
            }

            _textBox.text = processedMessage;
            _textBox.ForceMeshUpdate();

            if (_textBox.textInfo.characterCount == 0) yield break;

            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
            Color32[][] originalColors = new Color32[textInfo.meshInfo.Length][];
            for (int i = 0; i < originalColors.Length; i++) {
                Color32[] theColors = textInfo.meshInfo[i].colors32;
                originalColors[i] = new Color32[theColors.Length];
                Array.Copy(theColors, originalColors[i], theColors.Length);
            }
            int charCount = textInfo.characterCount;
            float[] charAnimStartTimes = new float[charCount];
            for (int i = 0; i < charCount; i++)
                charAnimStartTimes[i] = -1; //indicate the character as not yet started animating.

            int visableCharacterIndex = charCount;
            while (!_stopAnimation) {
                for (int j = 0; j < charCount; j++) {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[j];
                    if (charInfo.isVisible) //Invisible characters have a vertexIndex of 0 because they have no vertices and so they should be ignored to avoid messing up the first character in the string whic also has a vertexIndex of 0
                    {
                        int vertexIndex = charInfo.vertexIndex;
                        int materialIndex = charInfo.materialReferenceIndex;
                        Color32[] destinationColors = textInfo.meshInfo[materialIndex].colors32;
                        Color32 theColor = j < visableCharacterIndex ? originalColors[materialIndex][vertexIndex] : new Color32(0, 0, 0, 0);
                        for (int k = 0; k < 4; k++)
                            destinationColors[vertexIndex + k] = theColor;

                        Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                        Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                        Vector3 animPosAdjustment = GetAnimPosAdjustment(textAnimInfo, j, _textBox.fontSize, GetTime());
                        Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) * .5f;
                        destinationVertices[vertexIndex + 0] = ((sourceVertices[vertexIndex + 0] - offset)) + offset + animPosAdjustment;
                        destinationVertices[vertexIndex + 1] = ((sourceVertices[vertexIndex + 1] - offset)) + offset + animPosAdjustment;
                        destinationVertices[vertexIndex + 2] = ((sourceVertices[vertexIndex + 2] - offset)) + offset + animPosAdjustment;
                        destinationVertices[vertexIndex + 3] = ((sourceVertices[vertexIndex + 3] - offset)) + offset + animPosAdjustment;
                    }
                }
                _textBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                for (int i = 0; i < textInfo.meshInfo.Length; i++) {
                    TMP_MeshInfo theInfo = textInfo.meshInfo[i];
                    theInfo.mesh.vertices = theInfo.vertices;
                    _textBox.UpdateGeometry(theInfo.mesh, i);
                }
                yield return null;
            }
        }

        private void ExecuteCommandsForCurrentIndex(List<DialogueCommand> commands, int visableCharacterIndex, ref float secondsPerCharacter, ref float timeOfLastCharacter) {
            for (int i = 0; i < commands.Count; i++) {
                DialogueCommand command = commands[i];
                if (command.position == visableCharacterIndex) {
                    switch (command.type) {
                        case DialogueCommandType.Pause:
                            timeOfLastCharacter = GetTime() + command.floatValue;
                            break;
                        case DialogueCommandType.TextSpeedChange:
                            secondsPerCharacter = 1f / command.floatValue;
                            break;
                    }
                    commands.RemoveAt(i);
                    i--;
                }
            }
        }

        public void FinishAnimating(Action OnFinish) {
            textAnimating = false;
            _stopAnimating = false;
            OnFinish?.Invoke();
        }

        private const float k_NoiseMagnitudeAdjustment = .24f;
        private const float k_NoiseFrequencyAdjustment = 20f;
        private const float k_WaveMagnitudeAdjustment = .12f;
        private Vector3 GetAnimPosAdjustment(TextAnimInfo[] textAnimInfo, int charIndex, float fontSize, float time) {
            float x = 0;
            float y = 0;
            for (int i = 0; i < textAnimInfo.Length; i++) {
                TextAnimInfo info = textAnimInfo[i];
                if (charIndex >= info.startIndex && charIndex < info.endIndex) {
                    if (info.type == TextAnimationType.shake) {
                        float scaleAdjust = fontSize * k_NoiseMagnitudeAdjustment;
                        x += (Mathf.PerlinNoise((charIndex + time) * k_NoiseFrequencyAdjustment, 0) - .5f) * scaleAdjust;
                        y += (Mathf.PerlinNoise((charIndex + time) * k_NoiseFrequencyAdjustment, 1000) - .5f) * scaleAdjust;
                    } else if (info.type == TextAnimationType.wave) {
                        y += Mathf.Sin((charIndex * 1.5f) + (time * 6)) * fontSize * k_WaveMagnitudeAdjustment;
                    }
                }
            }
            return new Vector3(x, y, 0);
        }

        private static bool ShouldShowNextCharacter(float secondsPerCharacter, float timeOfLastCharacter) {
            return (GetTime() - timeOfLastCharacter) > secondsPerCharacter;
        }

        public void SkipToEndOfCurrentText() {
            if (textAnimating) {
                _stopAnimating = true;
            }
        }

        public void StopAnimation() {
            _stopAnimation = true;
        }

        private TextAnimInfo[] SeparateOutTextAnimInfo(List<DialogueCommand> commands) {
            List<TextAnimInfo> tempResult = new List<TextAnimInfo>();
            List<DialogueCommand> animStartCommands = new List<DialogueCommand>();
            List<DialogueCommand> animEndCommands = new List<DialogueCommand>();
            for (int i = 0; i < commands.Count; i++) {
                DialogueCommand command = commands[i];
                if (command.type == DialogueCommandType.AnimStart) {
                    animStartCommands.Add(command);
                    commands.RemoveAt(i);
                    i--;
                } else if (command.type == DialogueCommandType.AnimEnd) {
                    animEndCommands.Add(command);
                    commands.RemoveAt(i);
                    i--;
                }
            }
            if (animStartCommands.Count != animEndCommands.Count) {
                GameLogger.LogError($"Unequal number of start and end animation commands. Start Commands: {animStartCommands.Count} End Commands: {animEndCommands.Count}");
            } else {
                for (int i = 0; i < animStartCommands.Count; i++) {
                    DialogueCommand startCommand = animStartCommands[i];
                    DialogueCommand endCommand = animEndCommands[i];
                    tempResult.Add(new TextAnimInfo {
                        startIndex = startCommand.position,
                        endIndex = endCommand.position,
                        type = startCommand.textAnimValue
                    });
                }
            }
            return tempResult.ToArray();
        }

        private static float GetTime() => UseUnscaledTime ? Time.unscaledTime : Time.time;
    }

    public struct TextAnimInfo {
        public int startIndex;
        public int endIndex;
        public TextAnimationType type;
    }
}