# Requirements Document

## Introduction

Fitur ini mengganti mekanik minigame memasak yang lama (gerak-gerakkan mouse / "shakey-wakey" + manajemen suhu) dengan minigame mancing bergaya Stardew Valley yang berjalan di dalam UI Unity.

Pemain mengontrol sebuah kotak vertikal (Catch Bar) yang dapat didorong ke atas dengan menahan tombol mouse kiri dan jatuh ke bawah karena gravitasi saat dilepas. Sebuah target (ikon ikan / bahan masakan) bergerak naik-turun secara acak di dalam batang vertikal yang sama. Saat target berada di dalam jangkauan Catch Bar, sebuah Progress Bar terpisah akan terisi. Saat target di luar Catch Bar, Progress Bar berkurang. Pemain menang jika Progress mencapai 100% dan kalah jika Progress turun sampai 0%.

Minigame ini tetap dipanggil oleh `CookingStation` melalui method `StartMinigame(...)` dan, di akhir minigame, tetap memanggil `CookingStation.OnMinigameComplete(bool success)` agar integrasi dengan sistem cooking dan plating yang sudah ada tidak rusak. Setiap Recipe juga dapat menentukan tingkat kesulitan minigame-nya sendiri (kecepatan target, ukuran catch bar, kecepatan progress, dll.) lewat field tambahan di ScriptableObject `Recipe`.

Cakupan rilis ini adalah MVP: hanya catch bar, target, progress bar, dan kondisi menang/kalah. Tidak ada timer global, tidak ada zona suhu, tidak ada bonus zone, dan tidak ada tombol cancel.

## Glossary

- **CookingMinigame**: Komponen MonoBehaviour baru yang menggantikan `CookingTemperatureMinigame.cs`. Bertanggung jawab atas seluruh loop minigame mancing dan komunikasi hasil ke `CookingStation`.
- **CookingStation**: Komponen yang memulai minigame dan menerima hasil success/fail. Sudah ada di project (`Assets/Script/Cooking/CookingStation.cs`).
- **Recipe**: ScriptableObject yang mendeskripsikan resep masakan. Akan diperluas dengan parameter difficulty minigame.
- **Vertical_Bar**: Area UI vertikal tinggi yang menjadi "kolam" minigame. Catch Bar dan Target hanya bergerak di dalam area ini.
- **Catch_Bar**: Kotak hijau (region) yang dikontrol pemain di sumbu Y di dalam Vertical_Bar. Memiliki tinggi tertentu yang menentukan jangkauan tangkap.
- **Target**: Ikon (ikan / bahan masakan) yang bergerak naik-turun secara acak di dalam Vertical_Bar.
- **Progress**: Nilai numerik 0–100 yang merepresentasikan progres tangkapan saat ini.
- **Progress_Bar**: Bar UI terpisah yang memvisualisasikan nilai Progress.
- **UIAnimator**: Helper UI yang sudah ada di project (`Assets/Script/UI/UIAnimator.cs`) untuk show/hide panel.
- **PlayerInput_Hold**: Input "tahan tombol mouse kiri" yang dideteksi melalui `Input.GetMouseButton(0)`.
- **Difficulty_Settings**: Sekumpulan parameter (catch bar size, target speed, progress gain rate, progress loss rate, gravity, push force) yang menentukan tingkat kesulitan minigame untuk satu Recipe.

## Requirements

### Requirement 1: Memulai Minigame dari CookingStation

**User Story:** Sebagai sistem CookingStation, saya ingin memulai minigame mancing untuk sebuah resep tertentu, sehingga proses memasak diteruskan ke CookingMinigame tanpa mengubah alur cooking yang sudah ada.

#### Acceptance Criteria

1. WHEN CookingStation memanggil method start minigame dengan referensi station dan resep yang sedang dimasak, THE CookingMinigame SHALL menyimpan referensi tersebut dan menampilkan panel UI minigame.
2. WHEN CookingMinigame menampilkan panel UI minigame, THE CookingMinigame SHALL menggunakan UIAnimator untuk menampilkan panel jika UIAnimator ter-assign.
3. IF UIAnimator tidak ter-assign, THEN THE CookingMinigame SHALL mengaktifkan GameObject panel minigame secara langsung.
4. WHEN minigame dimulai, THE CookingMinigame SHALL mengatur Progress ke nilai awal 50.
5. WHEN minigame dimulai, THE CookingMinigame SHALL menempatkan Catch_Bar pada posisi vertikal awal di tengah Vertical_Bar.
6. WHEN minigame dimulai, THE CookingMinigame SHALL menempatkan Target pada posisi vertikal awal di tengah Vertical_Bar.

