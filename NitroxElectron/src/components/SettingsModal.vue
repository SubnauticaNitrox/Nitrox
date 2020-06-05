<script>
export default {
  name: 'SettingsModal',

  data () {
    return {
      settingsActiveTab: "general"
    }
  },

  methods: {
    close() {
      this.$emit('close');
    },

    settingsSwitchTab(tab) {
      this.settingsActiveTab = tab;
    },
  },

  mounted() {
    const close = (e) => {
      const ESC = 27;
      if (e.keyCode !== ESC) return;
      this.$emit('close');
    };
    // Close the modal when the
    // user presses the ESC key.
    document.addEventListener('keyup', close);
    this.$on('hook:destroyed', () => {
      document.removeEventListener('keyup', close);
    });
  },
};
</script>
<template>
  <transition name="modal-fade">
    <div class="modal-backdrop">
      <div class="modal" role="dialog" aria-labelledby="modalTitle" aria-describedby="modalDescription">

        <!-- Modal navigation -->
        <nav class="col-md-3 d-md-block sidebar modal-menu disable-select">
          <div class="sidebar-sticky d-flex align-items-start flex-column pt-0">
            <!-- Sidebar -->
            <ul class="nav w-100 flex-column mb-auto">
              <li class="nav-item mt-5 mb-1 pl-3">
                <h6>Nitrox settings</h6>
              </li>
              <li class="nav-item mb-1">
                <a :class="{ 'nav-link': true, active: (this.settingsActiveTab == 'general') }" @click.prevent="settingsSwitchTab('general')">
                  <span class="h-small">General</span>
                </a>
              </li>
              <li class="nav-item mb-1" @click.prevent="settingsSwitchTab('server')">
                <a :class="{ 'nav-link': true, active: (this.settingsActiveTab == 'server') }" @click.prevent="settingsSwitchTab('server')">
                  <span class="h-small">Server</span>
                </a>
              </li>
              <li class="nav-item mb-1" @click.prevent="settingsSwitchTab('changelog')">
                <a :class="{ 'nav-link': true, active: (this.settingsActiveTab == 'changelog') }" @click.prevent="settingsSwitchTab('changelog')">
                  <span class="h-small">Changelog</span>
                </a>
              </li>
            </ul>

            <!-- Version -->
            <ul class="nav w-100">
              <div class="d-flex w-100">
                <div class="px-3 pb-3 pt-3 mr-1 h-small">
                  <h6 class="mb-1 font-11 opacity-5">Version</h6>
                  <p class="mb-0 font-14 opacity-75" id="version-number">1.0.0</p>
                </div>
              </div>
            </ul>
          </div>
        </nav>

      <!-- Modal content -->
        <section class="modal-body pt-0" id="modalDescription">
          <div class="row m-0 p-0">

            <!-- Body -->
            <div class="col-md-8 col-lg-8 ml-auto px-4">
              <slot name="body">
                <template v-if="this.settingsActiveTab == 'general'">
                  <div class="settings-general pt-5 pb-5">
                      <div class="row">
                         <div class="col-md-12">
                             <h6>Subnautica Installation</h6>
                             <div class="bg-on-dark-variant p-4 mt-3 rounded-lg">
                               <div class="media">
                                 <img src="../assets/img/subnautica-icon.jpg" class="mr-3 img-fluid rounded disable-select" alt="Subnautica Icon" width="64px">
                                 <div class="media-body mt-1">
                                   <h5 class="mt-0 mb-1 font-400">Subnautica</h5>
                                   <p class="font-14 opacity-75 m-0">D:\Games\Epic Games\Subnautica</p>
                                 </div>
                               </div>
                             </div>

                             <p class="mt-4">Incorrect installation path?</p>
                             <button type="button" name="button" class="btn btn-primary font-16 btn-lg px-5">Browse</button>
                         </div>
                      </div>
                  </div>
                </template>

                <template v-if="this.settingsActiveTab == 'server'">
                    <div class="settings-server pt-5 pb-5">
                        <!-- Server Port -->
                        <div class="row">
                            <div class="col-md-6">
                                <h6 class="disable-select pb-1">Server Port</h6>
                                <span style="position: absolute;right: 16px;padding: 16px;margin-bottom: 0;z-index: 12;" data-tooltip="Port protocol type">
                                    <h6 class="font-mono">UDP</h6>
                                </span>
                                <input type="number" class="form-control pr-5" id="form-server-port" placeholder="Server port" value="11000">
                            </div>
                            <div class="col-md-12 mt-3">
                                <p class="opacity-75 font-14">The Nitrox server needs an open port to communicate through. This can be achieved by port forwarding or through the use of a VPN service like Hamachi if you do not know or want to port forward.</p>
                            </div>
                        </div>
                        <!-- Server Players and Gamemode -->
                        <div class="row mt-3">
                            <div class="col-md-6">
                                <h6 class="disable-select pb-1">Max. Players</h6>
                                <input type="number" min="2" max="100" class="form-control" id="form-server-slots" placeholder="2-100" value="100">
                            </div>
                            <div class="col-md-6">
                                <h6 class="disable-select pb-1">Game mode</h6>
                                <select class="form-control" id="form-server-game-mode">
                                    <option>Survival</option>
                                    <option>Creative</option>
                                </select>
                            </div>
                        </div>

                        <!-- Admin -->
                        <div class="row mt-3">
                            <div class="col-12 mb-4 pb-1 mt-4"><div class="line"></div></div>
                            <div class="col-md-12">
                                <h6 class="disable-select pb-1">Admin Password</h6>
                                <input class="form-control" id="form-server-admin-password" placeholder="Admin password" value="NONBPFWPFPHL">
                            </div>
                        </div>

                        <!-- Server Window -->
                        <div class="row mt-3">
                            <div class="col-12 mb-4 pb-1 mt-4"><div class="line"></div></div>
                            <div class="col-md-12">
                                <h6 class="disable-select pb-1">Server window</h6>
                                <p class="opacity-75 font-14">Choose whether you'd like to keep the server console docked in the launcher, or open in an external window.</p>

                                <div class="settings-control">
                                    <div class="custom-control custom-checkbox mb-2">
                                      <input type="radio" id="server-type-docked" name="server-type" class="custom-control-input" checked>
                                      <label class="custom-control-label" for="server-type-docked">Docked console</label>
                                    </div>
                                    <div class="custom-control custom-checkbox mb-2">
                                      <input type="radio" id="server-type-window" name="server-type" class="custom-control-input">
                                      <label class="custom-control-label" for="server-type-window">External console window</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                  </div>
                </template>

                <template v-if="this.settingsActiveTab == 'changelog'">
                    <div class="settings-changelog pt-5 pb-5">
                        <!-- Patch notes -->
                        <div class="row">
                            <div class="col-md-12">
                                <h6>Changelog</h6>
                                <div class="changelog-entry mt-4 mb-5">
                                    <h3>Nitrox Alpha 1.2.0.1</h3>
                                    <p class="font-14 opacity-75">March 8th 2020</p>
                                    <ul>
                                        <li>Fix for a common instance of infinite loading when reconnecting.</li>
                                        <li>Fix for base inventories being wiped when reconnecting.</li>
                                        <li>Fix for vehicles modules being wiped when reconnecting.</li>
                                        <li>Fix for radio messages being lost when reconnecting.</li>
                                        <li>Fix for furniture being unpowered when reconnecting.</li>
                                        <li>Fix for vehicle health being majorly out-of-sync causing seamoths to randomly explode.</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>

              </slot>
            </div>

            <!-- Close button -->
            <div class="col-md-1">
              <button type="button" class="btn sm-btn btn-icon position-fixed mt-4" @click="close" aria-label="Close modal" style="right: 38px;">
                <span class="material-icons">close</span>
              </button>
              <h6 class="esc-text disable-select opacity-5">ESC</h6>
            </div>
          </div>

        </section>
      </div>
    </div>
  </transition>
