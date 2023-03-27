// #define QEDDEBUG

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class QEDCard : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _shapes;
    [SerializeField]
    private GameObject _symbol;
    [SerializeField]
    private Texture[] _symbols;

    public event Action Press = () => { };

    public int Part { get; set; }
    
    private void Start()
    {
        GetComponent<KMSelectable>().OnHighlight += () => { Highlight(true); };
        GetComponent<KMSelectable>().OnHighlightEnded += () => { Highlight(false); };
        GetComponent<KMSelectable>().OnInteract += () => { StartCoroutine(HandlePress()); return false; };

#if QEDDEBUG
        ButtonShape = Shape.Hexagon;
        BaseColor = Color.blue;
        HighlightColor = Color.black;
        SymbolBaseColor = Color.black;
        SymbolHighlightColor = Color.white;
        ShownSymbol = new Symbol() { Shape = Shape.Pentagon, Fill = Fill.Dotted };
#endif
    }

    private IEnumerator HandlePress()
    {
        GetComponentInParent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Press();

        const float depth = -0.005f;
        const float delay = 0.07f;
        float t = Time.time;
        float et = t + delay;
        while(Time.time < et)
        {
            foreach(GameObject s in _shapes)
                s.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, depth, (Time.time - t) / delay));
            _symbol.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, depth, (Time.time - t) / delay));
            yield return null;
        }
        t = Time.time;
        et = t + delay;
        while(Time.time < et)
        {
            foreach(GameObject s in _shapes)
                s.transform.localPosition = new Vector3(0f, Mathf.Lerp(depth, 0, (Time.time - t) / delay));
            _symbol.transform.localPosition = new Vector3(0f, Mathf.Lerp(depth, 0, (Time.time - t) / delay));
            yield return null;
        }
        foreach(GameObject s in _shapes)
            s.transform.localPosition = Vector3.zero;
        _symbol.transform.localPosition = Vector3.zero;
    }

    private bool _highlighted;
    private void Highlight(bool on)
    {
        _highlighted = on;
        ButtonColor = on ? HighlightColor : BaseColor;
        SymbolColor = on ? SymbolHighlightColor : SymbolBaseColor;
    }

    private Shape _buttonShape;
    public Shape ButtonShape
    {
        get
        {
            return _buttonShape;
        }
        set
        {
            foreach(GameObject s in _shapes)
                s.SetActive(false);
            _shapes[(int)(_buttonShape = value)].SetActive(true);

            ButtonColor = _shapes[(int)_buttonShape].GetComponent<Renderer>().material.color;
        }
    }

    private Color ButtonColor
    {
        get
        {
            return _shapes[(int)_buttonShape].GetComponent<Renderer>().material.color;
        }
        set
        {
            _shapes[(int)_buttonShape].GetComponent<Renderer>().material.color = value;
        }
    }
    private Color _highlightColor;
    public Color HighlightColor
    {
        get
        {
            return _highlightColor;
        }
        set
        {
            _highlightColor = value;
            if(_highlighted)
                ButtonColor = value;
        }
    }
    private Color _baseColor;
    public Color BaseColor
    {
        get
        {
            return _baseColor;
        }
        set
        {
            _baseColor = value;
            if(!_highlighted)
                ButtonColor = value;
        }
    }

    private Color SymbolColor
    {
        get
        {
            return _symbol.GetComponent<Renderer>().material.color;
        }
        set
        {
            _symbol.GetComponent<Renderer>().material.color = value;
        }
    }
    private Color _symbolHighlightColor;
    public Color SymbolHighlightColor
    {
        get
        {
            return _symbolHighlightColor;
        }
        set
        {
            _symbolHighlightColor = value;
            if(_highlighted)
                SymbolColor = value;
        }
    }
    private Color _symbolBaseColor;
    public Color SymbolBaseColor
    {
        get
        {
            return _symbolBaseColor;
        }
        set
        {
            _symbolBaseColor = value;
            if(!_highlighted)
                SymbolColor = value;
        }
    }

    private Symbol _shownSymbol;
    public Symbol ShownSymbol
    {
        get
        {
            return _shownSymbol;
        }
        set
        {
            _shownSymbol = value;
            _symbol.GetComponent<Renderer>().material.mainTexture = _symbols[4 * (int)value.Shape + (int)value.Fill];
        }
    }

    public enum Shape
    {
        Triangle = 0,
        Square = 1,
        Pentagon = 2,
        Hexagon = 3
    }
    public enum Fill
    {
        Empty = 0,
        Dotted = 1,
        Lined = 2,
        Solid = 3
    }
    public struct Symbol
    {
        public Shape Shape;
        public Fill Fill;

        public override string ToString()
        {
            return Fill.ToString() + " " + Shape.ToString();
        }
    }
}
