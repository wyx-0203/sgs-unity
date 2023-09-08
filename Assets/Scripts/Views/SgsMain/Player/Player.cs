using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace View
{
    public class Player : MonoBehaviour
    {
        // 位置
        public bool IsSelf { get; private set; }
        public Image positionImage;
        public Sprite[] positionSprite;

        public General general;

        // 武将
        public Material gray;
        public Image team;
        public Sprite selfSprite;
        public Sprite oppoSprite;

        // 濒死状态
        public Image nearDeath;
        // 阵亡
        public Image death;
        public Image deadText;
        public Sprite selfDead;
        public Sprite oppoDead;

        //横置
        public GameObject Lock;

        // 回合内边框
        public Image turnBorder;

        // 判定区
        public Transform judgeArea;
        public GameObject judgeCardPrefab;


        public Model.Player model { get; private set; }
        // private List<Model.Skin> skins;
        // public Model.Skin CurrentSkin { get; private set; }
        // public Dictionary<int, Dictionary<string, List<string>>> voices;

        private void Start()
        {
            // 武将
            Model.SgsMain.Instance.GeneralView += InitGeneral;

            // 回合
            Model.TurnSystem.Instance.StartTurnView += StartTurn;
            Model.TurnSystem.Instance.FinishTurnView += FinishTurn;

            // 改变体力
            Model.UpdateHp.ActionView += UpdateHp;
            Model.UpdateHp.ActionView += NearDeath;

            // 阵亡
            Model.Die.ActionView += OnDead;

            // 判定区
            Model.DelayScheme.AddJudgeView += AddJudgeCard;
            Model.DelayScheme.RemoveJudgeView += RemoveJudgeCard;

            // 横置
            Model.SetLock.ActionView += OnLock;

            // 换肤
            model.ChangeSkinView += general.UpdateSkin;
        }

        private void OnDestroy()
        {
            // 武将
            Model.SgsMain.Instance.GeneralView -= InitGeneral;

            // 回合
            Model.TurnSystem.Instance.StartTurnView -= StartTurn;
            Model.TurnSystem.Instance.FinishTurnView -= FinishTurn;

            // 改变体力
            Model.UpdateHp.ActionView -= UpdateHp;
            Model.UpdateHp.ActionView -= NearDeath;

            // 阵亡
            Model.Die.ActionView -= OnDead;

            // 判定区
            Model.DelayScheme.AddJudgeView -= AddJudgeCard;
            Model.DelayScheme.RemoveJudgeView -= RemoveJudgeCard;

            // 横置
            Model.SetLock.ActionView -= OnLock;

            model.ChangeSkinView += general.UpdateSkin;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Model.Player player)
        {
            model = player;
            IsSelf = model.isSelf;
            positionImage.sprite = positionSprite[model.position];
            team.sprite = IsSelf ? selfSprite : oppoSprite;
        }

        private void InitGeneral()
        {
            // 显示势力和体力
            general.nation.transform.parent.gameObject.SetActive(true);
            general.imageGroup.transform.parent.gameObject.SetActive(true);

            general.Init(model.general);
        }

        private void UpdateHp(Model.UpdateHp operation)
        {
            if (operation.player != model) return;
            general.UpdateHp(model.Hp, model.HpLimit);
        }

        private void NearDeath(Model.UpdateHp operation)
        {
            if (operation.player != model) return;
            nearDeath.gameObject.SetActive(model.Hp < 1);
        }

        private void OnDead(Model.Die operation)
        {
            if (operation.player != model) return;
            nearDeath.gameObject.SetActive(false);
            general.skin.material = gray;
            death.gameObject.SetActive(true);
            deadText.sprite = IsSelf ? selfDead : oppoDead;
        }

        private void StartTurn()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;

            turnBorder.gameObject.SetActive(true);
            if (!IsSelf) positionImage.gameObject.SetActive(false);
        }

        private void FinishTurn()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;

            turnBorder.gameObject.SetActive(false);
            positionImage.gameObject.SetActive(true);
        }

        private void AddJudgeCard(Model.DelayScheme card)
        {
            if (card.Owner != model) return;

            // var instance = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("判定牌");
            var instance = Instantiate(judgeCardPrefab);
            instance.transform.SetParent(judgeArea, false);
            instance.name = card.name;
            instance.GetComponent<Image>().sprite = Sprites.Instance.judgeCard[card.name];
        }

        private void RemoveJudgeCard(Model.DelayScheme card)
        {
            if (card.Owner != model) return;

            Destroy(judgeArea.Find(card.name)?.gameObject);
        }

        private void OnLock(Model.SetLock setLock)
        {
            if (setLock.player != model) return;
            Lock.SetActive(setLock.player.IsLocked);
        }

        // private void ChangeSkin()
        // {
        //     general.UpdateSkin()
        // }
    }
}