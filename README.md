# GraphRunner

### 機能

#### グラフ作成、実行をインタラクティブに行える

```
$ graphrunner i
> help # コマンドリストを表示する
```

できることについては、helpで確認してください

#### グラフとノード接続の定義が書かれたjsonを読み込んで実行する

定義例

https://github.com/nananapo/GraphRunner/blob/main/test.json

```
graphrunner i fileName
```

* アクセスされたらグラフ実行、結果を出力するhttpサーバーを建てられる

```
起動したら、serverコマンドを使う
```
