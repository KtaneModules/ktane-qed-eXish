using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class QEDScript : MonoBehaviour
{
    [SerializeField]
    private QEDCard[] _buttons;
    [SerializeField]
    private Renderer _backing;

    private int _id;
    private static int _idc = 1;
    private bool _isSolved;

    private void Start()
    {
        _id = _idc++;
        Generate();
    }

    private QEDCard.Shape[] _shapes;
    private int[] _hls;
    private QEDCard.Symbol[] _symbols;

    private static readonly string[] _hlNames = new string[] { "White on White", "White on Black", "Black on White", "Black on Black" };
    private static readonly Color _buttonColor = new Color32(97, 65, 180, 255);
    private struct CardData
    {
        public QEDCard.Shape Shape;
        public int Hl;
        public QEDCard.Symbol Symbol;

        public static CardData Rng()
        {
            return new CardData()
            {
                Shape = (QEDCard.Shape)Random.Range(0, 4),
                Hl = Random.Range(0, 4),
                Symbol = new QEDCard.Symbol()
                {
                    Shape = (QEDCard.Shape)Random.Range(0, 4),
                    Fill = (QEDCard.Fill)Random.Range(0, 4)
                }
            };
        }

        public int Part;
    }
    private void Generate()
    {
        //_shapes = Enumerable.Repeat(1, 16).Select(_ => (QEDCard.Shape)Random.Range(0, 4)).ToArray();
        //_hls = Enumerable.Repeat(2, 16).Select(_ => Random.Range(0, 4)).ToArray();
        //_symbols = Enumerable.Repeat(3, 16).Select(_ => new QEDCard.Symbol() { Shape = (QEDCard.Shape)Random.Range(0, 4), Fill = (QEDCard.Fill)Random.Range(0, 4) }).ToArray();
        _shapes = new QEDCard.Shape[16];
        _hls = new int[16];
        _symbols = new QEDCard.Symbol[16];

        bool[] set = new bool[4].Select(_ => Random.Range(0, 1) == 1).ToArray();

        // shape, hl, shape, fill
        // same, 2pair, different
        int[][] qedmodes = new int[4][];
        do
        {
            for(int i = 0; i < 4; ++i)
            {
                if(set[i])
                    qedmodes[i] = Enumerable.Repeat(Random.Range(1, 4), 3).ToArray();
                else
                    qedmodes[i] = Enumerable.Range(1, 3).OrderBy(_ => Random.value).ToArray();
            }
        }
        while(new int[] { 0, 1, 2 }.Any(i => new int[] { 0, 1, 2, 3 }.All(j => qedmodes[j][i] == 1)));

        CardData[][] qeds = new CardData[3][];
        for(int i = 0; i < 3; ++i)
            qeds[i] = new CardData[4];

        for(int q = 0; q < 3; ++q)
        {
            QEDCard.Shape[] tshapes;
            if(qedmodes[0][q] == 1)
                tshapes = Enumerable.Repeat((QEDCard.Shape)Random.Range(0, 4), 4).ToArray();
            else if(qedmodes[0][q] == 2)
                tshapes = Enumerable.Range(0, 4).Select(i => (QEDCard.Shape)i).OrderBy(_ => Random.value).Take(2).SelectMany(i => new QEDCard.Shape[] { i, i }).ToArray();
            else if(qedmodes[0][q] == 3)
                tshapes = Enumerable.Range(0, 4).Select(i => (QEDCard.Shape)i).OrderBy(_ => Random.value).ToArray();
            else
                throw new Exception("shape mode");

            int[] thls;
            if(qedmodes[1][q] == 1)
                thls = Enumerable.Repeat(Random.Range(0, 4), 4).ToArray();
            else if(qedmodes[1][q] == 2)
                thls = Enumerable.Range(0, 4).OrderBy(_ => Random.value).Take(2).SelectMany(i => new int[] { i, i }).ToArray();
            else if(qedmodes[1][q] == 3)
                thls = Enumerable.Range(0, 4).OrderBy(_ => Random.value).ToArray();
            else
                throw new Exception("hl mode");

            QEDCard.Shape[] tshapes2;
            if(qedmodes[2][q] == 1)
                tshapes2 = Enumerable.Repeat((QEDCard.Shape)Random.Range(0, 4), 4).ToArray();
            else if(qedmodes[2][q] == 2)
                tshapes2 = Enumerable.Range(0, 4).Select(i => (QEDCard.Shape)i).OrderBy(_ => Random.value).Take(2).SelectMany(i => new QEDCard.Shape[] { i, i }).ToArray();
            else if(qedmodes[2][q] == 3)
                tshapes2 = Enumerable.Range(0, 4).Select(i => (QEDCard.Shape)i).OrderBy(_ => Random.value).ToArray();
            else
                throw new Exception("shape mode 2");

            QEDCard.Fill[] tfills;
            if(qedmodes[3][q] == 1)
                tfills = Enumerable.Repeat((QEDCard.Fill)Random.Range(0, 4), 4).ToArray();
            else if(qedmodes[3][q] == 2)
                tfills = Enumerable.Range(0, 4).Select(i => (QEDCard.Fill)i).OrderBy(_ => Random.value).Take(2).SelectMany(i => new QEDCard.Fill[] { i, i }).ToArray();
            else if(qedmodes[3][q] == 3)
                tfills = Enumerable.Range(0, 4).Select(i => (QEDCard.Fill)i).OrderBy(_ => Random.value).ToArray();
            else
                throw new Exception("fill mode");

            for(int i = 0; i < 4; ++i)
            {
                qeds[q][i] = new CardData()
                {
                    Shape = tshapes[i],
                    Hl = thls[i],
                    Symbol = new QEDCard.Symbol()
                    {
                        Shape = tshapes2[i],
                        Fill = tfills[i]
                    },
                    Part = q + 1
                };
            }
        }

        CardData[] r = Enumerable.Repeat(0, 4).Select(_ => CardData.Rng()).ToArray();
        CardData[] cards = qeds.SelectMany(q => q).Concat(r).ToArray();
        cards = cards.OrderBy(_ => Random.value).ToArray();
        _shapes = cards.Select(d => d.Shape).ToArray();
        _hls = cards.Select(d => d.Hl).ToArray();
        _symbols = cards.Select(d => d.Symbol).ToArray();

        for(int i = 0; i < 16; ++i)
        {
            Log(string.Format("Button {0}: {1}, {2}, {3}", i, _shapes[i], _hlNames[_hls[i]], _symbols[i]));

            _buttons[i].ButtonShape = _shapes[i];
            _buttons[i].BaseColor = _buttonColor;
            _buttons[i].HighlightColor = _hls[i] % 2 == 1 ? Color.white : Color.black;
            _buttons[i].SymbolBaseColor = Color.black;
            _buttons[i].SymbolHighlightColor = _hls[i] / 2 == 1 ? Color.white : Color.black;
            _buttons[i].ShownSymbol = _symbols[i];
            _buttons[i].Part = cards[i].Part;

            int j = i;
            _buttons[i].Press += () => Press(j);
        }

        Log(string.Format("A possible solution would be: ({0}), ({1}), ({2})",
            Enumerable.Range(0, 16).Where(i => cards[i].Part == 1).Join(", "),
            Enumerable.Range(0, 16).Where(i => cards[i].Part == 2).Join(", "),
            Enumerable.Range(0, 16).Where(i => cards[i].Part == 3).Join(", ")
            ));

        //TODO: Generate uniquely?
    }

    private readonly List<int> _selections = new List<int>(12);
    private readonly List<Color> _usedColors = new List<Color>(3);
    private readonly List<Color> _unusedColors = new List<Color>(3) { Color.red, Color.green, Color.blue };
    private readonly List<int[]> _qedRules = new List<int[]>(3);
    private void Press(int ix)
    {
        Log("Pressed button " + ix);
        if(_isSolved)
            return;

        if(_usedColors.Count == 0)
        {
            int c = Random.Range(0, 3);
            _usedColors.Add(_unusedColors[c]);
            _unusedColors.RemoveAt(c);
        }
        if(_selections.Contains(ix))
        {
            int pos = _selections.IndexOf(ix);
            if(_selections.Count >= 8 && pos >= 4 && pos < 8)
            {
                foreach(int btn in _selections.Skip(4).Take(4))
                    _buttons[btn].BaseColor = _buttonColor;
                _selections.RemoveRange(4, 4);
                _unusedColors.Add(_usedColors[1]);
                _usedColors.RemoveAt(1);
                _qedRules.RemoveAt(1);
            }
            else if(_selections.Count >= 4 && pos < 4)
            {
                foreach(int btn in _selections.Take(4))
                    _buttons[btn].BaseColor = _buttonColor;
                _selections.RemoveRange(0, 4);
                _unusedColors.Add(_usedColors[0]);
                _usedColors.RemoveAt(0);
                _qedRules.RemoveAt(0);
            }
            else
            {
                _buttons[ix].BaseColor = _buttonColor;
                _selections.RemoveAt(pos);
            }
        }
        else
        {
            if(_selections.Count % 4 == 3)
            {
                bool valid = IsQED(_selections.TakeLast(3).Concat(new int[] { ix }).ToArray());

                if(!valid)
                {
                    Log("That does not form a Q.E.D. Strike!");
                    GetComponent<KMBombModule>().HandleStrike();
                }
                else
                {
                    Log("That is a Q.E.D.");
                    if(_selections.Count == 11)
                    {
                        bool setValid = IsSET();

                        if(!setValid)
                        {
                            Log("Those Q.E.D.s do not form a S.E.T. Strike!");
                            GetComponent<KMBombModule>().HandleStrike();
                        }
                        else
                        {
                            Log("That is a S.E.T. Module solved.");
                            _isSolved = true;
                            _buttons[ix].BaseColor = _usedColors.Last();
                            _backing.material.color = new Color32(14, 118, 35, 255);
                            GetComponent<KMAudio>().PlaySoundAtTransform("solve", transform);
                            GetComponent<KMBombModule>().HandlePass();
                        }
                    }
                    else
                    {
                        _selections.Add(ix);
                        _buttons[ix].BaseColor = _usedColors.Last();

                        int c = Random.Range(0, _unusedColors.Count);
                        _usedColors.Add(_unusedColors[c]);
                        _unusedColors.RemoveAt(c);
                    }
                }
            }
            else
            {
                _selections.Add(ix);
                _buttons[ix].BaseColor = _usedColors.Last();
            }
        }
    }

    private bool IsSET()
    {
        bool[] good = new bool[4];

        for(int i = 0; i < 4; ++i)
        {
            int[] s = new int[] { 0, 1, 2 }.Select(q => _qedRules[q][i]).ToArray();
            if(s.Distinct().Count() == 3 || s.Distinct().Count() == 1)
                good[i] = true;
        }

        if(good.All(i => i))
            return true;
        else
        {
            if(!good[0])
                Log("Bad S.E.T. of button shapes.");
            if(!good[0])
                Log("Bad S.E.T. of highlight rules.");
            if(!good[0])
                Log("Bad S.E.T. of shape fills.");
            if(!good[0])
                Log("Bad S.E.T. of shapes.");
            return false;
        }
    }

    private bool IsQED(int[] cards)
    {
        int[] rules = new int[4];

        int[][] s = new int[][]
        {
            cards.Select(i => (int)_shapes[i]).ToArray(),
            cards.Select(i => _hls[i]).ToArray(),
            cards.Select(i => (int)_symbols[i].Fill).ToArray(),
            cards.Select(i => (int)_symbols[i].Shape).ToArray()
        };
        for(int i = 0; i < s.Length; ++i)
        {
            if(s[i].Distinct().Count() == 4)
                rules[i] = 1;
            else if(s[i].Distinct().Count() == 2 && s[i].Count(sp => sp == s[i][0]) == 2)
                rules[i] = 2;
            else if(s[i].Distinct().Count() == 1)
                rules[i] = 3;
        }

        if(rules.All(i => i != 0))
        {
            _qedRules.Add(rules);
            return true;
        }
        else
        {
            if(rules[0] == 0)
                Log("Bad button shapes.");
            if(rules[1] == 0)
                Log("Bad highlight rules.");
            if(rules[2] == 0)
                Log("Bad shape fills.");
            if(rules[3] == 0)
                Log("Bad shapes.");
            return false;
        }
    }

    private void Log(string v)
    {
        //#if UNITY_EDITOR
        Debug.Log("[Q.E.D. #" + _id + "] " + v);
        //#endif
    }

#pragma warning disable 414
    private const string TwitchHelpMessage = @"!{0} A1 B1 C1...";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToLowerInvariant();
        Match m = Regex.Match(command, @"[a-d][1-4](?:\s+[a-d][1-4])*");

        if(m.Success)
        {
            yield return null;
            while((m = Regex.Match(command, @"^\s*([a-d])([1-4])\s*")).Success)
            {
                int ix = "abcd".IndexOf(m.Groups[1].Value) + 4 * int.Parse(m.Groups[2].Value) - 4;
                _buttons[ix].GetComponent<KMSelectable>().OnInteract();
                command = command.Substring(m.Index + m.Length);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if(_selections.Count >= 4)
        {
            for(int i = _selections.Count / 4 - 1; i > 0; --i)
            {
                int[] ps = _selections.Skip(4 * i).Take(4).Select(ix => _buttons[ix].Part).ToArray();
                if(!ps.All(a => a == ps[0] && a != 0))
                {
                    _buttons[_selections[4 * i]].GetComponent<KMSelectable>().OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        int q = 1;

        if(_selections.Count % 4 != 0)
        {
            for(int i = _selections.Count - 1; (i + 4) % 4 != 3; --i)
            {
                int[] ps = _selections.TakeLast(_selections.Count % 4).ToArray();
                if(!ps.All(a => a == ps[0] && a != 0))
                {
                    _buttons[_selections[i]].GetComponent<KMSelectable>().OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                else
                    q = _buttons[_selections[i]].Part;
            }
        }

        List<QEDCard> bs = _buttons.Where((c, i) => !_selections.Contains(i) || c.Part == 0).ToList();
        while(!_isSolved)
        {
            foreach(QEDCard c in bs.Where(cd => cd.Part == q))
            {
                c.GetComponent<KMSelectable>().OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            bs.RemoveAll(c => c.Part == q);
            if(bs.Count != 0)
                q = bs.First().Part;
            if(bs.Count == 0)
                break;
        }

        yield break;
    }
}
