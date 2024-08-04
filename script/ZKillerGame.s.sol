// SPDX-License-Identifier: UNLICENSED
pragma solidity ^0.8.13;

import {Script, console} from "forge-std/Script.sol";
import {ZKillerGame} from "../src/ZKillerGame.sol";

contract CounterScript is Script {
    ZKillerGame public counter;

    function setUp() public {}

    function run() public {
        vm.startBroadcast();

        counter = new ZKillerGame();

        vm.stopBroadcast();
    }
}