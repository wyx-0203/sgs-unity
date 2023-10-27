using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace View
{
    public class EquipArea : SingletonMono<EquipArea>
    {
        public Dictionary<string, Equipage> Equips { get; private set; }
        private Model.Timer timer => Model.Timer.Instance;
        // private Model.Skill skill => Model.Timer.Instance.temp.skill;

        private Player self => SgsMain.Instance.self;

        private void Start()
        {
            var parent = transform.Find("装备区");
            Equips = new Dictionary<string, Equipage>
            {
                {"武器", parent.transform.Find("武器").GetComponent<Equipage>()},
                {"防具", parent.transform.Find("防具").GetComponent<Equipage>()},
                {"加一马", parent.transform.Find("加一马").GetComponent<Equipage>()},
                {"减一马", parent.transform.Find("减一马").GetComponent<Equipage>()}
            };

            Model.Equipment.AddEquipView += AddEquip;
            Model.Equipment.RemoveEquipView += RemoveEquip;
            Model.Timer.Instance.StopTimerView += Reset;

            Model.SgsMain.Instance.MoveSeatView += MoveSeat;
        }

        private void OnDestroy()
        {
            Model.Equipment.AddEquipView -= AddEquip;
            Model.Equipment.RemoveEquipView -= RemoveEquip;
            Model.Timer.Instance.StopTimerView -= Reset;

            Model.SgsMain.Instance.MoveSeatView -= MoveSeat;
        }

        public void OnStartPlay()
        {
            var equipSkill = Model.Timer.Instance.equipSkill;
            if (equipSkill != null && Equips.ContainsKey(equipSkill.name)) Equips[equipSkill.name].Use();

            if (timer.maxCard == 0)
            {
                foreach (var i in Equips.Values) i.button.interactable = false;
            }
            else
            {
                foreach (var i in Equips.Values) i.button.interactable = i.gameObject.activeSelf && timer.isValidCard(i.model);

            }

            if (Equips["武器"].model is Model.丈八蛇矛 zbsm && (zbsm.skill.IsValid || Model.Timer.Instance.temp.skill == zbsm.skill))
            {
                Equips["武器"].button.interactable = true;
            }
        }

        public void Reset()
        {
            if (!timer.players.Contains(self.model)) return;

            // 重置装备牌状态
            foreach (var card in Equips.Values) card.Reset();
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in Equips)
            {
                if (!model.Equipments.ContainsKey(i.Key)) i.Value.gameObject.SetActive(false);
                else
                {
                    i.Value.gameObject.SetActive(true);
                    i.Value.Init(model.Equipments[i.Key]);
                }
            }
        }

        public void AddEquip(Model.Equipment card)
        {
            if (card.Owner != self.model) return;

            Equips[card.type].gameObject.SetActive(true);
            Equips[card.type].Init(card);
        }

        public void RemoveEquip(Model.Equipment card)
        {
            if (card.Owner != self.model) return;
            if (card.id != Equips[card.type].Id) return;

            Equips[card.type].model = null;
            Equips[card.type].gameObject.SetActive(false);
        }
    }
}