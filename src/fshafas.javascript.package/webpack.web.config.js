var path = require("path");
const webpack = require('webpack');

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
  experiments: {
    outputModule: true
  },
  target: "web",
  entry: './build/Lib.js',
  output: {
    path: path.join(__dirname, "./fs-hafas-client-web"),
    filename: 'fshafas.web.bundle.js',
    library: {
      type: 'module'
    }
  },
  plugins: [new webpack.ProvidePlugin({
    Buffer: ['buffer', 'Buffer'],
  })],
  module: {
    rules: [
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