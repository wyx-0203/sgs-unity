using System.Threading.Tasks;

namespace Model
{
    public class 咆哮 : Triggered
    {
        public override bool isObey => true;

        public bool Effect(Card card) => card is 杀;

        public override void OnEnable()
        {
            Src.unlimitTimes += Effect;
            Src.events.WhenUseCard.AddEvent(Src, Execute);
        }

        public override void OnDisable()
        {
            Src.unlimitTimes -= Effect;
            Src.events.WhenUseCard.RemoveEvent(Src);
        }

        public async Task Execute(Card card)
        {
            if (card is 杀 && Src.杀Count > 0) await Execute();
        }
    }
}