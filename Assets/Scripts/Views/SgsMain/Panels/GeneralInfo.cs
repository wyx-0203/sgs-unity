using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class GeneralInfo : SingletonMono<GeneralInfo>
    {
        public Image image;
        public Text generalName;
        public Transform skillParent;
        public GameObject skillPrefab;

        public async void Show(Model.General general, string skinId, string skinName)
        {
            generalName.text = "   " + skinName + "*" + general.name;
            for (int i = 0; i < general.skill.Count; i++)
            {
                var skill = Instantiate(skillPrefab, skillParent).GetComponent<SkillInfo>();
                skill.title.text = general.skill[i];
                skill.discribe.text = general.describe[i];
            }

            string url = Url.GENERAL_IMAGE + "Window/" + skinId + ".png";
            var texture = await WebRequest.GetTexture(url);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            foreach (Transform i in skillParent) if (i.name != "武将名" && i.name != "背景") Destroy(i.gameObject);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
        }
    }
}