### Requirement 2: Kontrol Catch Bar dengan Mouse Kiri

**User Story:** Sebagai pemain, saya ingin mengontrol Catch Bar dengan menahan tombol mouse kiri, sehingga saya dapat menyesuaikan posisi tangkap dengan ritmis seperti minigame mancing Stardew Valley.

#### Acceptance Criteria

1. WHILE pemain menahan tombol mouse kiri, THE CookingMinigame SHALL menambahkan akselerasi ke atas pada Catch_Bar sebesar pushForce per detik kuadrat.
2. WHILE pemain tidak menahan tombol mouse kiri, THE CookingMinigame SHALL menambahkan akselerasi ke bawah pada Catch_Bar sebesar gravity per detik kuadrat.
3. THE CookingMinigame SHALL memperbarui posisi vertikal Catch_Bar setiap frame berdasarkan kecepatan vertikal saat itu dan delta time.
4. IF posisi vertikal Catch_Bar mencapai batas atas Vertical_Bar, THEN THE CookingMinigame SHALL menahan posisi Catch_Bar di batas atas dan menyetel kecepatan vertikalnya ke nol.
5. IF posisi vertikal Catch_Bar mencapai batas bawah Vertical_Bar, THEN THE CookingMinigame SHALL menahan posisi Catch_Bar di batas bawah dan menyetel kecepatan vertikalnya ke nol.

### Requirement 3: Pergerakan Target Secara Acak

**User Story:** Sebagai pemain, saya ingin Target bergerak naik-turun secara acak di dalam batang vertikal, sehingga minigame terasa menantang dan tidak dapat ditebak.

#### Acceptance Criteria

1. THE CookingMinigame SHALL memilih posisi vertikal tujuan baru untuk Target di dalam batas Vertical_Bar setelah Target mencapai posisi tujuan sebelumnya.
2. THE CookingMinigame SHALL menggerakkan Target menuju posisi tujuan saat ini dengan kecepatan targetSpeed dari Difficulty_Settings.
3. THE CookingMinigame SHALL memastikan posisi Target setiap frame berada di dalam batas Vertical_Bar.
4. WHEN Target mencapai posisi tujuan saat ini, THE CookingMinigame SHALL memilih waktu jeda acak antara minIdleTime dan maxIdleTime sebelum memilih posisi tujuan berikutnya.

### Requirement 4: Perhitungan Progress Berdasarkan Overlap

**User Story:** Sebagai pemain, saya ingin Progress bertambah saat Target berada di dalam Catch Bar dan berkurang saat di luar, sehingga keberhasilan minigame ditentukan oleh seberapa lama saya berhasil menjaga Target di dalam Catch Bar.

#### Acceptance Criteria

1. WHILE posisi vertikal Target berada di dalam rentang vertikal Catch_Bar, THE CookingMinigame SHALL menambah Progress sebesar progressGainRate dikalikan delta time setiap frame.
2. WHILE posisi vertikal Target berada di luar rentang vertikal Catch_Bar, THE CookingMinigame SHALL mengurangi Progress sebesar progressLossRate dikalikan delta time setiap frame.
3. THE CookingMinigame SHALL menjaga nilai Progress dalam rentang 0 sampai 100 setelah setiap pembaruan.
4. THE CookingMinigame SHALL memperbarui nilai Progress_Bar setiap frame agar sama dengan nilai Progress saat itu.

### Requirement 5: Kondisi Menang dan Kalah

**User Story:** Sebagai pemain, saya ingin minigame berakhir dengan jelas saat Progress penuh atau kosong, sehingga hasil masakan langsung diketahui.

#### Acceptance Criteria

1. WHEN Progress mencapai atau melebihi 100, THE CookingMinigame SHALL mengakhiri minigame dengan status success bernilai true.
2. WHEN Progress mencapai atau berada di bawah 0, THE CookingMinigame SHALL mengakhiri minigame dengan status success bernilai false.
3. WHEN minigame berakhir, THE CookingMinigame SHALL menghentikan loop update minigame sehingga input pemain tidak lagi memengaruhi Catch_Bar atau Progress.

