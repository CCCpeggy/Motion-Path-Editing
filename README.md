 Motion Path Editing

## 介紹

![Load_Result](document_data/Load_Result.gif)

* 載入多個 BVH 檔
* 編輯 BVH 的路徑
* 合併兩個動作
  ![Blend_Result](document_data/Blend_Result.gif)
* 將兩個動作連接
  ![Concat_Result](document_data/Concat_Result.gif)
* 相機選擇跟隨的動作
* 為骨架套上模型

## 使用教學

### 載入 BVH 檔

點擊 `Load` 按鈕，並且選擇要載入的 bvh

![Load_GIF](document_data/Load.gif)

> 注意：這邊只接受 18 個關節格式，且不支援 T pose

### 合併兩個動作

載入兩個 BVH 檔後，再點擊 `Blend` 按鈕，並且選擇要載入的 BVH

![Blend_GIF](document_data/Blend.gif)

### 連接兩個動作

載入兩個 BVH 檔後，再點擊 `Concat` 按鈕，並且選擇要載入的 BVH

![Blend_GIF](document_data/Concat.gif)

### 編輯動作路徑

選擇要編輯的動作後，點擊 `Edit Control Points`，接著透過拖移控制點來做編輯

![Edit_Path](document_data/Edit_Path.gif)

### 相機跟隨動作

在下拉選單中，選擇要跟隨的動作

## 技術說明

### BVH 解析

BVH 分成兩個部分，上半部是樹狀的骨架資訊，下半部是動作資訊。

#### 樹狀的骨架

樹狀的骨架包含的資訊

* 關節間彼此的樹狀連接關係與距離
* 針對此關節的旋轉或移動順序

使用遞迴去解析文件，且每一個關節使用一個類別表示，並且儲存相關的資訊。

#### 動作資訊

動作包含的資訊

* 幀數、一幀的時間、每一幀的資訊
* 每一幀裡面是依據前者的關節讀入順序與旋轉與移動順序的在當幀的值。

使用迴圈依序讀入，並使用陣列儲存。

### BVH 播放

在 Update() 中，對每一個關節，套入對應幀的移動與旋轉。
為了讓動作更加平滑，所以對於幀與幀之間會去做位置與旋轉角度的內插。

* 位置內插：$\text{new position} = (1 - t) \times \text{previous position} + t \times \text{next position}$
* 角度內插：使用 Unity 的 API `Quaternion.Lerp`

### 路徑

1. 用 Curve 去模擬原本動作的位移路徑，利用解矩陣的方式，使 Curve 與原本的經過的點距離最短 (目前只有使用一段 Curve 去模擬)。
2. 接著將 Curve 的位移與面相角度提出，去除此資訊的 Curve 應該是在原地並且原本移動的方向會朝向同一個方向。
3. 使用者可以透過編輯控制點，來操控曲線的樣子
4. 之後在取每一幀的時候，就會再加上對應 Curve 的位移與並旋轉至前進方向

<div class="info">

> 參考資料：[【Paper】Motion Path Editing](https://medium.com/maochinn/paper-motion-path-editing-c6779c24822b)

</div>

### Timewarp

!!! _ 來自 Registration Curves[^1] 中提出的 Timewarp

這邊將兩個動作分別用 a 跟 b 代稱。
建立一個表，列為 a 的所有 frame，欄為 b 的所有 frame，算出所有表格所對應的 a 與 b 的距離
其中距離使用前後共 5 個 frame 的動作去對其兩個動作，之後再去算兩個動作的關節距離差值總合做為距離。
然後使用 DP 從中找出一條符合限制且最長路徑作為結果，其中限制為：

* 斜率在 2 以內，也就是最多連續向橫向或值向走兩格
* 兩個 frame 的距離在目前最長的路徑的一半以內
* DP 時只會往左、上、左上找，以確保路徑只會往右、下、右下走

[^1]: Lucas Kovar and Michael Gleicher, Flexible Automatic Motion Blending with Registration Curves

### 合成

利用 Timewarp 知道最相近的 frame 對應後，將兩個之間的角度與距離平均，形成新的動作。

### 連接

這邊將兩個要連接的動作分別用 a 跟 b 代稱，即 a 後面要接著 b。
那概念就是把 b 的動作接在 a 的後面，但為了讓動作是有連貫的，所以在連接的地方需要做處理。
連接處利用 Timewarp 找到一段 a 與 b 之間相近的 frame 對應後，將兩個之間的角度與距離依據時間，使 a 跟 b 的比例從 1:0 到 0:1，就能呈現從 a 漸變到 b 的效果。

### 相機跟隨

計算出與相機與目標物體的向量，並使相機往那個方向前進，達到相機慢慢往目標的物件移動的效果。

<style>
.info blockquote
{
  color: #31708f;
  background-color: #d9edf7;
  border-color: #bce8f1;
}
</style>

<style>
.warning p
{
  color: #8a6d3b;
  background-color: #fcf8e3;
  border-color: #faebcc;
  padding: 15px;
  border: 1px solid transparent;
  border-radius: 4px;
}
</style>