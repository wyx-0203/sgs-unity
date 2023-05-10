namespace Model
{
    public class CardArea : Singleton<CardArea>
    {
        public bool ValidCard(Card card)
        {
            var player = TurnSystem.Instance.CurrentPlayer;
            if (!player.HandCards.Contains(card) && !card.IsConvert) return false;
            if (player.DisabledCard(card)) return false;
            switch (card.Name)
            {
                case "闪":
                case "无懈可击":
                    return false;

                case "桃":
                    return player.Hp < player.HpLimit;

                case "杀":
                case "雷杀":
                case "火杀":
                    return UseSha(player, card);

                case "闪电":
                    foreach (var i in player.JudgeArea) if (i is 闪电) return false;
                    return true;

                case "酒":
                    return player.酒Count < 1 || player.UnlimitedCard(card);

                default:
                    return true;
            }
        }

        public bool UseSha(Player player, Card card = null)
        {
            if (card is null) card = Card.Convert<杀>();
            return player.杀Count < 1 || player.UnlimitedCard(card);
        }
    }
}
