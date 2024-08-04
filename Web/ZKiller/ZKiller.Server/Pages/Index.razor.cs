using Microsoft.AspNetCore.Components;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZKiller.Contracts.ZKillerGame;
using ZKiller.Contracts.ZKillerGame.ContractDefinition;

namespace ZKiller.Server.Pages;
public record Game(int Id, GamesOutputDTO Info);

public partial class Index {
	[Inject]
	private SelectedEthereumHostProviderService SelectedHostProviderService { get; set; } = default!;


	private IEthereumHostProvider ethereumHostProvider = default!;
	private ZKillerGameService zkillerGameService = default!;
	private readonly List<Game> games = [];

	private Game? currentGame;

	private const string contractAddress = "0x2354C0ca250D8E461781a523B4043a71d482b55C"; //scroll
																						 //private const string contractAddress = "0xA1e4CEaa9924e42680b3d6a4658eB637d4B42DA7"; //geth

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		ethereumHostProvider = SelectedHostProviderService.SelectedHost;
		zkillerGameService = new(await ethereumHostProvider.GetWeb3Async(), contractAddress);
	}

	protected override async Task OnAfterRenderAsync(bool firstTime) {
		if (!firstTime) {
			return;
		}

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

	private async Task NewGameAsync() {
		try {
			List<string> players = [
				"0xedD31e732EA38E95f0637634FE1EBb3Ca5055979",
				"0x12e1079ECEBB9e96ACaB873e5F2B788E805d4C41",
				//"0x666192B083e809C832F88EAf24120f7188AE77B8",
				"0xD16B43DB0F469A5CE11dbfDc133238e51390407c",
			];

			TransactionReceipt receipt = await zkillerGameService
				.NewGameRequestAndWaitForReceiptAsync(players, players[Random.Shared.Next(players.Count)]);
			await Task.Delay(5000);
			await LoadGamesAsync();
		} catch { }
	}

	private void PlayGame(Game game) {
		currentGame = game;
	}
}