# Task-Handler測試

### 接口說明
|API|Description|
|--|--|
|/trigger/pubsub|使用者上傳資料後觸發|
|/trigger/crawler|接收Pub/Sub並觸發爬蟲|
|/trigger/ai|接收Pub/Sub並觸發AI|

### acknowledge - http code設定
|code|Description|
|--|--|
|200|task 執行結束（包含成功/失敗）|
|102|處理中|
|500|未處理|

> 其他事項
1. 僅用於重現 task-handler 執行邏輯，<br/>為方便測試，未實作main task table且 sub_task寫入點亦有調整，<br/>此段邏輯請依照原有方式處理
（亦有註解於程式內）
2. 目前強制結束條件為執行時間 > 35 分鐘，請依實際執行狀況做調整
3. 強制結束條件請加入retry機制 （將retry機制從爬蟲端移除）
4. 路徑及命名僅供參考

<hr/>

> 流程圖 
<br/>
<a href="https://drive.google.com/uc?export=view&id=1gcEY9iwG35fL4YmuXq9CeDw6J7kOkP-t"><img src="https://drive.google.com/uc?export=view&id=1gcEY9iwG35fL4YmuXq9CeDw6J7kOkP-t" style="width: 650px; max-width: 100%; height: auto" title="Click to enlarge picture" />