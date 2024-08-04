using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using ZKiller.Contracts.ZKillerGame.ContractDefinition;

namespace ZKiller.Contracts.ZKillerGame
{
    public partial class ZKillerGameService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, ZKillerGameDeployment zKillerGameDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<ZKillerGameDeployment>().SendRequestAndWaitForReceiptAsync(zKillerGameDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, ZKillerGameDeployment zKillerGameDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<ZKillerGameDeployment>().SendRequestAsync(zKillerGameDeployment);
        }

        public static async Task<ZKillerGameService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, ZKillerGameDeployment zKillerGameDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, zKillerGameDeployment, cancellationTokenSource);
            return new ZKillerGameService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3{ get; }

        public ContractHandler ContractHandler { get; }

        public ZKillerGameService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public ZKillerGameService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<GamesOutputDTO> GamesQueryAsync(GamesFunction gamesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GamesFunction, GamesOutputDTO>(gamesFunction, blockParameter);
        }

        public Task<GamesOutputDTO> GamesQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var gamesFunction = new GamesFunction();
                gamesFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryDeserializingToObjectAsync<GamesFunction, GamesOutputDTO>(gamesFunction, blockParameter);
        }

        public Task<List<string>> GetAlivePlayersQueryAsync(GetAlivePlayersFunction getAlivePlayersFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAlivePlayersFunction, List<string>>(getAlivePlayersFunction, blockParameter);
        }

        
        public Task<List<string>> GetAlivePlayersQueryAsync(BigInteger gameIndex, BlockParameter blockParameter = null)
        {
            var getAlivePlayersFunction = new GetAlivePlayersFunction();
                getAlivePlayersFunction.GameIndex = gameIndex;
            
            return ContractHandler.QueryAsync<GetAlivePlayersFunction, List<string>>(getAlivePlayersFunction, blockParameter);
        }

        public Task<List<string>> GetPlayersQueryAsync(GetPlayersFunction getPlayersFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetPlayersFunction, List<string>>(getPlayersFunction, blockParameter);
        }

        
        public Task<List<string>> GetPlayersQueryAsync(BigInteger gameIndex, BlockParameter blockParameter = null)
        {
            var getPlayersFunction = new GetPlayersFunction();
                getPlayersFunction.GameIndex = gameIndex;
            
            return ContractHandler.QueryAsync<GetPlayersFunction, List<string>>(getPlayersFunction, blockParameter);
        }

        public Task<BigInteger> LastGameQueryAsync(LastGameFunction lastGameFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<LastGameFunction, BigInteger>(lastGameFunction, blockParameter);
        }

        
        public Task<BigInteger> LastGameQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<LastGameFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> NewGameRequestAsync(NewGameFunction newGameFunction)
        {
             return ContractHandler.SendRequestAsync(newGameFunction);
        }

        public Task<TransactionReceipt> NewGameRequestAndWaitForReceiptAsync(NewGameFunction newGameFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(newGameFunction, cancellationToken);
        }

        public Task<string> NewGameRequestAsync(List<string> players, string killer)
        {
            var newGameFunction = new NewGameFunction();
                newGameFunction.Players = players;
                newGameFunction.Killer = killer;
            
             return ContractHandler.SendRequestAsync(newGameFunction);
        }

        public Task<TransactionReceipt> NewGameRequestAndWaitForReceiptAsync(List<string> players, string killer, CancellationTokenSource cancellationToken = null)
        {
            var newGameFunction = new NewGameFunction();
                newGameFunction.Players = players;
                newGameFunction.Killer = killer;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(newGameFunction, cancellationToken);
        }

        public Task<bool> PlayerTurnVoteQueryAsync(PlayerTurnVoteFunction playerTurnVoteFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<PlayerTurnVoteFunction, bool>(playerTurnVoteFunction, blockParameter);
        }

        
        public Task<bool> PlayerTurnVoteQueryAsync(BigInteger gameIndex, BigInteger turn, string player, BlockParameter blockParameter = null)
        {
            var playerTurnVoteFunction = new PlayerTurnVoteFunction();
                playerTurnVoteFunction.GameIndex = gameIndex;
                playerTurnVoteFunction.Turn = turn;
                playerTurnVoteFunction.Player = player;
            
            return ContractHandler.QueryAsync<PlayerTurnVoteFunction, bool>(playerTurnVoteFunction, blockParameter);
        }

        public Task<string> VoteRequestAsync(VoteFunction voteFunction)
        {
             return ContractHandler.SendRequestAsync(voteFunction);
        }

        public Task<TransactionReceipt> VoteRequestAndWaitForReceiptAsync(VoteFunction voteFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(voteFunction, cancellationToken);
        }

        public Task<string> VoteRequestAsync(BigInteger gameIndex, string target)
        {
            var voteFunction = new VoteFunction();
                voteFunction.GameIndex = gameIndex;
                voteFunction.Target = target;
            
             return ContractHandler.SendRequestAsync(voteFunction);
        }

        public Task<TransactionReceipt> VoteRequestAndWaitForReceiptAsync(BigInteger gameIndex, string target, CancellationTokenSource cancellationToken = null)
        {
            var voteFunction = new VoteFunction();
                voteFunction.GameIndex = gameIndex;
                voteFunction.Target = target;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(voteFunction, cancellationToken);
        }
    }
}
