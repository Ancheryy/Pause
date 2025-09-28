using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Season1_2 : MonoBehaviour
{
    [SerializeField] public int seasonId;
    [SerializeField] public int correspondingGlassDomeId;
    [SerializeField] public bool isClickable = true;
    [SerializeField] public Sprite season;
    [SerializeField] public Sprite goldenSeason;
    [SerializeField] public Sprite selectSeason;

    public void TurnToGoldenSeason()
    {
        GetComponent<SpriteButton>().SetInteractable(false);
        
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                var color = this.GetComponent<SpriteRenderer>().color;
                color.a = 0f;
                this.GetComponent<SpriteRenderer>().color = color;
                this.GetComponent<SpriteRenderer>().sprite = goldenSeason;
                this.GetComponent<SpriteFade>().FadeIn(0.4f);
            })
            .AddWait(0.5f)
            .AddNode(() =>
            {
                this.GetComponent<SpriteFade>().FadeOut(0.4f);
            })
            .AddWait(0.5f)
            .AddNode(() =>
            {
                this.GetComponent<SpriteRenderer>().sprite = goldenSeason;
            });

        anim.Play();
    }

    public void SelectSeason(UnityAction<int> onComplete)
    {
        AnimSequence anim = AnimMgr.Instance.CreateSequence();
        anim.AddNode(() =>
            {
                var color = this.GetComponent<SpriteRenderer>().color;
                color.a = 0f;
                this.GetComponent<SpriteRenderer>().color = color;
                this.GetComponent<SpriteRenderer>().sprite = selectSeason;
                this.GetComponent<SpriteFade>().FadeIn(0.2f);
            })
            .AddWait(0.3f)
            .AddNode(() =>
            {
                this.GetComponent<SpriteFade>().FadeOut(0.2f);
            })
            .AddWait(0.3f)
            .AddNode(() =>
            {
                this.GetComponent<SpriteRenderer>().sprite = season;
                this.GetComponent<SpriteFade>().FadeIn(0.2f);

                onComplete?.Invoke(seasonId - 1);
            });

        anim.Play();
    }
    
}
