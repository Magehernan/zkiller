// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

enum GameStatus {
    Playing,
    PeopleWin,
    KillerWin
}

struct GameData {
    uint8 turn;
    address killer;
    address killerTarget;
    address[] players;
    GameStatus status;
}

contract ZKillerGame {
    mapping(uint256 => mapping(uint256 => mapping(address => bool))) gameTurnVoted;
    mapping(uint256 => mapping(uint256 => mapping(address => uint256))) gameTurnVoteCount;

    mapping(uint256 => GameData) public games;
    uint256 public lastGame;

    function getPlayers(uint256 gameIndex) external view returns (address[] memory players) {
        return games[gameIndex].players;
    }

    function playerTurnVote(uint256 gameIndex, uint256 turn, address player) external view returns (bool) {
        return gameTurnVoted[gameIndex][turn][player];
    }

    function newGame(address[] calldata players, address killer) external {
        lastGame++;
        games[lastGame] =
            GameData({turn: 1, players: players, killer: killer, killerTarget: address(0), status: GameStatus.Playing});
    }

    function vote(uint256 gameIndex, address target) external {
        require(msg.sender != target);
        require(isPlayer(gameIndex, msg.sender), "not a active player");
        require(isPlayer(gameIndex, target), "not a player");

        GameData storage gameData = games[gameIndex];

        require(gameData.status == GameStatus.Playing);

        require(!gameTurnVoted[gameIndex][gameData.turn][msg.sender], "already vote");

        gameTurnVoted[gameIndex][gameData.turn][msg.sender] = true;
        gameTurnVoteCount[gameIndex][gameData.turn][target]++;

        if (gameData.killer == msg.sender) {
            gameData.killerTarget = target;
        }

        for (uint256 i = 0; i < gameData.players.length; i++) {
            if (!gameTurnVoted[gameIndex][gameData.turn][gameData.players[i]]) {
                return;
            }
        }

        endTurn(gameIndex, gameData);
    }

    function endTurn(uint256 gameIndex, GameData storage gameData) private {
        uint256 maxIndex = 0;
        uint256 maxVote = 0;
        bool tie = false;

        for (uint256 i = 0; i < gameData.players.length; i++) {
            uint256 votes = gameTurnVoteCount[gameIndex][gameData.turn][gameData.players[i]];
            if (votes >= maxVote) {
                tie = votes == maxVote;
                maxIndex = i;
                maxVote = votes;
            }
        }

        address playerSelected = gameData.players[maxIndex];
        if (!tie && playerSelected == gameData.killer) {
            gameData.status = GameStatus.PeopleWin;
            return;
        }

        for (uint256 i = 0; i < gameData.players.length; i++) {
            if (gameData.players[i] == gameData.killerTarget) {
                gameData.players[i] = gameData.players[gameData.players.length - 1];
                gameData.players.pop();
                break;
            }
        }

        if (gameData.players.length == 2) {
            gameData.status = GameStatus.KillerWin;
            return;
        }

        gameData.turn++;
    }

    function isPlayer(uint256 gameIndex, address player) private view returns (bool) {
        GameData memory gameData = games[gameIndex];
        for (uint256 i = 0; i < gameData.players.length; i++) {
            if (gameData.players[i] == player) {
                return true;
            }
        }

        return false;
    }
}
