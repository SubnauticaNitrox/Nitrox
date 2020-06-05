<script>
export default {
  name: 'LaunchGameModal',
  methods: {
    close() {
      this.$emit('close');
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
<transition name="modal-game">

  <div class="game-modal-backdrop align-items-start flex-column ">
    <div class="modal align-self-center shadow-lg" role="dialog" aria-labelledby="modalTitle" aria-describedby="modalDescription">

      <section class="modal-body" id="modalDescription">
        <div class="row m-0 h-100">
          <div class="col-md-12 p-0 d-flex">
            <!-- Modal content -->
            <div class="m-auto">
              <svg class="spinner" width="65px" height="65px" viewBox="0 0 66 66" xmlns="http://www.w3.org/2000/svg">
                <circle class="path" fill="none" stroke-width="6" stroke-linecap="round" cx="33" cy="33" r="30"></circle>
              </svg>
            </div>
          </div>
        </div>

      </section>
    </div>
  </div>
</transition>
</template>
<style>
.spinner {
  -webkit-animation: rotator 1.4s linear infinite;
  animation: rotator 1.4s linear infinite;
}

@-webkit-keyframes rotator {
  0% {
    -webkit-transform: rotate(0deg);
    transform: rotate(0deg);
  }

  100% {
    -webkit-transform: rotate(270deg);
    transform: rotate(270deg);
  }
}


.path {
  stroke: #FFFFFF;
  stroke-dasharray: 180;
  stroke-dashoffset: 0;
  -webkit-transform-origin: center;
  transform-origin: center;
  -webkit-animation: dash 1.4s ease-in-out infinite, colors 5.6s ease-in-out infinite;
  animation: dash 1.4s ease-in-out infinite, colors 5.6s ease-in-out infinite;
}

@-webkit-keyframes dash {
  0% {
    stroke-dashoffset: 187;
  }

  50% {
    stroke-dashoffset: 50;
    -webkit-transform: rotate(135deg);
    transform: rotate(135deg);
  }

  100% {
    stroke-dashoffset: 187;
    -webkit-transform: rotate(450deg);
    transform: rotate(450deg);
  }
}

.modal-game-enter {
  opacity: 0;
  transform: scale(1.1);
}

.modal-game-leave-active {
  opacity: 0;
}

.modal-game-leave-active.game-modal-backdrop .modal {
  transform: scale(0.9);
  transition: all .25s ease-in-out;
}

.modal-game-enter-active,
.modal-game-leave-active {
  transition: all .25s ease-in-out;
}

.game-modal-backdrop {
  position: fixed;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: rgba(26, 26, 29, 0.75);
  /* backdrop-filter: blur(20px); */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 100;
}

.game-modal-backdrop .modal {
  background-position: center;
  background-size: cover;
  overflow-x: auto;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  height: 242px;
  width: 342px;
  position: relative;
  border-radius: 8px
}
</style>
