using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class ButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    public float scaleDown = 1f;
    public float duration = 1f;

    [Header("Custom")]
    public TextMeshProUGUI label;
    public Color normalColor;
    public Color pressColor;

    private void Awake() {
        if (label) {
            normalColor = label.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (scaleDown != 1f) {
            transform.DOScale(scaleDown, duration);
            }
        //
        if (label) {
            label.DOColor(pressColor, duration);
        }
    }
    //
    public void OnPointerUp(PointerEventData eventData) {
        if (scaleDown != 1f) {
            transform.DOScale(1f, duration);
            }
        //
        if (label) {
            label.DOColor(normalColor, duration);
        }
    }
    //
    }