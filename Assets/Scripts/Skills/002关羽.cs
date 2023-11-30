using GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class 武圣 : Converted, Durative
{
    // 转化牌
    public override Card Convert(List<Card> cards) => Card.Convert<杀>(src, cards);
    public override bool IsValidCard(Card card) => card.isRed && base.IsValidCard(card);

    // 无距离限制
    public void OnStart()
    {
        src.effects.NoDistanceLimit.Add((x, y) => x is 杀 && x.suit == "方片" && x.isConvert, this);
    }
}

/// <summary>
/// 义绝
/// </summary>
public class 义绝 : Active
{
    public override int MaxCard => 1;
    public override int MinCard => 1;
    public override int MaxDest => 1;
    public override int MinDest => 1;

    public override bool IsValidDest(Player dest) => dest.handCardsCount > 0 && dest != src;

    public override async Task Use(Decision decision)
    {
        Execute(decision);
        var dest = decision.dests[0];

        // 弃一张手牌
        await new Discard(src, decision.cards).Execute();

        // 展示手牌
        Timer.Instance.hint = src + "对你发动义绝，请展示一张手牌。";
        var showCard = await TimerAction.ShowOneCard(dest);

        // 红色
        if (showCard[0].isRed)
        {
            // 获得牌
            await new GetCardFromElse(src, dest, showCard).Execute();
            // 回复体力
            if (dest.hp < dest.hpLimit)
            {
                Timer.Instance.hint = "是否让" + dest + "回复一点体力？";
                Timer.Instance.DefaultAI = () => new Decision { action = (dest.team == src.team) == AI.CertainValue };
                if ((await Timer.Instance.Run(src)).action) await new Recover(dest).Execute();
            }
        }
        // 黑色
        else
        {
            // 禁用手牌
            dest.effects.DisableCard.Add(x => x.isHandCard, Duration.UntilTurnEnd);
            // 禁用非锁定技
            dest.effects.DisableSkill.Add(x => !x.passive, Duration.UntilTurnEnd);
            // 下次受到杀的伤害+1
            dest.effects.OffsetDamageValue.Add(x => x.Src == src && x.SrcCard is 杀 sha && sha.suit == "红桃" ? 1 : 0, Duration.UntilTurnEnd, true);
        }
    }

    public override Decision AIDecision()
    {
        Timer.Instance.temp.cards = AI.GetRandomCard();
        Timer.Instance.temp.dests = AI.GetValidDest();
        return base.AIDecision();
    }
}
