#if USE_DATA_CACHING
const cacheName = {{{JSON.stringify(COMPANY_NAME + "-" + PRODUCT_NAME + "-" + PRODUCT_VERSION )}}};
const contentToCache = [
    "Build/{{{ LOADER_FILENAME }}}",
    "Build/{{{ FRAMEWORK_FILENAME }}}",
#if USE_THREADS
    "Build/{{{ WORKER_FILENAME }}}",
#endif
    "Build/{{{ DATA_FILENAME }}}",
    "Build/{{{ CODE_FILENAME }}}",
    "TemplateData/style.css"

];
#endif

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
#if USE_DATA_CACHING
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
#endif
});

#if USE_DATA_CACHING
self.addEventListener('fetch', function (e) {

    if (e.request.method !== 'GET') {
        e.respondWith(fetch(e.request));
        return;
    }

    e.respondWith((async function () {

        const cached = await caches.match(e.request);

        if (cached) {
            return cached;
        }

        const response = await fetch(e.request);

        if (response.ok) {
            const cache = await caches.open(cacheName);
            await cache.put(e.request, response.clone());
        }

        return response;

    })());
});
#endif