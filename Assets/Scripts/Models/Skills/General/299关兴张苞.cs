using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

public class 父魂 : Converted
{
    public override int MaxCard => 2;
    public override int MinCard => 2;

    public override Card Convert(List<Card> cards) => Card.Convert<杀>(cards);

    public override Card Use(List<Card> cards)
    {
        primitives = cards;
        return base.Use(cards);
    }

    private List<Card> primitives;

    public override bool IsValidCard(Card card) => card.isHandCard && base.IsValidCard(card);

    public override bool OnEveryDamaged(Damaged damaged) =>
        damaged.Src == src
        && damaged.SrcCard is 杀 sha
        && sha.PrimiTives == primitives
        && TurnSystem.Instance.CurrentPlayer == src
        && TurnSystem.Instance.CurrentPhase == Phase.Play;

    public override async Task Invoke(object arg)
    {
        await Task.Yield();
        Execute();
        new UpdateSkill(src, new List<string> { "武圣", "咆哮" }).Add();
    }

    private bool invoked;

    protected override void ResetAfterTurn()
    {
        if (!invoked) return;
        invoked = false;
        new UpdateSkill(src, new List<string> { "武圣", "咆哮" }).Remove();
    }
}
