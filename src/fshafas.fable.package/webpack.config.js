var path = require("path");
var nodeExternals = require('webpack-node-externals');

var babelOptions = {
  presets: [
    ["@babel/preset-env", {
      "targets": {
        "node": true,
      },
    }]
  ],
};

console.log("Bundling function...");

module.exports = {
  target: "node",
  externals: [nodeExternals()],
  node: {
    __dirname: false,
    __filename: false,
  },
  entry: './fshafas.fable.fsproj',
  output: {
    path: path.join(__dirname, "./fs-hafas-client"),
    filename: 'fshafas.bundle.js',
    library:"fshafas",
    libraryTarget: 'commonjs'
  },
  plugins: [ ],
  module: {
    rules: [{
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader",
          options: {
            typedArrays: false,
            babel: babelOptions,
            define: ["WEBPACK"]
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: babelOptions
        },
      }
    ]
  },
};