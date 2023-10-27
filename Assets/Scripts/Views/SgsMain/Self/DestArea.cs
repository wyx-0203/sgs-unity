using System.Collections.Generic;
using System.Linq;

namespace View
{
    public class DestArea : SingletonMono<DestArea>
    {
        private List<Dest> players;

        private List<Model.Player> SelectedPlayer => Model.Timer.Instance.temp.dests;
        // private Model.Skill skill => Model.Timer.Instance.temp.skill;
        // private Player self => ;
        private Model.Timer timer => Model.Timer.Instance;

        private int maxCount;
        private int minCount;
        public bool IsValid { get; private set; } = false;

        private void Start()
        {
            Model.Timer.Instance.StopTimerView += Reset;
            players = SgsMain.Instance.players.Select(x => x.GetComponent<Dest>()).ToList();
        }

        private void OnDestroy()
        {
            Model.Timer.Instance.StopTimerView -= Reset;
        }

        /// <summary>
        /// 初始化目标区
        /// </summary>
        public void OnStartPlay()
        {
            if (!CardArea.Instance.IsValid || !CardArea.Instance.ConvertIsValid) return;

            // 设置可选目标数量

            maxCount = timer.maxDest();
            minCount = timer.minDest();

            Update_();

            // 自动选择

            var validDests = players.Where(x => x.button.interactable);
            if (validDests.Count() == 1 && minCount == 1)
            {
                foreach (var i in validDests) i.OnClick();
            }
        }

        /// <summary>
        /// 重置目标区
        /// </summary>
        public void Reset()
        {
            if (!timer.players.Contains(SgsMain.Instance.self.model)) return;

            // 重置目标按键状态
            foreach (var i in players) i.Reset();

            IsValid = false;
        }

        public void Update_()
        {
            // 若已选中角色的数量超出范围，取消第一个选中的角色
            while (SelectedPlayer.Count > maxCount) players.Find(x => x.model == SelectedPlayer[0]).Unselect();

            IsValid = SelectedPlayer.Count >= minCount;
            if (maxCount == 0) return;

            foreach (var i in players)
            {
                // 每指定一个角色，都要更新不能指定的角色，例如明策指定一个目标后，第二个目标需在第一个的攻击范围内
                i.button.interactable = Model.Timer.Instance.isValidDest(i.model);

                // 对不能选择的角色设置阴影
                i.AddShadow();
            }
        }
    }
}
