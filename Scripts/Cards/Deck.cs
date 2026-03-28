using System;
using System.Collections.Generic;
using System.Linq;

namespace Sortis.Cards;

/// <summary>
/// 런 중 플레이어의 덱을 관리한다.
/// 드로우파일 / 핸드 / 버림파일 / 소멸파일의 4영역 구조.
/// </summary>
public class Deck
{
    private readonly List<Card> _drawPile = new();
    private readonly List<Card> _hand = new();
    private readonly List<Card> _discardPile = new();
    private readonly List<Card> _exhaustPile = new();
    private readonly Random _rng = new();

    public IReadOnlyList<Card> Hand => _hand;
    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;

    /// <summary>덱 초기화 — 모든 카드를 드로우파일에 넣고 셔플</summary>
    public void Initialize(IEnumerable<Card> cards)
    {
        _drawPile.Clear();
        _hand.Clear();
        _discardPile.Clear();
        _exhaustPile.Clear();

        _drawPile.AddRange(cards);
        Shuffle();
    }

    /// <summary>드로우파일에서 N장을 핸드로 드로우</summary>
    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
                ReshuffleDiscardPile();

            if (_drawPile.Count == 0)
                break; // 모든 카드가 소멸되었으면 중단

            var card = _drawPile[^1];
            _drawPile.RemoveAt(_drawPile.Count - 1);
            _hand.Add(card);
        }
    }

    /// <summary>핸드에서 카드를 버림패로 이동</summary>
    public void DiscardFromHand(Card card)
    {
        if (_hand.Remove(card))
            _discardPile.Add(card);
    }

    /// <summary>턴 종료 시 남은 핸드 전부 버림</summary>
    public void DiscardAllHand()
    {
        _discardPile.AddRange(_hand);
        _hand.Clear();
    }

    /// <summary>카드를 영구 소멸 (Death 등 메이저 아르카나 효과)</summary>
    public void Exhaust(Card card)
    {
        _hand.Remove(card);
        _exhaustPile.Add(card);
    }

    private void Shuffle()
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }

    private void ReshuffleDiscardPile()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle();
    }
}
