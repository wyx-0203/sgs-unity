using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class BanPick : SingletonMono<BanPick>
    {
        public List<GeneralBP> Pool;
        public List<GeneralBP> All { get; private set; }
        public GameObject SelfPool;
        // 倒计时读条
        public Slider timer;
        // 提示
        public Text hint;

        private Model.BanPick model => Model.BanPick.Instance;
        private bool selfTeam => Model.Self.Instance.team;

        private void Start()
        {
            for (int i = 0; i < 12; i++) Pool[i].Init(model.Pool[i]);
            All = new List<GeneralBP>(Pool);

            model.StartPickView += StartPick;
            model.OnPickView += OnPick;
            model.StartBanView += StartBan;
            model.OnBanView += OnBan;
            model.StartSelfPickView += SelfPick;
            Model.SgsMain.Instance.GeneralView += Destroy;
        }

        private void OnDestroy()
        {
            model.StartPickView -= StartPick;
            model.OnPickView -= OnPick;
            model.StartBanView -= StartBan;
            model.OnBanView -= OnBan;
            model.StartSelfPickView -= SelfPick;
            Model.SgsMain.Instance.GeneralView -= Destroy;
        }

        private void StartPick()
        {
            StartCoroutine(StartTimer(model.second));
            if (model.Current.isSelf)
            {
                foreach (var i in Pool) i.button.interactable = true;
                hint.text = "请点击选择武将";
            }

            else
            {
                foreach (var i in Pool) i.button.interactable = false;
                hint.text = model.Current.team == selfTeam ? "等待队友选将" : "等待对方选将";
            }
        }

        private void OnPick(int id)
        {
            StopAllCoroutines();

            var general = Pool.Find(x => x.Id == id);
            if (general is null) return;

            general.button.interactable = false;
            Pool.Remove(general);
            if (model.Current.team == selfTeam)
            {
                general.transform.SetParent(SelfPool.transform, false);
            }

            general.OnPick(model.Current.team == selfTeam);
        }

        private void StartBan()
        {
            StartCoroutine(StartTimer(model.second));

            if (model.Current.isSelf)
            {
                foreach (var i in Pool) i.button.interactable = true;
                hint.text = "请点击禁用武将";
            }
            else
            {
                foreach (var i in Pool) i.button.interactable = false;
                hint.text = model.Current.team == selfTeam ? "等待队友禁将" : "等待对方禁将";
            }
        }

        private void OnBan(int id)
        {
            StopAllCoroutines();

            var general = Pool.Find(x => x.Id == id);
            if (general is null) return;

            general.button.interactable = false;
            Pool.Remove(general);
            general.OnBan();
        }

        private void SelfPick()
        {
            hint.text = "请选择己方要出场的武将";
            StartCoroutine(StartTimer(model.second));

            SelfPool.SetActive(false);
            GetComponent<SelfPick>().enabled = true;
        }

        private void Destroy()
        {
            Destroy(gameObject);
        }

        private IEnumerator StartTimer(int second)
        {
            timer.value = 1;
            while (timer.value > 0)
            {
                timer.value -= Time.deltaTime / second;
                yield return null;
            }
        }
    }
}
