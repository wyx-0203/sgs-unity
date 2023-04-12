using UnityEngine;

namespace View
{
    public class Seat : MonoBehaviour
    {
        public User user;
        public GameObject already;
        public GameObject owner;

        private Model.User model;

        public void AddPlayer(Model.User model)
        {
            if (model is null)
            {
                user.gameObject.SetActive(false);
                return;
            }
            this.model = model;

            user.gameObject.SetActive(true);
            user.Init(model);
            UpdateStatus();
        }

        public void RemovePlayer()
        {
            model = null;

            user.gameObject.SetActive(false);
            already.SetActive(false);
            owner.SetActive(false);
        }

        public void UpdateStatus()
        {
            already.SetActive(model.already);
            owner.SetActive(model.owner);
        }
    }
}