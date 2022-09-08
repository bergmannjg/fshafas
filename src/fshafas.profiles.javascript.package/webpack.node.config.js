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
  entry: './build/Profiles.js',
  output: {
    path: path.join(__dirname, "./fs-hafas-profiles"),
    filename: 'profiles.bundle.js',
    library:"profiles",
    libraryTarget: 'commonjs'
  },
  plugins: [ ],
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