</template>
<style>
.esc-text {
  position: fixed;
  top: 92px;
  right: 49px;
  letter-spacing: 0.5px;
  font-size: 10px;
}
.modal .sidebar {
  background: #28292C;
}

.modal-fade-enter,
.modal-fade-leave-active {
  opacity: 0;
  transform: scale(1.1);
}

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: all .5s cubic-bezier(1, -0.4, 0, 1.4);
}

.modal-backdrop {
  position: fixed;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: #1F1F22;
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal {
  background: #333437;
  overflow-x: auto;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  margin-top: 24px;
  height: calc(100vh - 24px);
}

.modal::-webkit-scrollbar-track {
  background: none;        /* color of the tracking area */
  border: 4px solid #333437;  /* creates padding around scroll thumb */
}

.modal::-webkit-scrollbar-thumb {
  background-color: rgba(0,0,0,.5);    /* color of the scroll thumb */
  border-radius: 20px;       /* roundness of the scroll thumb */
  border: 4px solid #333437;  /* creates padding around scroll thumb */
}

.modal-header,
.modal-footer {
  padding: 15px;
  display: flex;
}

.modal-header {
  justify-content: space-between;
}

.modal-footer {
  justify-content: flex-end;
}

.modal-body {
  position: relative;
  padding: 20px 10px;
}
</style>
