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
  target: "web",
  entry: './build/Lib.js',
  output: {
    path: path.join(__dirname, "../fshafas.fable.web/wwwroot/js"),
    filename: 'fshafas.bundle.js',
    library: {
      name: 'fshafas',
      type: 'umd',
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