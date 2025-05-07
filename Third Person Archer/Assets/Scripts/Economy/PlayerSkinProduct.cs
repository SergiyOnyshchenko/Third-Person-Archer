using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PlayerSkinProduct : MonoBehaviour, IProduct
{
    [field: SerializeField] public Price Price { get; private set; }
    [Space]
    [SerializeField] private PlayerSkinSelector _skinSelector;
    [SerializeField] private GameObject _lockView;
    [SerializeField] private PurchaseButton _purchaseButton;
    [SerializeField] private string _saveKey;
    private Button _selectButton;
    public bool IsPurchased { get; private set; }

    private void Awake()
    {
        _selectButton = GetComponent<Button>(); 

        Load();
        UpdateView();
    }

    private void OnEnable()
    {
        _purchaseButton.onPurchaseSuccess.AddListener(Purchase);
    }

    private void OnDisable()
    {
        _purchaseButton.onPurchaseSuccess.RemoveListener(Purchase);
    }

    public void Purchase() 
    {
        Purchase(null);
    }

    public void Purchase(IBuyer buyer)
    {
        IsPurchased = true;
        _skinSelector.Select();
        UpdateView();
        Save();
    }

    private void UpdateView()
    {
        _lockView.SetActive(!IsPurchased);
        _selectButton.interactable = IsPurchased;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(_saveKey, MathExtentions.BoolToInt(IsPurchased));
    }

    private void Load()
    {
        IsPurchased = MathExtentions.IntToBool(PlayerPrefs.GetInt(_saveKey));
    }

}
