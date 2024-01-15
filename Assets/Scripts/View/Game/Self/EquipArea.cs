using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Model;

public class EquipArea : SingletonMono<EquipArea>
{
    public Dictionary<string, Equipment> equipments { get; private set; }
    private SinglePlayQuery current => PlayArea.Instance.current;
    private PlayQuery playQuery => PlayArea.Instance.playQuery;
    private int self => GameMain.Instance.firstPerson.model.index;

    private void Start()
    {
        var parent = transform.Find("EquipmentArea");
        equipments = new Dictionary<string, Equipment>
        {
            {"武器", parent.transform.Find("武器").GetComponent<Equipment>()},
            {"防具", parent.transform.Find("防具").GetComponent<Equipment>()},
            {"加一马", parent.transform.Find("加一马").GetComponent<Equipment>()},
            {"减一马", parent.transform.Find("减一马").GetComponent<Equipment>()}
        };

        EventSystem.Instance.AddEvent<AddEquipment>(OnAddEquipment);
        EventSystem.Instance.AddEvent<LoseCard>(OnRemoveEquipment);
        EventSystem.Instance.AddEvent<FinishPlay>(Reset);

        GameMain.Instance.OnChangeView += OnChangeView;
    }

    private void OnDestroy()
    {
        EventSystem.Instance.RemoveEvent<AddEquipment>(OnAddEquipment);
        EventSystem.Instance.RemoveEvent<LoseCard>(OnRemoveEquipment);
        EventSystem.Instance.RemoveEvent<FinishPlay>(Reset);
    }

    public void OnStartPlay()
    {
        foreach (var i in equipments.Values.Where(x => x.gameObject.activeSelf))
        {
            // 若可发动主动技能，则可选。例如丈八蛇矛
            if (!playQuery.skills.All(x => x.skillName != i.name))
            {
                i.toggle.interactable = true;
            }
            // 询问是否发动技能，则选中。例如八卦阵
            else if (current.skillName == i.name) i.toggle.isOn = true;
            // 可选装备牌。例如制衡
            else if (current.cards.Contains(i.id)) i.toggle.interactable = true;
        }
    }

    public void ResetBySkill(string skill)
    {
        foreach (var card in equipments.Values.Where(x => x.name != skill)) card.Reset();
    }

    private void Reset(FinishPlay finishPlay)
    {
        if (Player.IsSelf(finishPlay.player))
        {
            // 重置装备牌状态
            foreach (var card in equipments.Values) card.Reset();
        }
    }

    public void OnClickEquipment(Equipment equipment, bool value)
    {
        if (!playQuery.skills.All(x => x.skillName != equipment.name))
        {
            SkillArea.Instance.OnClickSkill(equipment.name, value);
            // Debug.Log(value);
        }
        else if (current.cards.Contains(equipment.id)) CardArea.Instance.OnClickCard(equipment.id, value);
    }

    public void OnChangeView(Model.Player player)
    {
        foreach (var i in equipments)
        {
            if (player.Equipments.ContainsKey(i.Key)) i.Value.Show(player.Equipments[i.Key].id);
            else i.Value.Hide();
        }
    }

    public void OnAddEquipment(AddEquipment addEquipment)
    {
        if (addEquipment.player != self) return;
        int id = addEquipment.card;
        equipments[Model.Card.Find(id).type].Show(id);
    }

    public void OnRemoveEquipment(LoseCard loseCard)
    {
        if (loseCard.player != self) return;
        foreach (var i in equipments.Values.Where(x => loseCard.cards.Contains(x.id))) i.Hide();
    }
}