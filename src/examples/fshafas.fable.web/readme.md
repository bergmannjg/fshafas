# PWA Example

* a progressive web app using the fshafas package
* a web server that acts as generic proxy for the hafas endpoints and serves static pages.

## Build and run

* *npx webpack --config webpack.config.js* in directrory ./lib
* *cp lib/dist/bundle.js wwwroot/js/lib/*
* *tsc --target ES2015 site.ts* in directrory ./wwwroot/js
* *dotnet run*
