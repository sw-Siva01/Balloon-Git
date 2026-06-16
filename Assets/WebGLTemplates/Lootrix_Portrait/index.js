// =====================
// Date/Time for cache busting
// =====================
//replacetitleinfo//
var currentDate = new Date();
var year = currentDate.getFullYear();
var month = currentDate.getMonth() + 1;
var day = currentDate.getDate();
var hours = currentDate.getHours();
var minutes = currentDate.getMinutes();
var seconds = currentDate.getSeconds();
var currentDateTime = year + "-" + month + "-" + day + " " + hours + ":" + minutes + ":" + seconds;

var applicationInstance;

// =====================
// Service Worker Registration
// =====================
window.addEventListener("load", function () {
  if ("serviceWorker" in navigator) {
    navigator.serviceWorker.register("ServiceWorker.js?timefornocatch=" + currentDateTime);
  }
});

// =====================
// DOM References
// =====================
var container = document.querySelector("#unity-container");
var canvas = document.querySelector("#unity-canvas");
var loadingBar = document.querySelector("#unity-loading-bar");
var loadingtext = document.querySelector(".loading-text");
var progressBarFull = document.querySelector("#unity-progress-bar-full");
var warningBanner = document.querySelector("#unity-warning");

// =====================
// Block Unity from injecting inline style on canvas
// All cursor/style control handled via CSS classes only
// =====================
(function blockInlineStyleOnCanvas() {
  if (!canvas) return;

  var styleDescriptor = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'style');
  var originalStyleGetter = styleDescriptor.get;

  Object.defineProperty(canvas, 'style', {
    get: function () {
      var style = originalStyleGetter.call(this);
      return new Proxy(style, {
        set: function (target, prop, value) {
          // Block inline style injection for cursor — CSS class handles it
          if (prop === 'cursor') return true;
          target[prop] = value;
          return true;
        }
      });
    },
    configurable: true
  });
})();

// =====================
// Warning Banner
// =====================
function unityShowBanner(msg, type) {
  function updateBannerVisibility() {
    if (warningBanner.children.length) {
      warningBanner.classList.add("has-children");
    } else {
      warningBanner.classList.remove("has-children");
    }
  }

  var div = document.createElement('div');
  div.innerHTML = msg;

  if (type === 'error') {
    div.classList.add('unity-warning-error');
  } else if (type === 'warning') {
    div.classList.add('unity-warning-warning');
    setTimeout(function () {
      warningBanner.removeChild(div);
      updateBannerVisibility();
    }, 5000);
  }

  warningBanner.appendChild(div);
  updateBannerVisibility();
}

// =====================
// Unity Build Config
// =====================

      var buildUrl = "Build";
      var loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}?timefornocatch=" + currentDateTime;
      var config = {
        dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}?timefornocatch=" + currentDateTime,
        frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}?timefornocatch=" + currentDateTime,
#if USE_THREADS
        workerUrl: buildUrl + "/{{{ WORKER_FILENAME }}}?timefornocatch=" + currentDateTime,
#endif
#if USE_WASM
        codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}?timefornocatch=" + currentDateTime,
#endif
#if MEMORY_FILENAME
        memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}?timefornocatch=" + currentDateTime,
#endif
#if SYMBOLS_FILENAME
        symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}?timefornocatch=" + currentDateTime,
#endif
        streamingAssetsUrl: "StreamingAssets",
        companyName: {{{ JSON.stringify(COMPANY_NAME) }}},
        productName: {{{ JSON.stringify(PRODUCT_NAME) }}},
        productVersion: {{{ JSON.stringify(PRODUCT_VERSION) }}},
        showBanner: unityShowBanner,
      };

      if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
        var meta = document.createElement('meta');
        meta.name = 'viewport';
        meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
        document.getElementsByTagName('head')[0].appendChild(meta);
      }

#if BACKGROUND_FILENAME
      canvas.style.background = "url('" + buildUrl + "/{{{ BACKGROUND_FILENAME.replace(/'/g, '%27') }}}') center / cover";
#endif

// =====================
// Unity Loader
// =====================
loadingBar.classList.add("visible");
loadingtext.classList.add("visible");


var script = document.createElement("script");
script.src = loaderUrl;
console.log("Loading:", loaderUrl);
script.onload = function () {
    if (typeof SetTitleInfo === 'function')
    {
      console.log("title info " + titleInfo);
        SetTitleInfo(titleInfo);  // <-- your parameter here
    }
    else
    {
        console.error("StartGame is not defined.");
    }

    console.log("Canvas:", canvas);
    console.log("Config:", config);
    console.log("createUnityInstance:", typeof createUnityInstance);

    try {


        var unityPromise = createUnityInstance(
            canvas,
            config,
            function (progress) {


                progressBarFull.style.width =
                    (100 * progress) + "%";

                if (typeof window.onUnityProgress === 'function') {
                    window.onUnityProgress(progress);
                }
            }
        );


        unityPromise.then(function (unityInstance) {


            applicationInstance = unityInstance;

            loadingBar.classList.remove("visible");
            loadingtext.classList.remove("visible");
            loadingtext.style.display = "none";

            canvas.classList.add("unity-ready");


        }).catch(function (error) {

            console.error(
                "STEP 7 - Unity Promise Failed"
            );

            console.error(error);

        });

    } catch (error) {

        console.error(
            "STEP 8 - Exception During createUnityInstance"
        );

        console.error(error);

    }
};

script.onerror = function (error) {

    console.error(
        "STEP 9 - Loader Script Failed"
    );

    console.error(error);

};
console.log("APPENDING SCRIPT");
  setTimeout(function () {

document.body.appendChild(script);
console.log("SCRIPT APPENDED");

    }, 1000);

// =====================
// Loading Progress Text
// =====================
var $loadingText = $('.loading-text .Gathering');

function onUnityProgress(progress) {
  var loadProgress = Math.round(progress * 100);

  if (loadProgress <= 35) {
    $loadingText.text("Getting Ready");
  } else if (loadProgress <= 80) {
    $loadingText.text("Gathering Resources");
  } else if (loadProgress <= 100) {
    $loadingText.text("Powering Up");
  }

  var currentTime = new Date();
  var h = currentTime.getHours().toString().padStart(2, '0');
  var m = currentTime.getMinutes().toString().padStart(2, '0');
  var s = currentTime.getSeconds().toString().padStart(2, '0');
  var formattedTime = h + ":" + m + ":" + s;

  console.log("[" + formattedTime + "] Progress: " + loadProgress + "%, Text: " + $loadingText.text());
}

window.onUnityProgress = onUnityProgress;

// =====================
// Login / Parent Frame
// =====================


function getParentUrl() {
  var isInIframe = (parent !== window);
  var parentUrl = null;
  if (isInIframe) {
    parentUrl = document.referrer;
  }
  return parentUrl;
}