### Requirement 6: Pengembalian Hasil ke CookingStation

**User Story:** Sebagai sistem CookingStation, saya ingin menerima hasil minigame dengan kontrak yang sama seperti minigame lama, sehingga sistem cooking dan plating yang sudah ada tidak perlu diubah.

#### Acceptance Criteria

1. WHEN minigame berakhir, THE CookingMinigame SHALL memanggil method `OnMinigameComplete(bool success)` pada CookingStation yang disimpan saat minigame dimulai.
2. WHEN minigame berakhir, THE CookingMinigame SHALL menyembunyikan panel UI minigame menggunakan UIAnimator jika UIAnimator ter-assign.
3. IF UIAnimator tidak ter-assign saat minigame berakhir, THEN THE CookingMinigame SHALL menonaktifkan GameObject panel minigame secara langsung.
4. WHEN minigame berakhir, THE CookingMinigame SHALL melepaskan referensi ke CookingStation agar siklus minigame berikutnya dimulai dari kondisi bersih.

### Requirement 7: Konfigurasi Kesulitan per Recipe

**User Story:** Sebagai desainer game, saya ingin tiap Recipe punya tingkat kesulitan minigame sendiri, sehingga resep yang lebih kompleks terasa lebih sulit dimasak.

#### Acceptance Criteria

1. THE Recipe SHALL menyediakan field Difficulty_Settings yang berisi parameter catchBarSize, targetSpeed, progressGainRate, progressLossRate, gravity, dan pushForce.
2. WHEN minigame dimulai untuk sebuah Recipe, THE CookingMinigame SHALL menggunakan nilai-nilai Difficulty_Settings dari Recipe tersebut untuk menginisialisasi parameter minigame.
3. WHERE sebuah Recipe tidak menentukan Difficulty_Settings (mis. nilai default), THE CookingMinigame SHALL menggunakan nilai default yang diset di Inspector pada komponen CookingMinigame.
4. THE Difficulty_Settings.catchBarSize SHALL dinyatakan sebagai panjang vertikal Catch_Bar relatif terhadap panjang Vertical_Bar dalam rentang 0 sampai 1.

### Requirement 8: Struktur dan Visualisasi UI Minigame

**User Story:** Sebagai pemain, saya ingin melihat batang vertikal, catch bar, target, dan progress bar dengan jelas, sehingga saya dapat membaca keadaan minigame secara cepat.

#### Acceptance Criteria

1. THE CookingMinigame SHALL menampilkan sebuah Vertical_Bar sebagai container vertikal yang membatasi gerakan Catch_Bar dan Target.
2. THE CookingMinigame SHALL menampilkan sebuah Catch_Bar sebagai region berwarna hijau di dalam Vertical_Bar yang posisi vertikalnya mengikuti posisi Catch_Bar saat itu.
3. THE CookingMinigame SHALL menampilkan sebuah Target berupa Image dengan ikon yang dapat diatur dari Inspector dan posisinya mengikuti posisi Target saat itu.
4. THE CookingMinigame SHALL menampilkan sebuah Progress_Bar berupa Slider yang nilainya mengikuti nilai Progress saat itu.
5. THE CookingMinigame SHALL menyediakan field publik di Inspector untuk menugaskan referensi `RectTransform` Vertical_Bar, Catch_Bar, dan Target, serta `Slider` Progress_Bar.

### Requirement 9: Kompatibilitas Lokasi File dan Penggantian Komponen

**User Story:** Sebagai developer, saya ingin file minigame baru tetap berada di lokasi yang sudah dipakai sistem cooking, sehingga saya tidak perlu mengubah referensi prefab dan scene yang sudah ada.

#### Acceptance Criteria

1. THE CookingMinigame SHALL ditempatkan pada path file `Assets/Script/Cooking/CookingTemperatureMinigame.cs` (file lama digantikan isinya, bukan dipindahkan).
2. THE CookingMinigame SHALL mempertahankan nama class `CookingTemperatureMinigame` agar referensi `temperatureMinigame` di `CookingStation` tetap valid.
3. THE CookingMinigame SHALL mempertahankan signature method publik `StartMinigame(CookingStation station)` agar pemanggilan dari `CookingStation.StartCooking` tetap berjalan tanpa modifikasi.
