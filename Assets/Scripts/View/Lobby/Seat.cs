using UnityEngine;

public class Seat : MonoBehaviour
{
    public User user;
    public GameObject already;
    public GameObject owner;

    private UserJson model;

    public void AddPlayer(UserJson model)
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