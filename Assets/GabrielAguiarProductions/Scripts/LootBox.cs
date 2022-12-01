using System.Collections;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;

[RequireComponent(typeof (Collider))]
public class LootBox : MonoBehaviour
{
    public Animator cameraAnimator;

    public GameObject lootBox;

    public GameObject lootBoxFractured;

    public GameObject redGem;

    public GameObject purpleGem;

    private GameObject fracturedObject;

    private GameObject loot;

    private Animator animator;

    private RaycastHit hit;

    private Ray ray;

    private ThirdwebSDK sdk;

    private ERC1155Reward openedLootItem;

    public GameObject helperText;

    async Task<Pack> GetPackContract()
    {
        await EnsureCorrectWalletState();
        return sdk
            .GetContract("0xd8Bd34726814855fB9cFF58fe5372558e3B411Cb")
            .pack;
    }

    async Task<string> EnsureCorrectWalletState()
    {
        string address = await sdk.wallet.Connect();
        await sdk.wallet.SwitchNetwork(420);
        return address;
    }

    async Task<TransactionResult> BuyPackFromMarketplace()
    {
        await EnsureCorrectWalletState();
        Marketplace marketplace =
            sdk
                .GetContract("0x8ecE57a92ea312D5f31E39E5F6f3E6fC02507D7B")
                .marketplace;
        var result = await marketplace.BuyListing("0", 1);
        return result;
    }

    async Task<ERC1155Reward> OpenPack()
    {
        await EnsureCorrectWalletState();

        // Buy the pack first (demo users won't have any packs)
        await BuyPackFromMarketplace();
        Pack packContract = await GetPackContract();

        // Pack sometimes fails due to gas limit, so let's make sure we allow for enough gas.
        // Then open the pack
        var result = await packContract.Open("0", "1");
        Debug.Log("Result:");
        Debug.Log(result.erc1155Rewards[0].ToString());
        openedLootItem = result.erc1155Rewards[0];
        Debug.Log("Opened Loot Item:");
        Debug.Log(openedLootItem.ToString());
        return result.erc1155Rewards[0];
    }

    void Start()
    {
        sdk = new ThirdwebSDK("optimism-goerli");
        animator = GetComponent<Animator>();
    }

    async void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == gameObject.name)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Hover", true);

                cameraAnimator.SetBool("Idle", false);
                cameraAnimator.SetBool("Hover", true);

                if (Input.GetMouseButtonDown(0))
                {
                    var openedPack = await OpenPack();
                    if (openedLootItem != null)
                    {
                        animator.SetBool("Open", true);
                        cameraAnimator.SetBool("Open", true);
                    }
                }
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Hover", false);

                cameraAnimator.SetBool("Idle", true);
                cameraAnimator.SetBool("Hover", false);
            }
        }
    }

    //called via animation
    public void LootReward()
    {
        lootBox.SetActive(false);

        Debug.Log("Token ID: " + openedLootItem.tokenId);

        if (openedLootItem.tokenId == "0")
        {
            loot = Instantiate(redGem);
            UpdateHelperText("You opened a COMMON red gem. Click the restart icon to play again.");
        }

        if (openedLootItem.tokenId == "1")
        {
            loot = Instantiate(purpleGem);
            UpdateHelperText("You opened a RARE purple gem! Click the restart icon to play again.");
        }

        if (lootBoxFractured != null)
        {
            fracturedObject = Instantiate(lootBoxFractured) as GameObject;
        }
    }

    private void UpdateHelperText(string text)
    {
        helperText.GetComponent<TMPro.TextMeshProUGUI>().text = text;
    }

    public void Restart()
    {
        animator.Rebind();
        animator.SetBool("Idle", true);
        animator.SetBool("Open", false);
        animator.SetBool("Hover", false);

        cameraAnimator.Rebind();
        cameraAnimator.SetBool("Idle", true);
        cameraAnimator.SetBool("Open", false);
        cameraAnimator.SetBool("Hover", false);

        Destroy (loot);
        Destroy (fracturedObject);

        lootBox.SetActive(true);

        openedLootItem = null;
    }

    IEnumerator RestartCo()
    {
        yield return new WaitForFixedUpdate();
    }
}
