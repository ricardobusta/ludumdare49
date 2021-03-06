using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Cauldron : InteractiveObject
{
    public int level = 1;
    [SerializeField] private float unstableSpeed = 0.1f;
    [SerializeField] private float wrongDamage = 0.1f;
    [SerializeField] private float rightHeal = 1f;

    [SerializeField] private float stability = 1;

    [SerializeField] private MeshRenderer cauldronSoup;

    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Canvas gameOverCanvas;

    [SerializeField] private Button menuButton;

    [SerializeField] private TMP_Text resultText;

    [SerializeField] private Image stabilityBar;
    [SerializeField] private TMP_Text levelLabel;

    [SerializeField] private FpsInput input;

    private readonly Dictionary<int, List<CmykColor>> _targetColorByLevel = new Dictionary<int, List<CmykColor>>()
    {
        [1] = new List<CmykColor>
        {
            new CmykColor {c = 1, m = 0, y = 0, k = 0, a = 1},
            new CmykColor {c = 0, m = 1, y = 0, k = 0, a = 1},
            new CmykColor {c = 0, m = 0, y = 1, k = 0, a = 1},
            new CmykColor {c = 0, m = 0, y = 0, k = 1, a = 1},
            new CmykColor {c = 0, m = 0, y = 0, k = 0, a = 1},
        },
        [2] = new List<CmykColor>
        {
            new CmykColor {c = 1, m = 1, y = 0, k = 0, a = 1},
            new CmykColor {c = 1, m = 0, y = 1, k = 0, a = 1},
            new CmykColor {c = 0, m = 1, y = 1, k = 0, a = 1},
            new CmykColor {c = 1, m = 1, y = 1, k = 0, a = 1},
        },
        [3] = new List<CmykColor>
        {
            new CmykColor {c = 1, m = 0, y = 0, k = 0, a = .5f},
            new CmykColor {c = 0, m = 1, y = 0, k = 0, a = .5f},
            new CmykColor {c = 0, m = 0, y = 1, k = 0, a = .5f},
            new CmykColor {c = 0, m = 0, y = 0, k = 1, a = .5f},
            new CmykColor {c = 0, m = 0, y = 0, k = 0, a = .5f},
            new CmykColor {c = 1, m = 1, y = 0, k = 0, a = .5f},
            new CmykColor {c = 0, m = 1, y = 1, k = 0, a = .5f},
            new CmykColor {c = 1, m = 0, y = 1, k = 0, a = .5f},
            new CmykColor {c = 1, m = 1, y = 1, k = 0, a = .5f},
        },
        [3] = new List<CmykColor>
        {
            new CmykColor {c = .5f, m = 0, y = 0, k = 0, a = 1},
            new CmykColor {c = 0, m = .5f, y = 0, k = 0, a = 1},
            new CmykColor {c = 0, m = 0, y = .5f, k = 0, a = 1},
            new CmykColor {c = 0, m = 0, y = 0, k = .5f, a = 1},
        },
    };

    private CmykColor _targetColor;
    private Material _cauldronSoupMaterial;

    private void Start()
    {
        gameCanvas.gameObject.SetActive(true);
        gameOverCanvas.gameObject.SetActive(false);
        _cauldronSoupMaterial = new Material(cauldronSoup.material);
        cauldronSoup.material = _cauldronSoupMaterial;
        SetLevel(1);

        menuButton.onClick.AddListener(() => { SceneManager.LoadScene("Title"); });
    }

    private void Update()
    {
        if (input.locked)
        {
            return;
        }
        
        if (stability <= 0)
        {
            gameCanvas.gameObject.SetActive(false);
            gameOverCanvas.gameObject.SetActive(true);
            input.locked = true;
            resultText.text = $"Congratulations! You reached level {level}!";
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        stability -= level * unstableSpeed * Time.deltaTime;
        stability = Mathf.Clamp01(stability);
        stabilityBar.fillAmount = stability;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetLevel(level + 1);
            stability = 1;
        }else if (Input.GetKeyDown(KeyCode.F2))
        {
            SetLevel(level + 10);
            stability = 1;
        }
#endif
    }

    private void SetLevel(int l)
    {
        level = l;
        if (level <= _targetColorByLevel.Count)
        {
            var potionLevel = _targetColorByLevel[Random.Range(1, Mathf.Min(level, _targetColorByLevel.Count) + 1)];
            _targetColor = potionLevel[Random.Range(0, potionLevel.Count)];
        }
        else
        {
            float RandomValue(int steps, int min = 0)
            {
                return Random.Range(min, steps) / (float) (steps - 1);
            }

            _targetColor = new CmykColor
                {c = RandomValue(5), m = RandomValue(5), y = RandomValue(5), k = 0, a = RandomValue(3, 1)};
        }

        levelLabel.text = $"Cauldron Stability - Level {level}\nTarget: {_targetColor}";
        stabilityBar.color = _targetColor.ToRgba();
        _cauldronSoupMaterial.color = stabilityBar.color;
    }

    public override void Interact(HandController controller)
    {
        if (!controller.HasHoldingItem())
        {
            return;
        }

        if (controller.HasHoldingItem(Item.ItemType.Flask) || controller.HasHoldingItem(Item.ItemType.Ingredient))
        {
            stability -= wrongDamage;
            Destroy(controller.RemoveHoldingItem(null).gameObject);
            return;
        }

        if (controller.HasHoldingItem(Item.ItemType.Potion))
        {
            var item = controller.RemoveHoldingItem(null);
            var color = item.color;
            Destroy(item.gameObject);
            if (color == _targetColor)
            {
                stability += rightHeal;
                SetLevel(level + 1);
            }
            else
            {
                stability -= wrongDamage;
            }
        }
    }
}