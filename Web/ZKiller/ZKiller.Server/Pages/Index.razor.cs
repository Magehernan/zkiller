using Microsoft.AspNetCore.Components;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Hex.HexTypes;
using Nethereum.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZKiller.Contracts.ZKillerGame;
using ZKiller.Contracts.ZKillerGame.ContractDefinition;

namespace ZKiller.Server.Pages;
public record Game(int Id, GamesOutputDTO Info);

public partial class Index : IDisposable {
	[Inject]
	private SelectedEthereumHostProviderService SelectedHostProviderService { get; set; } = default!;


	private IEthereumHostProvider ethereumHostProvider = default!;
	private ZKillerGameService zkillerGameService = default!;
	private readonly List<Game> games = [];

	private Game? currentGame;
	private long selectedNetwork;

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		ethereumHostProvider = SelectedHostProviderService.SelectedHost;
		ethereumHostProvider.EnabledChanged += HostProviderOnEnabledChanged;
		ethereumHostProvider.NetworkChanged += NetworkChanged;
		//scroll zkillerGameService = new(await ethereumHostProvider.GetWeb3Async(), "0x2354C0ca250D8E461781a523B4043a71d482b55C");
		zkillerGameService = new(await ethereumHostProvider.GetWeb3Async(), "0xA1e4CEaa9924e42680b3d6a4658eB637d4B42DA7");
	}

	public void Dispose() {
		ethereumHostProvider.EnabledChanged -= HostProviderOnEnabledChanged;
		ethereumHostProvider.NetworkChanged -= NetworkChanged;

		GC.SuppressFinalize(this);
	}

	private Task NetworkChanged(long arg) {
		selectedNetwork = arg;
		StateHasChanged();
		return Task.CompletedTask;
	}

	protected override async Task OnAfterRenderAsync(bool firstTime) {
		if (!firstTime) {
			return;
		}


		await GetChainId();

		await LoadGamesAsync();
	}

	private async Task LoadGamesAsync() {
		System.Numerics.BigInteger lastGame = await zkillerGameService.LastGameQueryAsync();
		int from = (int)lastGame;
		games.Clear();
		for (int i = from; i > 0; i--) {
			GamesOutputDTO game = await zkillerGameService.GamesQueryAsync(i);
			if (game.Status > 0) {
				continue;
			}
			games.Add(new(i, game));
		}
		StateHasChanged();
	}

	private async Task HostProviderOnEnabledChanged(bool enabled) {
		if (enabled) {
			await GetChainId();
			StateHasChanged();
		}
	}

	private async Task GetChainId() {
		Nethereum.Web3.IWeb3 web3 = await ethereumHostProvider.GetWeb3Async();
		HexBigInteger chainId = await web3.Eth.ChainId.SendRequestAsync();
		selectedNetwork = chainId.ToLong();
		StateHasChanged();
	}

	private async Task NewGameAsync() {
		try {
			Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt = await zkillerGameService.NewGameRequestAndWaitForReceiptAsync(
				[
					"0xedD31e732EA38E95f0637634FE1EBb3Ca5055979",
					"0x12e1079ECEBB9e96ACaB873e5F2B788E805d4C41",
					"0x666192B083e809C832F88EAf24120f7188AE77B8"
				], "0xedD31e732EA38E95f0637634FE1EBb3Ca5055979");
			await LoadGamesAsync();
		} catch { }
	}

	private void PlayGame(Game game) {
		currentGame = game;
	}
}