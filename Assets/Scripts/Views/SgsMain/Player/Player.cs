using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

namespace View
{
    public class Player : MonoBehaviour
    {
        // 位置
        public Image positionImage;
        public Sprite[] positionSprite;

        public General general;

        // 阵营
        public Image team;
        public Sprite[] teamSprites;

        // 濒死状态
        public Image nearDeath;
        // 阵亡
        public Image death;
        public Sprite selfDead;
        public Sprite oppoDead;
        public Material gray;

        // 横置
        public GameObject Lock;
        // 翻面
        public GameObject TurnOver;

        // 回合内边框
        public Image turnBorder;

        // 判定区
        public Transform judgeArea;
        public GameObject judgeCardPrefab;

        public Model.Player model { get; private set; }

        private void Start()
        {
            // 武将
            // Model.SgsMain.Instance.AfterBanPickView += InitGeneral;

            // 回合
            Model.TurnSystem.Instance.StartTurnView += StartTurn;
            Model.TurnSystem.Instance.FinishTurnView += FinishTurn;

            // 改变体力
            Model.UpdateHp.ActionView += UpdateHp;
            Model.UpdateHp.ActionView += NearDeath;

            // 阵亡
            Model.Die.ActionView += OnDead;

            // 判定区
            if (judgeArea.gameObject.activeSelf)
            {
                Model.DelayScheme.AddJudgeView += AddJudgeCard;
                Model.DelayScheme.RemoveJudgeView += RemoveJudgeCard;
            }

            // 横置
            Model.SetLock.ActionView += OnLock;
            // 翻面
            Model.TurnOver.ActionView += OnTurnOver;

            // 换肤
            model.ChangeSkinView += general.UpdateSkin;
        }

        private void OnDestroy()
        {
            // 武将
            // Model.SgsMain.Instance.AfterBanPickView -= InitGeneral;

            // 回合
            Model.TurnSystem.Instance.StartTurnView -= StartTurn;
            Model.TurnSystem.Instance.FinishTurnView -= FinishTurn;

            // 改变体力
            Model.UpdateHp.ActionView -= UpdateHp;
            Model.UpdateHp.ActionView -= NearDeath;

            // 阵亡
            Model.Die.ActionView -= OnDead;

            // 判定区
            if (judgeArea.gameObject.activeSelf)
            {
                Model.DelayScheme.AddJudgeView -= AddJudgeCard;
                Model.DelayScheme.RemoveJudgeView -= RemoveJudgeCard;
            }

            // 横置
            Model.SetLock.ActionView -= OnLock;
            Model.TurnOver.ActionView -= OnTurnOver;

            model.ChangeSkinView -= general.UpdateSkin;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Model.Player player)
        {
            model = player;
            positionImage.sprite = positionSprite[model.turnOrder];

            // 2v2
            if (Model.Mode.Instance is Model.TwoVSTwo) team.sprite = model.isSelf ? teamSprites[0] : teamSprites[1];

            // 3v3
            else if (Model.Mode.Instance is Model.ThreeVSThree)
            {
                // 主将
                if (model.isMaster) team.sprite = model.team == Team.BLUE ? teamSprites[2] : teamSprites[3];

                // 先锋
                else team.sprite = model.team == Team.BLUE ? teamSprites[4] : teamSprites[5];
            }

            nearDeath.gameObject.SetActive(model.Hp < 1 && model.HpLimit > 0);
            death.gameObject.SetActive(false);
            general.skin.material = null;

            general.Init(model);
        }

        private void InitGeneral()
        {
            // 显示势力和体力
            general.nation.transform.parent.gameObject.SetActive(true);
            general.imageGroup.transform.parent.gameObject.SetActive(true);

            general.Init(model);
        }

        private void UpdateHp(Model.UpdateHp operation)
        {
            if (operation.player != model) return;
            general.SetHp(model.Hp, model.HpLimit);
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
            death.sprite = model.isSelf ? selfDead : oppoDead;
        }

        private void StartTurn()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;
            turnBorder.gameObject.SetActive(true);
        }

        private void FinishTurn()
        {
            if (Model.TurnSystem.Instance.CurrentPlayer != model) return;
            turnBorder.gameObject.SetActive(false);
        }

        private void AddJudgeCard(Model.DelayScheme card)
        {
            if (card.Owner != model) return;

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
            Lock.SetActive(setLock.player.locked);
        }

        private void OnTurnOver(Model.TurnOver turnOver)
        {
            if (turnOver.player != model) return;
            TurnOver.SetActive(model.turnOver);
        }
    }
}