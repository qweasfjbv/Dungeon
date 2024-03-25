using UnityEngine;

public class PotionCard : CardBase
{
    [SerializeField]
    private GameObject effectPrefab;

    private GameObject effect = null;

    public override void ActivateEffect(Vector3 pos)
    {
        effect.GetComponent<PotionEffect>().StartEffect(5f);
    }

    public override void PreviewEffect(Vector3 pos)
    {
        pos.z = 0;

        effect.transform.position = pos;
        effect.GetComponent<PotionEffect>().PreviewizeEffect();

        if (!effect.activeSelf)
            effect.SetActive(true);
    }


    public override void OnEnable()
    {
        base.OnEnable();
        if (effect == null)
        {
            effect = Instantiate(effectPrefab);
            effect.SetActive(false);
        }
        
    }

    public override void UnPreviewEffect()
    {
        effect.SetActive(false);
    }
}
