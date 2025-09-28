using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GlassDome1_2 : MonoBehaviour
{
    [FormerlySerializedAs("part1_Open")]
    [Header("Part 1")]
    [SerializeField] private GameObject part1Close;
    [SerializeField] private GameObject part1Open;
    
    [Header("Part 2")]
    [SerializeField] public int glassDomeId;
    [SerializeField] public int correspondingSeasonId;
    [SerializeField] public bool isClickable = true;
    [SerializeField] public GameObject glassDomeClose;
    [SerializeField] public GameObject glassDomeOpen;
    
    [SerializeField] public GameObject selectBackground;
    [SerializeField] public GameObject correctBackground;

    
    public void Part1OpenGlassDome()
    {
        part1Close.SetActive(false);
        part1Open.SetActive(true);
    }
    
    public void Part1CloseGlassDome()
    {
        part1Close.SetActive(true);
        part1Open.SetActive(false);
    }

    public void Part2OpenGlassDome(bool isSelect)
    {
        glassDomeClose.gameObject.SetActive(false);
        if (isSelect)
        {
            selectBackground.gameObject.SetActive(true);
            correctBackground.gameObject.SetActive(false);
        }
        else
        {
            selectBackground.gameObject.SetActive(false);
            correctBackground.gameObject.SetActive(true);
        }

        glassDomeOpen.gameObject.SetActive(true);
    }

    public void Part2CloseGlassDome()
    {
        glassDomeClose.gameObject.SetActive(true);
        glassDomeOpen.gameObject.SetActive(false);

        if (selectBackground != null)
        {
            selectBackground.gameObject.SetActive(false);
            correctBackground.gameObject.SetActive(false);
        }
    }

    public void TurnToGoldenBG()
    {
        selectBackground.gameObject.SetActive(false);
        correctBackground.gameObject.SetActive(true);
        glassDomeClose.GetComponent<SpriteButton>().SetInteractable(false);

        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode((() =>
            {
                correctBackground.gameObject.SetActive(true);
                glassDomeClose.gameObject.SetActive(false);
                glassDomeOpen.gameObject.SetActive(false);
                correctBackground.GetComponent<SpriteFade>().FadeIn(0.4f);
            }))
            .AddWait(0.5f)
            .AddNode((() =>
            {
                correctBackground.GetComponent<SpriteFade>().FadeOut(0.4f);
            }))
            .AddWait(0.5f)
            .AddNode((() =>
            {
                correctBackground.gameObject.SetActive(false);
            }));
        
        anim.Play();
    }

}
