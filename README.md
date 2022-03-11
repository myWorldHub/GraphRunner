# GraphRunner

nananapo/GraphConnectEngine
を使って作成したアプリケーション

### 機能

#### グラフ作成、実行をインタラクティブに行える

```
$ graphrunner i
> help # コマンドリストを表示する
> creategraph # グラフを作る
> remove # グラフを削除する
> connect # ノードを繋ぐ
> quit # 終了
```

他のコマンドについては、helpで確認してください

#### グラフとノード接続の定義が書かれたjsonを読み込んで実行する

定義例

https://github.com/nananapo/GraphRunner/blob/main/test.json

```
graphrunner i fileName
```

#### アクセスされたらグラフ実行、結果を出力するhttpサーバーを建てられる

```
> server start ポート番号 # サーバー起動
> server bind パス グラフID # パスにアクセスされたとき、グラフを実行して結果を返す
```
