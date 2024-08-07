using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Nethereum.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZKiller.Contracts.ZKillerGame;
using ZKiller.Contracts.ZKillerGame.ContractDefinition;

namespace ZKiller.Server.Pages;
public partial class Play {
	[Parameter, EditorRequired]
	public required ZKillerGameService Contract { get; set; }

	[Parameter, EditorRequired]
	public required Game Game { get; set; }
	[Parameter, EditorRequired]
	public required EventCallback Exit { get; set; }


	[Inject]
	private SelectedEthereumHostProviderService SelectedHostProviderService { get; set; } = default!;
	[Inject]
	private ModalService ModalService { get; set; } = default!;

	private GamesOutputDTO CurrentGame => Game.Info;

	private List<string>? alivePlayers;
	private List<string>? players;
	private bool alreadyVote = false;

	public override async Task SetParametersAsync(ParameterView parameters) {
		bool hasChanges = parameters.IsParameterChanged(nameof(Game), Game);
		await base.SetParametersAsync(parameters);
		if (hasChanges) {
			await LoadAsync();
			players = await Contract.GetPlayersQueryAsync(Game.Id);
			if (!IAmPlayer(players)) {
				ModalService.Error(new ConfirmOptions() {
					Title = "Sorry",
					Content = "you are not a player or the Zkiller already kill you"
				});
				await Exit.InvokeAsync();
			}
		}
	}

	private async Task LoadAsync() {
		Game = new(Game.Id, await Contract.GamesQueryAsync(Game.Id));
		alreadyVote = await Contract.PlayerTurnVoteQueryAsync(Game.Id, Game.Info.Turn, SelectedHostProviderService.SelectedHost.SelectedAccount);
		await InvokeAsync(StateHasChanged);

		if (Game.Info.Status != 0) {
			return;
		}

		if (alreadyVote) {
			await Task.Delay(2000);
			_ = LoadAsync();
			return;
		}
		alivePlayers = await Contract.GetAlivePlayersQueryAsync(Game.Id);
		await InvokeAsync(StateHasChanged);
	}

	private bool IAmPlayer(List<string> players) {
		foreach (string player in players) {
			if (IsPlayerAddress(player)) { return true; }
		}
		return false;
	}

	private bool IsPlayerAddress(string player) {
		return player.Equals(SelectedHostProviderService.SelectedHost.SelectedAccount, StringComparison.OrdinalIgnoreCase);
	}

	private bool IsPlayerKiller() {
		return CurrentGame.Killer.Equals(SelectedHostProviderService.SelectedHost.SelectedAccount, StringComparison.OrdinalIgnoreCase);
	}

	private async Task VoteAsync(string player) {
		try {
			await Contract.VoteRequestAndWaitForReceiptAsync(Game.Id, player);
		} catch { }
		await Task.Delay(5000);
		await LoadAsync();
	}
}