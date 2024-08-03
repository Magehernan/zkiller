using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Nethereum.Hex.HexTypes;
using Nethereum.UI;
using System;
using System.Threading.Tasks;
using ZKiller.Contracts.ZKillerGame;

namespace ZKiller.Server.Pages;
public partial class Index {
	[CascadingParameter]
	public Task<AuthenticationState> AuthenticationState { get; set; }

	private bool EthereumAvailable { get; set; }
	private string SelectedAccount { get; set; }
	private long SelectedChainId { get; set; }

	private IEthereumHostProvider ethereumHostProvider;
	private ZKillerGameService zkillerGameService;

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		ethereumHostProvider = selectedHostProviderService.SelectedHost;
		ethereumHostProvider.SelectedAccountChanged += HostProvider_SelectedAccountChanged;
		ethereumHostProvider.NetworkChanged += HostProvider_NetworkChanged;
		ethereumHostProvider.EnabledChanged += HostProviderOnEnabledChanged;
		zkillerGameService = new(await ethereumHostProvider.GetWeb3Async(), "0xF7c9589564990ca244DEAD198db3Db1A310E8225");
	}

	public void Dispose() {
		ethereumHostProvider.SelectedAccountChanged -= HostProvider_SelectedAccountChanged;
		ethereumHostProvider.NetworkChanged -= HostProvider_NetworkChanged;
		ethereumHostProvider.EnabledChanged -= HostProviderOnEnabledChanged;

		GC.SuppressFinalize(this);
	}

	protected override async Task OnAfterRenderAsync(bool firstTime) {
		if (!firstTime) {
			return;
		}

		EthereumAvailable = await ethereumHostProvider.CheckProviderAvailabilityAsync();

		if (EthereumAvailable) {
			SelectedAccount = await ethereumHostProvider.GetProviderSelectedAccountAsync();
			await GetChainId();
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
		SelectedChainId = (int)chainId.Value;
	}

	private async Task HostProvider_SelectedAccountChanged(string account) {
		SelectedAccount = account;
		await GetChainId();
		StateHasChanged();
	}

	private Task HostProvider_NetworkChanged(long chainId) {
		SelectedChainId = chainId;
		StateHasChanged();
		return Task.CompletedTask;
	}

	private async Task NewGameAsync() {
		try {
			Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt = await zkillerGameService.NewGameRequestAndWaitForReceiptAsync(
				[
					"0xedD31e732EA38E95f0637634FE1EBb3Ca5055979",
					"0x12e1079ECEBB9e96ACaB873e5F2B788E805d4C41",
					"0x666192B083e809C832F88EAf24120f7188AE77B8"
				], "0xedD31e732EA38E95f0637634FE1EBb3Ca5055979");
		} catch { }
	}
}