const cacheName = "WakerIndustries-Macarenight-1.0.0";
const contentToCache = [
    "Build/docs.loader.js",
    "Build/docs.framework.js.br",
    "Build/docs.data.br",
    "Build/docs.wasm.br",
    "TemplateData/style.css"

];

self.addEventListener('install', function (e) {
    console.log('[Service Worker] Install');
    
    e.waitUntil((async function () {
      const cache = await caches.open(cacheName);
      console.log('[Service Worker] Caching all: app shell and content');
      await cache.addAll(contentToCache);
    })());
});

self.addEventListener('fetch', function (e) {
    e.respondWith((async function () {
      const request = e.request;
      
      // Check cache first
      let response = await caches.match(request);
      console.log(`[Service Worker] Fetching resource: ${request.url}`);
      
      if (response) { 
        // For .br files from cache, ensure correct headers
        if (request.url.endsWith('.br')) {
          const headers = new Headers(response.headers);
          headers.set('Content-Encoding', 'br');
          headers.set('Content-Type', 'application/octet-stream');
          return new Response(response.body, {
            status: response.status,
            statusText: response.statusText,
            headers: headers
          });
        }
        return response; 
      }

      // Fetch from network
      response = await fetch(request);
      
      // For .br files, modify headers before caching
      if (request.url.endsWith('.br')) {
        const headers = new Headers(response.headers);
        headers.set('Content-Encoding', 'br');
        headers.set('Content-Type', 'application/octet-stream');
        
        const modifiedResponse = new Response(response.body, {
          status: response.status,
          statusText: response.statusText,
          headers: headers
        });
        
        const cache = await caches.open(cacheName);
        console.log(`[Service Worker] Caching new resource: ${request.url}`);
        cache.put(request, modifiedResponse.clone());
        return modifiedResponse;
      }
      
      // Cache and return other files normally
      const cache = await caches.open(cacheName);
      console.log(`[Service Worker] Caching new resource: ${request.url}`);
      cache.put(request, response.clone());
      return response;
    })());
});
