'use strict'

import { app, protocol, BrowserWindow } from 'electron'
import {
  createProtocol,
  /* installVueDevtools */
} from 'vue-cli-plugin-electron-builder/lib'
const isDevelopment = process.env.NODE_ENV !== 'production'

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let win
let loadingWindow

// Scheme must be registered before the app is ready
protocol.registerSchemesAsPrivileged([{scheme: 'app', privileges: { secure: true, standard: true } }])

function createLoadingWindow() {
  loadingWindow = new BrowserWindow({
    width: 332,
    height: 342,
    frame: false,
    backgroundColor: '#28292C',
  })

  loadingWindow.setResizable(false);

  if (process.env.WEBPACK_DEV_SERVER_URL) {
    // Load the url of the dev server if in development mode
    loadingWindow.loadURL(process.env.WEBPACK_DEV_SERVER_URL + "/loading.html")
  } else {
    createProtocol('app')
    // Load the index.html when not in development
    loadingWindow.loadURL('app://./loading.html')
  }

  loadingWindow.on('closed', () => loadingWindow = null);
  loadingWindow.webContents.on('did-finish-load', () => {
    loadingWindow.show();
  });
}


function createWindow () {
  // Create the browser window.
  win = new BrowserWindow({
    width: 1024,
    height: 612,
    minWidth: 1024,
    minHeight: 612,
    frame: false,
    titleBarStyle: 'hidden',
    backgroundColor: '#28292C',
    webPreferences: {
      nodeIntegration: true
    },
    show: false
  })

  if (process.env.WEBPACK_DEV_SERVER_URL) {
    // Load the url of the dev server if in development mode
    win.loadURL(process.env.WEBPACK_DEV_SERVER_URL)
    if (!process.env.IS_TEST) win.webContents.openDevTools()
  } else {
    createProtocol('app')
    // Load the index.html when not in development
    win.loadURL('app://./index.html')
  }

  win.on('closed', () => {
    win = null
  })

  win.webContents.on('did-finish-load', () => {
  /// when the content has loaded, hide the loading screen and show the main window
  if (loadingWindow) {
    loadingWindow.close();
  }
  win.show();
});
}

// Quit when all windows are closed.
app.on('window-all-closed', () => {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

app.on('activate', () => {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (win === null) {
    createWindow()
  }
})

app.on('window-all-closed', () => {
  win = null
});

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', () => {
  if (isDevelopment && !process.env.IS_TEST) {
    // Install Vue Devtools
    // Devtools extensions are broken in Electron 6.0.0 and greater
    // See https://github.com/nklayman/vue-cli-plugin-electron-builder/issues/378 for more info
    // Electron will not launch with Devtools extensions installed on Windows 10 with dark mode
    // If you are not using Windows 10 dark mode, you may uncomment these lines
    // In addition, if the linked issue is closed, you can upgrade electron and uncomment these lines
    // try {
    //   await installVueDevtools()
    // } catch (e) {
    //   console.error('Vue Devtools failed to install:', e.toString())
    // }

  }
  // createWindow()

  createLoadingWindow()

   setTimeout(() => {
     createWindow()
   }, 1000)

})

// Exit cleanly on request from parent process in development mode.
if (isDevelopment) {
  if (process.platform === 'win32') {
    process.on('message', data => {
      if (data === 'graceful-exit') {
        app.quit()
      }
    })
  } else {
    process.on('SIGTERM', () => {
      app.quit()
    })
  }
}
