using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Model
{
    public class EventSystem : Singleton<EventSystem>
    {
        // public List<Skill> skills = new();

        public async Task Invoke<T>(Func<Skill, Predicate<T>> func, T arg, Action afterInvoke = null)
        {
            foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
            {
                foreach (var skill in i.skills.Where(x => x.IsValid && func(x)(arg)).ToList())
                {
                    await skill.Invoke(arg);
                    afterInvoke?.Invoke();
                }
            }

            // foreach(var i in skills.Where(x=>x.IsValid&&func(x)(arg)))
        }
    }

    //     private async Task InvokeOne<T, TArg>(TArg arg, Player player) where T : SkillEvent<TArg>
    //     {
    //         foreach (var skill in player.skills)
    //         {
    //             if (skill is T t && t.IsValid(arg)) await t.Invoke(arg);
    //         }
    //     }

    //     public async Task OnPhaseStart(Player player, Phase phase)
    //     {
    //         foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
    //         {
    //             foreach (var j in i.skills)
    //             {
    //                 if (j is OnPhaseStart onPhaseStart && i == player && onPhaseStart.IsValid(phase)) await onPhaseStart.Invoke(phase);
    //                 else if (j is OnEveryPhaseStart x && x.IsValid(player, phase)) await x.Invoke(player, phase);
    //             }
    //         }
    //     }

    //     public async Task OnPhaseOver(Player player, Phase phase)
    //     {
    //         foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
    //         {
    //             foreach (var skill in i.skills)
    //             {
    //                 if (skill is OnPhaseOver x1 && i == player && x1.IsValid(phase)) await x1.Invoke(phase);
    //                 else if (skill is OnEveryPhaseOver x2 && x2.IsValid(player, phase)) await x2.Invoke(player, phase);
    //             }
    //         }
    //     }

    //     // public async Task InGetCardPhase(GetCardFromPile getCard)
    //     // {
    //     //     foreach (var skill in getCard.player.skills)
    //     //     {
    //     //         if (skill is OnGetCardPhase x && x.IsValid(getCard)) await x.Invoke(getCard);
    //     //     }
    //     // }
    //     public async Task InGetCardPhase(GetCardFromPile getCard) => await InvokeOne<OnGetCardPhase, GetCardFromPile>(getCard, getCard.player);

    //     public async Task AfterGetCard(GetCard getCard)
    //     {
    //         foreach (var skill in getCard.player.skills)
    //         {
    //             if (skill is AfterGetCard x && x.IsValid(getCard)) await x.Invoke(getCard);
    //             else if (getCard is GetCardFromElse gcfl && skill is AfterGetCardFromElse x1 && x1.IsValid(gcfl)) await x1.Invoke(gcfl);
    //         }
    //     }

    //     // public async Task AfterLoseCard(LoseCard loseCard)
    //     // {
    //     //     foreach (var skill in loseCard.player.skills)
    //     //     {
    //     //         if (skill is AfterLoseCard x && x.IsValid(loseCard)) await x.Invoke(loseCard);
    //     //     }
    //     // }
    //     public async Task AfterLoseCard(LoseCard loseCard) => await InvokeOne<AfterLoseCard, LoseCard>(loseCard, loseCard.player);

    //     public async Task OnDamaged(Damaged damaged)
    //     {
    //         foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
    //         {
    //             foreach (var skill in i.skills)
    //             {
    //                 if (skill is OnDamaged x1 && i == damaged.player && x1.IsValid(damaged)) await x1.Invoke(damaged);
    //                 else if (skill is OnEveryDamaged x2 && x2.IsValid(damaged)) await x2.Invoke(damaged);
    //             }
    //         }
    //     }

    //     public async Task OnUpdateUp(UpdateHp updateHp)
    //     {
    //         foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
    //         {
    //             foreach (var skill in i.skills)
    //             {
    //                 if (i == updateHp.player)
    //                 {
    //                     if (skill is AfterUpdateHp x1 && x1.IsValid(updateHp)) await x1.Invoke(updateHp);
    //                     else if (updateHp is Damaged damaged && skill is AfterDamaged x2 && x2.IsValid(damaged)) await x2.Invoke(damaged);
    //                     else if (updateHp is Recover recover && skill is AfterRecover x3 && x3.IsValid(recover)) await x3.Invoke(recover);
    //                     else if (updateHp.value < 0 && skill is AfterLoseHp x4 && x4.IsValid(updateHp)) await x4.Invoke(updateHp);
    //                 }

    //                 else if (updateHp is Damaged damaged && skill is OnEveryDamaged x && x.IsValid(damaged)) await x.Invoke(damaged);
    //             }
    //         }
    //     }

    //     public async Task OnUseCard(Card card)
    //     {
    //         foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
    //         {
    //             foreach (var skill in i.skills)
    //             {
    //                 if (skill is OnUseCard x1 && i == card.Src && x1.IsValid(card)) await x1.Invoke(card);
    //                 else if (skill is OnEveryUseCard x2 && x2.IsValid(card)) await x2.Invoke(card);
    //             }
    //         }
    //     }


    //     public async Task AfterUseCard(Card card)
    //     {
    //         foreach (var i in SgsMain.Instance.AlivePlayers.OrderBy(x => x.orderKey))
    //         {
    //             foreach (var skill in i.skills)
    //             {
    //                 if (skill is AfterUseCard x1 && i == card.Src && x1.IsValid(card)) await x1.Invoke(card);
    //                 else if (skill is AfterEveryUseCard x2 && x2.IsValid(card)) await x2.Invoke(card);
    //             }
    //         }
    //     }
    //     // public class EventCollection<T>
    //     // {
    //     //     public Dictionary<SkillEvent<T>,Skill> skillEvents = new();

    //     //     public async Task Invoke(T arg)
    //     //     {
    //     //         foreach(var (e,skill) in skillEvents.OrderBy(x=>x.Value.src.orderKey))
    //     //         {
    //     //             if(e.IsValid(arg)) await e.Invoke(arg);
    //     //         }
    //     //     }
    //     // }
    // }





    // public interface SkillEvent<TArg>
    // {
    //     public bool IsValid(TArg arg);
    //     public Task Invoke(TArg arg);
    //     // public virtual void a();
    // }

    // public interface SkillEvent<TArg1, TArg2>
    // {
    //     public bool IsValid(TArg1 arg1, TArg2 arg2);
    //     public Task Invoke(TArg1 arg1, TArg2 arg2);
    // }


    // public interface OnPhaseStart : SkillEvent<Phase> { }
    // public interface OnEveryPhaseStart : SkillEvent<Player, Phase> { }


    // public interface OnPhaseOver : SkillEvent<Phase> { }
    // public interface OnEveryPhaseOver : SkillEvent<Player, Phase> { }


    // public interface OnGetCardPhase : SkillEvent<GetCardFromPile> { }


    // public interface AfterGetCard : SkillEvent<GetCard> { }
    // public interface AfterGetCardFromElse : SkillEvent<GetCardFromElse> { }

    // public interface AfterLoseCard : SkillEvent<LoseCard> { }

    // public interface OnDamaged : SkillEvent<Damaged> { }
    // public interface OnEveryDamaged : SkillEvent<Damaged> { }

    // public interface AfterUpdateHp : SkillEvent<UpdateHp> { }
    // public interface AfterRecover : SkillEvent<Recover> { }
    // public interface AfterDamaged : SkillEvent<Damaged> { }
    // public interface AfterEveryDamaged : SkillEvent<Damaged> { }
    // public interface AfterLoseHp : SkillEvent<UpdateHp> { }

    // public interface OnUseCard : SkillEvent<Card> { }
    // public interface OnEveryUseCard : SkillEvent<Card> { }

    // public interface AfterUseCard : SkillEvent<Card> { }
    // public interface AfterEveryUseCard : SkillEvent<Card> { }

    // public interface OnTurnOver : SkillEvent<TurnOver> { }
    // public interface OnEveryTurnOver : SkillEvent<TurnOver> { }

    // public interface OnExecuteSha : SkillEvent<杀> { }
    // public interface OnExecuteJueDou : SkillEvent<决斗> { }

}
