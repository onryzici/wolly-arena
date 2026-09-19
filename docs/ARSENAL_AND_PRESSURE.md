# Arsenal ve zorluk — 19 Eylül 2026

Bu güncellemeden sonra [survivor arayüzü ve bölgesel AI/spawn değişiklikleri](SURVIVOR_UI_AND_SQUADS.md) uygulandı. Aşağıdaki süre ölçümleri önceki spawn temposuna aittir. Otomatik joystick kontrolünün ekran kamerası eşlemesi de düzeltildi; eski hareketli kontrol süreleri güncel bir karşılaştırma olarak kullanılmamalıdır.

GitHub `origin/main` sürümü `b8d6df6` üzerine uygulanır. Mevcut altı silaha 14 yeni silah eklenir; aynı anda altı silah taşıma sınırı korunur. Yeni silahlar normal mağazadan alınır, birleştirilir, satılır ve mevcut v5 kayıt biçiminde ID ile saklanır. Eski enum değerleri değiştirilmez; eski koşular silinmez.

## Silahlar

| Aile | Silah | Oynanış |
|---|---|---|
| Nişancı | Altıpatlar | Dengeli tek mermi |
| Nişancı | Seri Tüfek | Hızlı seri; 18 atış sonrası otomatik soğuma |
| Nişancı | Çifte | Üç saçmalı kısa menzil |
| Nişancı | Delici Tüfek | Hazırlık sonrası üç hedefi deler; geçişte hasar azalır |
| Nişancı | Zıpkın Arbalet | İki hedefi delen daha sık atış |
| Nişancı | Üçlü Karabina | Aynı karede değil, aralıklı üç mermi |
| Nişancı | Sekme Tabancası | Yakın üç hedefe azalan hasarla seker |
| Yakın dövüş | Hilal Kılıcı | 120 derece süpürme |
| Yakın dövüş | Diken Mızrak | Dar ve uzun saplama |
| Yakın dövüş | Yarma Baltası | Ağır 180 derece darbe |
| Yakın dövüş | Deprem Çekici | Hazırlık sonrası çevresel şok |
| Yakın dövüş | Dönüş Bıçağı | Gidiş ve dönüşte ayrı vuruş |
| Yakın dövüş | Döner Testere | Çok kısa menzilde çevresel kesim |
| Element | Yıldırım | Yakın düşmanlar arasında zincir |
| Element | Alev Püskürtücü | Koni içinde hasar ve iki saniye yanma |
| Element | Buz Küresi | Alan hasarı ve süreli yavaşlatma |
| Patlayıcı | Girdap | Geniş alana büyü |
| Patlayıcı | Meteor | Yoğun alan hasarı |
| Patlayıcı | Bomba Atar | 0,65 saniye uçuş, kilitlenen konumda patlama |
| Patlayıcı | Kuşatma Havanı | 0,9 saniye uçuş, daha geniş ağır patlama |

Saldırılar süpürme, saplama, geri tepme, şarj, püskürtme, fırlatma ve dönüş hareketleri kullanır. Bunlar ekipmanın prosedürel animasyonlarıdır; yeni karakter iskelet klipleri olarak sunulmaz. 14 yeni silahın 3D modelleri `ArsenalModels` içinde üretilir; 256 px şeffaf mağaza ikonları aynı modellerin Unity render'ıdır. `ArsenalIconBake.Bake` bunları tekrar üretebilir.

## Build bonusları

Aynı aileden **2 / 4 dolu yuva** sayılır; kademe veya altı kopya bonusu sınırsız büyütmez. Birleştirme/satışla yuva sayısı azalınca bonus da yeniden hesaplanır. Mağazada aktif aile sayıları ve seçili silahın bonusu görünür.

| Aile | 2 silah | 4 silah |
|---|---|---|
| Nişancı | Aile silahlarına +%4 kritik | +%8 kritik |
| Yakın dövüş | +2 zırh, +%8 yakın saldırı hızı | +4 zırh, +%16 yakın saldırı hızı |
| Element | +%15 yanma, +4 puan yavaşlatma | +%30 yanma, +8 puan yavaşlatma |
| Patlayıcı | +%10 alan yarıçapı | +%20 alan yarıçapı |

Dövüşçü Sargısı, Kor Yakıtı, Buz Merceği ve Şarapnel Kesesi dördüncü mağazadan itibaren ek uzmanlaşmalar sunar; her birinin negatif stat karşılığı vardır. Mevcut Düellocu Rozeti yalnız ateşli silah/arbalet ve bomba atarı; Prizma Çekirdeği eski üç büyüyü ve buz küresini etkiler. Yeni enum değerleri yanlışlıkla bütün güç bonuslarını almaz.

Yanma en güçlü uygulamayla yenilenir, kopya başına üst üste eklenmez. Tekrarlanan uygulama sıradaki tik zamanını geciktirmez. Yavaşlatma normal düşmanda %45, bossta %15 ile; toplam alan yarıçapı bonusu %35 ile sınırlıdır. Siper kontrolü karakterleri engel saymaz, gerçek çevreyi ve kırılabilir siperleri dikkate alır.

## Seri tüfek ve geç oyun

