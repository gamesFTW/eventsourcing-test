﻿using System;
using System.Collections;
using TMPro;
using UnibusEvent;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
	public UnitDisplay UnitDisplay;
	public CardData cardData;

	public GameObject artwork;

	public TextMeshPro nameText;
	public TextMeshPro descriptionText;

	public TextMeshPro manaText;
	public TextMeshPro damageText;
    public TextMeshPro currentHpText;
    public TextMeshPro maxHpText;
    public TextMeshPro currentMovingPoints;

    public Preset SelectedHighlightGlow;
    public Preset OverHighlightGlow;

    public static readonly string CARD_PLAY_AS_MANA = "CARD_PLAY_AS_MANA";
    public static readonly string CARD_SELECTED_TO_PLAY = "CARD_SELECTED_TO_PLAY";
    public static readonly string CARD_MOUSE_ENTER = "CARD_MOUSE_ENTER";
    public static readonly string CARD_MOUSE_EXIT = "CARD_MOUSE_EXIT";

    private SpriteGlow.SpriteGlowEffect spriteGlowEffect;
    private bool IsSelected = false;

    private int currentMouseButton;

    public int CurrentHp
    {
        get { return cardData.currentHp; }
        set {
            cardData.currentHp = value;
            currentHpText.text = value.ToString();
        }
    }

    public int CurrentMovingPoints
    {
        get { return cardData.currentMovingPoints; }
        set
        {
            cardData.currentMovingPoints = value;
            currentMovingPoints.text = value.ToString();
        }
    }

    // Use this for initialization
    void Start () 
    {
		nameText.text = cardData.name;

		manaText.text = cardData.manaCost.ToString();
        damageText.text = cardData.damage.ToString();
        maxHpText.text = cardData.maxHp.ToString();
        currentHpText.text = cardData.currentHp.ToString();
        currentMovingPoints.text = cardData.currentMovingPoints.ToString();

        StartCoroutine(LoadSprite());
    }

    void Update()
    {
        CheckRightMouseDown();
    }

    public void FaceUp() {
        this.transform.Find("Back").gameObject.SetActive(false);
        this.transform.Find("Front").gameObject.SetActive(true);
    }

    public void FaceDown() {
        this.transform.Find("Back").gameObject.SetActive(true);
        this.transform.Find("Front").gameObject.SetActive(false);
    }

    public void Tap()
    {
        cardData.tapped = true;
        this.transform.Rotate(0, 0, -90);
    }

    public void Untap()
    {
        cardData.tapped = false;
        this.transform.Rotate(0, 0, 90);
    }

    public void ZoomIn ()
    {
        this.transform.localScale = new Vector3(1.5F, 1.5F, 1.5F);
        this.transform.position += new Vector3(0, 0, -1);
    }

    public void ZoomOut ()
    {
        this.transform.localScale = new Vector3(1, 1, 1);
        this.transform.position -= new Vector3(0, 0, -1);
    }

    void CheckRightMouseDown()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider.gameObject == this.gameObject)
            {
                OnRightMouseClicked();
            }
        }
    }

    void OnMouseDown()
    {
        OnLeftMouseClicked();
    }

    void OnMouseEnter()
    {
        this.ZoomIn();
        Unibus.Dispatch(CARD_MOUSE_ENTER, this);
    }

    void OnMouseExit()
    {
        this.ZoomOut();
        Unibus.Dispatch(CARD_MOUSE_EXIT, this);
    }
    
    public void SelectedHighlightOn()
    {
        IsSelected = true;
        
        if (!spriteGlowEffect)
        {
            GameObject go = transform.Find("Front").Find("CardTemplate").gameObject;
            spriteGlowEffect = go.AddComponent(typeof(SpriteGlow.SpriteGlowEffect)) as SpriteGlow.SpriteGlowEffect;
        }

        SelectedHighlightGlow.ApplyTo(spriteGlowEffect);
    }

    public void SelectedHighlightOff()
    {
        IsSelected = false;
        Destroy(spriteGlowEffect);
    }

    public void OverHighlightOn()
    {
        if (!IsSelected)
        {
            GameObject go = transform.Find("Front").Find("CardTemplate").gameObject;

            spriteGlowEffect = go.AddComponent(typeof(SpriteGlow.SpriteGlowEffect)) as SpriteGlow.SpriteGlowEffect;
            OverHighlightGlow.ApplyTo(spriteGlowEffect);
        }
    }

    public void OverHighlightOff()
    {
        if (!IsSelected)
        {
            Destroy(spriteGlowEffect);
        }
    }

    private void OnLeftMouseClicked()
    {
        Unibus.Dispatch(CARD_SELECTED_TO_PLAY, this);
    }

    private void OnRightMouseClicked()
    {
        Unibus.Dispatch(CARD_PLAY_AS_MANA, this);
    }

    public IEnumerator LoadSprite()
    {
        WWW www = new WWW(Config.LOBBY_SERVER_URL + cardData.image);
        yield return www;

        Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5F, 0.5F));

        artwork.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}