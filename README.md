# Unity NFT Loot Boxes

Purchase NFT loot box [Pack](https://portal.thirdweb.com/pre-built-contracts/pack) NFTs from a [Marketplace](https://portal.thirdweb.com/pre-built-contracts/marketplace) and open them to reveal and receive a randomly selected NFT; either a common red gem, or a rare purple gem!

---

<p align="center">

**Assets in this repository are created by [Gabriel Aguiar](https://www.gabrielaguiarprod.com/).**

<a href="https://www.youtube.com/watch?v=CKtazkqsGRA">Gabriel's YouTube</a> •
<a href="https://twitter.com/GabrielAguiarFX">Gabriel's Twitter</a> •
<a href="https://www.gabrielaguiarprod.com/product-page/unity-vfx-loot-box-project">Purchase these assets ($10 USD)</a>

</p>

---

See the [live demo](https://unity-nft-lootboxes.thirdweb-example.com/) or view the preview below:

![Pack Preview](https://blog.thirdweb.com/content/images/2022/11/pack-preview-gif2.gif)

### Using This Template

Install [Blender](https://www.blender.org/download/) and follow our guide to [Setting Up Unity](https://blog.thirdweb.com/guides/get-started-with-thirdwebs-unity-sdk/)

## How It Works

**[View the full guide on our blog](https://blog.thirdweb.com/guides/create-in-game-nft-loot-boxes-in-unity/)**!

Below, we'll explore key areas of the codebase that enable the web3 functionality of this project using our [GamingKit](https://portal.thirdweb.com/gamingkit).

For information on the visual effects and animations, please see Gabriel's video: [How To Create A Loot Box](https://www.youtube.com/watch?v=CKtazkqsGRA).

The logic for all of the web3 functionality is contained in the [`Lootbox.cs`](/Assets/GabrielAguiarProductions/Scripts/LootBox.cs) script.

### Smart Contracts

Three smart contracts make up this project:

| Contract            | URL                                                                                                             |
| ------------------- | --------------------------------------------------------------------------------------------------------------- |
| Edition (NFT Items) | [View Smart Contract](https://thirdweb.com/optimism-goerli/0x73197DBbFFad473e6917dBE790b927B61E831219/nfts)     |
| Pack (Loot boxes)   | [View Smart Contract](https://thirdweb.com/optimism-goerli/0xd8Bd34726814855fB9cFF58fe5372558e3B411Cb/nfts)     |
| Marketplace         | [View Smart Contract](https://thirdweb.com/optimism-goerli/0x8ecE57a92ea312D5f31E39E5F6f3E6fC02507D7B/listings) |

The NFTs are bundled into the `Pack` NFTs, which are then sold on the `Marketplace`.

### Setting Up the SDK

To begin, we need to [instantiate the SDK](https://portal.thirdweb.com/gamingkit/setting-up/instantiating-the-sdk).

```csharp
using Thirdweb; // 1. Import the ThirdwebSDK

public class LootBox : MonoBehaviour
{
    // 2. Create a ThirdwebSDK instance for us to use throughout the class
    private ThirdwebSDK sdk;

    void Start()
    {
        // 3. When the game starts, set up the ThirdwebSDK
        sdk = new ThirdwebSDK("optimism-goerli");
    }
}
```

Now we can use the `sdk` instance throughout this class; allowing us to connect to user wallets, and interact with smart contracts.

### Connecting Wallets

We need users to connect their wallets to our game.

We achieve this by:

1. Requesting the user to connect their wallet
2. Requesting the user to switch to the correct network (`Optimism Goerli`):

```csharp
async Task<string> EnsureCorrectWalletState()
{
    string address = await sdk.wallet.Connect();
    await sdk.wallet.SwitchNetwork(420);
    return address;
}
```

### Purchasing Loot Boxes

We use the [Marketplace](https://portal.thirdweb.com/pre-built-contracts/marketplace) contract to purchase loot boxes from the connected wallet.

We achieve this by:

1. Connecting to the marketplace contract
2. Calling the `BuyListing` function.

```csharp
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
```

### Opening Packs

The [Pack](https://portal.thirdweb.com/pre-built-contracts/pack) contract is used to open loot boxes with the `Open` function. First, the user buys the pack, and then they can open it:

```csharp
async Task<ERC1155Reward> OpenPack()
{
    await EnsureCorrectWalletState();
    await BuyPackFromMarketplace();
    Pack packContract = await GetPackContract();
    // Here, 0 is the pack ID, and 1 is the amount of packs to open
    var result = await packContract.Open("0", "1");
    openedLootItem = result.erc1155Rewards[0];
    return result.erc1155Rewards[0];
}
```

The `Update` function listens for `Input.GetMouseButtonDown(0)` and calls the `OpenPack` function when the user clicks the mouse.

```csharp
if (Input.GetMouseButtonDown(0))
{
    var openedPack = await OpenPack();
    if (openedLootItem != null)
    {
        animator.SetBool("Open", true);
        cameraAnimator.SetBool("Open", true);
    }
}
```

Based on the token ID opened, we `Instantiate` the correct item (from red or purple):

```csharp
if (openedLootItem.tokenId == "0")
{
    loot = Instantiate(redGem);
}
if (openedLootItem.tokenId == "1")
{
    loot = Instantiate(purpleGem);
}
```

## Questions?

Jump into our [Discord](https://discord.com/invite/thirdweb) to speak with the team directly!