- Seri tüfek temel hasarı 10 → 8, aralığı 0,14 → 0,17 saniye. 18 atıştan sonra 1,35 saniye otomatik soğur; saldırı hızı soğumayı kısaltmaz. Soğumada silah yukarı kalkar. Manuel doldurma gerektirmez.
- Ham saldırı hızı bonusunun ilk +%60'ı tam uygulanır; sonraki artışların %35'i uygulanır. Örneğin +%300 ham bonusun sonucu 4× yerine 2,44× hızdır. Stat paneli etkin hızı gösterir. Dash'in mevcut geçici bonusu ayrıca uygulanır.
- Normal düşmanın sendeletme aralığı ilk beş dalgada 0,6 sn, sonrasında 1,05 sn. Bu aralık düşmanın 0,28 sn hazırlıklı yakın saldırısını tamamlamasına izin verir. Yanma, alev ve testere düşmanı sürekli itmez.
- Elitler doğrudan vuruş başına `2 + floor(wave/4)`, bosslar `3 + floor(wave/5)` hasarı emer; kalan doğrudan hasar en az 1'dir. Yanma tikleri fiziksel plakayı aşar. Zayıf ve çok sık vuruşlar ile ağır vuruşların farklı kullanım alanları oluşur.
- Boss canı 5/10/15/20. dalgalarda **1.800 / 8.550 / 20.800 / 38.550**. Canı yarıya inince alan saldırıları daha sık gelir; işaretli hazırlık süresi kısalmaz. Normal düşmanların mevcut HP eğrisi değiştirilmez.
- Altıncı dalgadan sonra takviye baskınları; yedinci dalgadan sonra en çok 1–4 Kor Eliti. Elit hedef konumu 1,15 sn önceden kilitlenir; halka oyuncuyu takip etmez. Ölüm, dalga sonu ve devre dışı kalma saldırıyı iptal eder. Genel düşman sınırı 60'tır.

## Ölüm ve VFX

İskelet ölürken bütün modelin küçülmesi yerine kafatası, kaburga, uzuv ve zırh parçaları darbe yönünde savrulur. Kritik ve boss ölümleri daha güçlü dağılır. Parçalar zeminde sekip 1,25–1,9 sn içinde söner. Üst sınır 144 ortak parça; öldürme başına rigidbody oluşturulmaz. Mevcut para/sandık havuzları bu efektlerden ayrıdır.

Silah efektlerinde 48 çizgi çifti ve 192 parçacıkla sınırlı alev havuzu kullanılır. Hasar almamış sıradan düşmanların can barları gizlenir; düşük canlı hedeflerin barları isabet sonrası kısa süre görünür. Elit ve boss göstergeleri korunur. Renkler alev, buz, delici atış, sekme ve yakın darbeleri ayırır.

## Doğrulama

`tools/validate_equipment_offline.py`, kurulu Unity'nin C# derleyicisini Editor başlatmadan kullanır. Windows ve macOS kurulumlarını destekler; başka konum için `WOOLLY_UNITY_DATA` belirtilebilir. Runtime/Editor derlemesi ve **229 model kontrolü geçti**. Sonuç: `WoollyArenaTest/Logs/equipment-offline-review.txt`. Ekonomi simülasyonu varsayılan öldürme sayılarını kullanır; yeni silahların DPS'si veya boss süresi olarak sunulmaz.

Kullanıcının bu oturumdaki açık izniyle Unity açıldı. `Woolly > Review Arsenal Expansion`, 20 silahın gerçek düşman collider'larına otomatik hasarını, yanma/yavaşlatmayı, duraklatmayı, mağaza geçişini, elitleri ve gerçek parçalanma havuzunu sınar. Tam tur geçti; çalışma sırasında hata veya exception kaydı oluşmadı. 16:9, 20:9 ve 4:3 görüntüleri `Logs/arsenal-*.png`; rapor `Logs/arsenal-runtime-review.txt` içindedir. İnceleme oyuncunun kayıt ve kariyer dosyalarını yazmaz.

Son tam Unity turunda ölçülen senaryolar:

| Senaryo | Sonuç |
|---|---|
| Altı kademe-IV seri tüfek, +%300 ham hız, +%100 hasar, +25 kritik; sabit ve görünür final boss hedefi, 38.550 HP ve boss zırhı | 17,67 sn'de öldü; ilk beş saniyeyi yaşadı |
| Altı kademe-III seri tüfek, +%120 ham hız, +%60 hasar, +10 kritik; 100 HP, ek zırh yok; gerçek 16. dalgada hareketsiz | 4,50 sn'de yenildi |
| Aynı build; dokunmatik çubuk girdisi ve dash kullanan, düşmanlardan uzaklaşmaya çalışan otomatik rota | 9,35 sn'de yenildi; 41,51 m hareket etti |

Hareketli kontrolün önceki turu 25,91 sn sürdü. Gerçek kare zamanları, fizik ve rota kararları sonuçları değiştirir; süreler sabit denge hedefi değildir. Hareketli kontrolün sabit oyuncudan daha uzun yaşaması doğrulandı, dalgayı kazanması doğrulanmadı. Bunlar belirli regresyon senaryolarıdır; insanın oynadığı tam koşu, cihaz termal yükü/FPS ve genel denge onayı değildir. Telefon build'i veya fiziksel cihaz testi yapılmadı. Yeni animasyonlar ve VFX mevcut prototipi genişletir; AAA üretim kalitesi onayı anlamına gelmez.
