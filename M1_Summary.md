# M1 Prototip Özeti - Turbo Garaj

## Yapılanlar

1. **Unity Projesi Kurulumu**
   - Unity 2022 LTS (3D Mobile şablonu, URP) kullanılarak proje oluşturuldu.
   - Proje kök directory: `C:\Users\Hasan\Desktop\TurboGaraj`

2. **Klasör Yapısı** (Assets altında)
   - Scripts/
     - Vehicle/
     - Economy/
     - Track/
     - UI/
     - Save/
   - Prefabs/
     - Vehicle/
   - Scenes/
   - Materials/
   - Textures/

3. **VehicleController.cs** (Assets/Scripts/Vehicle/VehicleController.cs)
   - WheelCollider tabanlı arcade fizik sistemi.
   - Otomatik gaz (sabit throttle input, otomatik uygulanır).
   - Stamina sistemi: zamanla azalan stamina, azaldıkça maksimum hız düşer.
   - Özellikler:
     - `baseMaxSpeed`: Stamina tam olduğunda temel maksimum hız (m/s).
     - `staminaSpeedInfluence`: Stamanın hız üzerindeki etkisi (0-1).
     - `initialStamina`: Başlangıç stamina değeri (0-100).
     - `staminaDrainRate`: Saniyede stamina azalma miktarı.
     - `throttleInput`: Otomatik gaz giriş değeri (0-1).
   - Rear-wheel drive modeli (arka tekerlekler motor torque alır).
   - Basit hız sınırlama (drag tabanlı).

4. **StaminaUI.cs** (Assets/Scripts/UI/StaminaUI.cs)
   - UI Slider'ı stamina seviyesine göre günceller.
   - VehicleController referansı gerektirir.
   - Slider'ın min/max değerleri 0-1 arasını ayarlar.
   - Her framede stamina normalized değerini okuyarak slider değerini ayarlar.

5. **Test Sahnesi Hazırlığı (Kullanıcı tarafından tamamlanması gerekenler)**
   - Yeni bir sahne oluşturun (Assets/Scenes/TestScene.unity).
   - Sahneye uzun ve geniş bir Plane ekleyin (yol için).
   - Araç Prefabı oluşturun:
     - Boş bir GameObject oluşturun (aracın kökü).
     - Rigidbody ekleyin (ağırlık ~1000).
     - VehicleController scriptini ekleyin.
     - 4 WheelCollider oluşturun (arka ve ön tekerlekler için ayrı ayrı alt objeler olarak) ve VehicleController'daki ilgili alanlara atayın.
     - Görselleştirme için Placeholder olarak bir Capsule veya Cube ekleyin (aracın gövdesi).
     - Bu hiyerarşiyi Prefab olarak kaydedin (Assets/Prefabs/Vehicle/VehiclePrefab.prefab).
   - Sahneye bir adet araç prefabı örneğini yerleştirin (yolun başında).
   - UI Canvas oluşturun:
     - Canvas > Slider (yatay) ekleyin.
     - Slider'ın Min değeri 0, Max değeri 1 olsun.
     - StaminaUI scriptini Slider objesine ( veya üstüne ) ekleyin.
     - StaminaUI'deki Vehicle referansını sahnenedeki araç örneğine bağlayın.
   - Play tuşuna basarak testi yapın: araç ileri gitmeye başlamalı, stamina barı zamanla boşalmalı ve azalana kadar maksimum hız düşmeli.

6. **Android Build Ayarları** (Build hedefi için)
   - File > Build Settings > Android seçin ve Switch Platform.
   - Player Settings:
     - Company Name ve Product Name girin.
     - Minimum API Level: Android 5.0 (Lollipop) veya üzeri önerilir.
     - Target Architectures: ARM64 ve ARMv7 (IL2CPP scripting backendi önerilir performans için).
     - Resolution ve Presentation: Fullscreen ve yön kilidi (Landscape Left) ayarlayın.
   - Build butonu ile APK oluşturun.

## Notlar
- Bu M1 prototibi, ekonomi, ses, görsel mod ve kaydetme sistemlerini içermez.
- Sadece "araç gidiyor, stamina tükeniyor, yavaşlıyor" mekaniği işlevleştirilmiştir.
- Stamina tükendiğinde araç duracaktır (motor sıfır çünkü stamina 0 → maxSpeed = baseMaxSpeed * (1 - staminaSpeedInfluence) but actually our formula: currentMaxSpeed = baseMaxSpeed * (1f - staminaSpeedInfluence * (1f - staminaNormalized)); when staminaNormalized=0, currentMaxSpeed = baseMaxSpeed * (1 - staminaSpeedInfluence). So if staminaSpeedInfluence=1, then maxSpeed=0 when stamina=0.)
- StaminaUI, VehicleController'dan normalleştirilmiş stamina değerini okur (0-1).

## Sonraki Adımlar (M2 ve sonrası)
- Ekonomi sistemi (para kazanma, harcama).
- Görsel modellar ve tekstürler.
- Ses efektleri ve müziqe.
- Daha karmaşık araç kontrolleri (yönlendirme, fren).
- Farklı araç tipleri ve yükseltmeler.
- Kaydetme/Yüklenme sistemi.
- UI geliştirmeleri (menüler, butonlar).
- Android Build ve test.