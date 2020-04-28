﻿const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const Path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    mode: 'development',
    entry: {
        site: './css/site.css',
    },
    output: {
        filename: '[name].js',
        path: Path.resolve(__dirname, 'wwwroot'),
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            importLoaders: 1
                        }
                    },
                    {
                        loader: 'postcss-loader',
                        options: {
                            plugins: (loader) => [
                                require('tailwindcss'),
                                require('autoprefixer')({}),
                                require('postcss-import')({ root: loader.resourcePath }),
                                require('postcss-preset-env')(),
                            ],
                            minimize: false
                        },
                    }
                ],
            },
        ],
    },
    plugins: [
        new CleanWebpackPlugin({
            options: {
                exlude: ['images']
            }
        }),
        new MiniCssExtractPlugin({
            filename: './css/[name].css',
        }),
    ],
};