// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.UI;

// namespace View
// {
//     public class SelfPick : SingletonMono<SelfPick>
//     {
//         public GridLayoutGroup gridLayoutGroup;
//         public ContentSizeFitter contentSizeFitter;
//         public GameObject selfPick;

//         private Team selfTeam => Model.Self.Instance.team;

//         public Transform seatParent;
//         public GameObject seatPrefab;

//         public Button commit;
//         public GameObject border;

//         // private int length;
//         public SelfPickSeat[] seats => seatParent.GetComponentsInChildren<SelfPickSeat>();

//         private async void Start()
//         {
//             foreach (var i in BanPick.Instance.generals)
//             {
//                 if (i.state == GeneralBP.State.Ban) Destroy(i.gameObject);
//                 else if (i.state == GeneralBP.State.Self) i.transform.SetParent(gridLayoutGroup.transform);
//             }

//             selfPick.SetActive(true);
//             commit.onClick.AddListener(ClickCommit);

//             // await Util.WaitFrame(1);
//             // gridLayoutGroup.constraintCount = 5;
//             gridLayoutGroup.enabled = true;
//             contentSizeFitter.enabled = true;

//             await Util.WaitFrame(1);

//             contentSizeFitter.enabled = false;
//             gridLayoutGroup.enabled = false;

//             // pos0.sprite = selfTeam == Team.BLUE ? posSprites[3] : posSprites[1];
//             // pos1.sprite = selfTeam == Team.BLUE ? posSprites[0] : posSprites[2];
//             // var players = selfTeam.GetAllPlayers();
//             // length = players.Count();
//             foreach (var i in selfTeam.GetAllPlayers()) Instantiate(seatPrefab, seatParent).GetComponent<SelfPickSeat>().Init(i);

//             foreach (var i in BanPick.Instance.generals)
//             {
//                 if (i.state == GeneralBP.State.Self) i.ToSelfPick();
//             }
//         }

//         public void UpdateCommitButton()
//         {
//             commit.gameObject.SetActive(seats.FirstOrDefault(x => x.general is null) is null);
//         }

//         private async void ClickCommit()
//         {
//             border.SetActive(true);
//             await System.Threading.Tasks.Task.Yield();
//             Model.BanPick.Instance.SendSelfResult(selfTeam, seats.Select(x => x.general.model.id).ToList());
//             // int pos = selfTeam == Team.BLUE ? 3 : 1;
//             // Model.BanPick.Instance.SendSelfResult(pos, general0.Id);
//             // Model.BanPick.Instance.SendSelfResult(Util.TeammatePos(pos), general1.Id);
//             Destroy(gameObject);
//         }
//     }
// }
