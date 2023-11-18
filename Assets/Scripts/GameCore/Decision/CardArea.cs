namespace GameCore
{
    public class CardArea : Singleton<CardArea>
    {
        public bool ValidCard(Card card)
        {
            var player = TurnSystem.Instance.CurrentPlayer;
            if (!card.isHandCard && !card.isConvert || !card.useable) return false;

            switch (card.name)
            {
                case "闪":
                case "无懈可击":
                    return false;

                case "桃":
                    return player.hp < player.hpLimit;

                case "杀":
                case "雷杀":
                case "火杀":
                    return player.shaCount < 1 || player.effects.NoTimesLimit.Invoke(card);
                // return player.shaCount < 1 || NoTimesLimit.Instance.Invoke(player, card);

                case "闪电":
                    return player.JudgeCards.Find(x => x is 闪电) is null;

                case "酒":
                    return player.jiuCount < 1 || player.effects.NoTimesLimit.Invoke(card);
                // return player.JiuCount < 1 || NoTimesLimit.Instance.Invoke(player, card);

                default:
                    return true;
            }
        }
    }
}
