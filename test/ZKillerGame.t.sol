// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import {Test, console} from "forge-std/Test.sol";
import "../src/ZKillerGame.sol";

contract ZKillerGameTest is Test {
    ZKillerGame public zkgame;
    address internal killer = vm.addr(1);
    address internal alice = vm.addr(2);
    address internal bob = vm.addr(3);
    address internal notAPlayer = vm.addr(4);

    function setUp() public {
        zkgame = new ZKillerGame();
    }

    function testFuzz_NewGame(address[] memory players) public {
        vm.assume(players.length > 0 && players.length <= 10);
        zkgame.newGame(players, players[0]);
        assertEq(zkgame.lastGame(), 1);
        zkgame.newGame(players, players[0]);
        assertEq(zkgame.lastGame(), 2);
    }

    function test_PeopleWin() public {
        address[] memory players = new address[](3);
        players[0] = killer;
        players[1] = alice;
        players[2] = bob;
        zkgame.newGame(players, killer);
        vm.startPrank(killer);
        zkgame.vote(1, bob);
        vm.startPrank(alice);
        zkgame.vote(1, killer);
        vm.startPrank(bob);
        zkgame.vote(1, killer);
        (,,, GameStatus status) = zkgame.games(1);
        assertEq(uint256(status), uint256(GameStatus.PeopleWin));
    }

    function test_KillerWin() public {
        address[] memory players = new address[](3);
        players[0] = killer;
        players[1] = alice;
        players[2] = bob;
        zkgame.newGame(players, killer);
        vm.startPrank(killer);
        zkgame.vote(1, bob);
        vm.startPrank(alice);
        zkgame.vote(1, bob);
        vm.startPrank(bob);
        zkgame.vote(1, alice);
        assertEq(2, zkgame.getAlivePlayers(1).length);
        (uint8 turn,,, GameStatus status) = zkgame.games(1);
        assertEq(turn, 1);
        assertEq(uint256(status), uint256(GameStatus.KillerWin));
    }

    function test_Revert() public {
        address[] memory players = new address[](3);
        players[0] = killer;
        players[1] = alice;
        players[2] = bob;
        zkgame.newGame(players, killer);

        vm.startPrank(notAPlayer);
        vm.expectRevert();
        zkgame.vote(1, bob);

        vm.startPrank(killer);
        zkgame.vote(1, bob);
        vm.expectRevert();
        zkgame.vote(1, bob);

        vm.startPrank(alice);
        zkgame.vote(1, bob);
        vm.expectRevert();
        zkgame.vote(1, address(88));

        vm.startPrank(bob);
        zkgame.vote(1, alice);
        vm.expectRevert();
        zkgame.vote(1, alice);
    }
}
