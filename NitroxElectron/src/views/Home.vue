<template>
<div class="home">

    <launch-game-modal v-show="isLaunchGameModalVisible" @close="closeLaunchGameModal" />

    <main role="main" class="col-md-9 col-lg-9 ml-auto pl-4 ">
        <div class="home-image d-flex">
            <div class="subnautica-logo"></div>
        </div>
        <div class="row mt-4">
            <div class="col-md-8">
                <p class="size-14">Nitrox is an open-source, multiplayer modification for the game Subnautica.
                    The project is maintained by the community with regular support and updates from the generous contributors.</p>
                <div class="row">
                    <div class="col-md-4">
                        <h6 class="pt-0 mb-1 text-uppercase size-11">Game developer</h6>
                        <p class="size-14">Unknownworlds</p>
                    </div>
                    <div class="col-md-4">
                        <h6 class="pt-0 mb-1 text-uppercase size-11">Mod developer</h6>
                        <p class="size-14">Nitrox Team</p>
                    </div>
                </div>

            </div>
            <div class="col-md-4 start-game-buttons pl-0">
                <button class="btn btn-primary btn-lg text-uppercase btn-block button-fancy" id="button-fancy" @click="showLaunchGameModal" v-on:mousemove="updateCoordinates">
                    <span>
                        Play
                        <h6 class="pt-0 mb-1 text-uppercase">multiplayer</h6>
                    </span>
                </button>
                <a class="btn btn-secondary btn-lg text-uppercase btn-block font-14" href="#" data-toggle="tooltip" data-placement="top" title="Play Subnautica singleplayer">Singleplayer</a>
            </div>
        </div>

        <div class="row">
            <div class="col-12 mb-4 pb-1 mt-3">
                <div class="line"></div>
            </div>

            <div class="col-md-3">
                <p>Follow Nitrox</p>
            </div>
            <div class="col-md-9">
                <div class="bg-on-dark-variant social rounded-lg d-flex justify-content-center py-5 disable-select">
                    <ul class="nav">
                        <li class="nav-item">
                            <a class="nav-link" href="#" target="_blank" @click.prevent="openExternalBrowser('https://discord.gg/E8B4X9s')" draggable="false" data-tooltip="Join the Discord server">
                                <img src="../assets/img/svg/discord.svg" alt="Discord Logo">
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" target="_blank" @click.prevent="openExternalBrowser('https://twitter.com/modnitrox')" draggable="false" data-tooltip="Follow on Twitter">
                                <img src="../assets/img/svg/twitter.svg" alt="Twitter">
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" target="_blank" @click.prevent="openExternalBrowser('https://www.reddit.com/r/SubnauticaNitrox/')" draggable="false" data-tooltip="Join the Subreddit">
                                <img src="../assets/img/svg/reddit.svg" alt="Reddit">
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" target="_blank" @click.prevent="openExternalBrowser('https://github.com/SubnauticaNitrox/Nitrox')" draggable="false" data-tooltip="Contribute on GitHub">
                                <img src="../assets/img/svg/github.svg" alt="GitHub" width="24px">
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" href="#" target="_blank" @click.prevent="openExternalBrowser('https://nitrox.rux.gg')" draggable="false" data-tooltip="Visit the official website">
                                <img src="../assets/img/svg/website.svg" alt="Website" width="26px">
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-12 mb-4 mt-1"></div>

            <div class="col-md-3">
                <p>About Nitrox</p>
            </div>
            <div class="col-md-3">
                <h6 class="pt-0 mb-1 text-uppercase size-11">Mod version</h6>
                <p class="size-14">Alpha 1.3.0.0</p>
            </div>
            <div class="col-md-3">
                <h6 class="pt-0 mb-1 text-uppercase size-11">Release</h6>
                <p class="size-14">4th April 2020</p>
            </div>
            <div class="col-md-3">
                <h6 class="pt-0 mb-1 text-uppercase size-11">Size</h6>
                <p class="size-14">24.71 MB</p>
            </div>
        </div>

        <div class="row mb-2">
            <div class="col-12 mb-4"></div>

            <div class="col-md-3">
                <p>Contributors</p>
            </div>
            <div class="col-md-9">
                <h6 class="pt-0 mb-1 text-uppercase size-11">Developers</h6>
                <p class="size-14">Sunrunner, Killzoms, Jannify, Marjin, Measurity, MadMax, Nes, _HeN_, CatSZekely, Garsia, RabidCrab, AquariusSidhe, Amarok, iCleeem</p>

                <h6 class="pt-0 mb-1 text-uppercase size-11">Staff</h6>
                <p class="size-14">Werewolfs, Shalix, CriticalCookie, InfamousJay, and a big thanks to the discord support team (Artic-Peepers, Peepers)</p>

                <h6 class="pt-0 mb-1 text-uppercase size-11">Design</h6>
                <p class="size-14">Rux</p>
            </div>
        </div>

    </main>
</div>
</template>

<style media="screen">
.subnautica-logo {
    background-image: url('../assets/img/subnauticaLogo.png');
    background-repeat: no-repeat;
    background-size: contain;
    background-position: center;
    width: 352px;
    margin: 0 auto;
}
</style>

<script>
/* eslint no-unused-vars:0 */

const { remote } = require('electron');

import LaunchGameModal from '@/components/LaunchGameModal.vue';

export default {
    name: 'Home',
    components: {
        LaunchGameModal,
    },
    data() {
        return {
            isSettingsModalVisible: false,
            isLaunchGameModalVisible: false,
            x: 0,
            y: 0,
        };
    },
    methods: {
        showLaunchGameModal() {
            this.isLaunchGameModalVisible = true;
            setTimeout(() => {
                this.isLaunchGameModalVisible = false;
            }, 2000)
        },
        closeLaunchGameModal() {
            this.isLaunchGameModalVisible = false;
        },
        openExternalBrowser(link) {
            remote.shell.openExternal(link);
        },
        updateCoordinates(e) {
            let buttonFancy = document.getElementById('button-fancy');
            this.x = e.layerX;
            this.y = e.layerY;
            buttonFancy.style.setProperty('--x', this.x + 'px');
            buttonFancy.style.setProperty('--y', this.y + 'px');
        },
    },
};
</script>
