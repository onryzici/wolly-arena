# Woolly Arena

Yatay mobil ekran için Unity hayatta kalma / otomatik saldırı prototipi. Oyuncu hareket ve dash kontrolünü kullanır; hedef seçimi, ateş ve satın alınan özel güçler otomatik çalışır.

19 Eylül güncellemesi: **20 silah, dört build ailesi, yeni saldırı animasyonları, element efektleri ve iskelet parçalanması**. Silah listesi, bonuslar, seri tüfek dengesi ve doğrulama ayrıntıları: [Arsenal ve zorluk güncellemesi](docs/ARSENAL_AND_PRESSURE.md).

Ardından savaş HUD'ı ve seviye atlama ekranı yenilendi; düşmanlara kuşatma, yol kesme, menzil koruma ve sekiz bölgeden işaretli grup doğması eklendi. Güncel davranışlar ve **268 model kontrolü**: [Survivor arayüzü ve düşman grupları](docs/SURVIVOR_UI_AND_SQUADS.md).

Son görsel revizyon: 36 ayrı çizim ikon, sekiz ek ücretsiz KayKit modeli, tek renk oyuncu halkası, çarpısız doğma uyarıları ve güçlendirilmiş parçalanma/saldırı efektleri. [Görsel değişiklikler ve doğrulama](docs/INK_ART_AND_COMBAT.md).

VFX düzeltmesi: alev püskürtücü dokulu animasyonlu alev akışı, balta/kılıç kısa kesme izleri kullanır; mağaza ikonları daha sade cartoon çizimlerle değiştirildi. [VFX revizyonu ve yakın plan inceleme](docs/WEAPON_VFX_REVISION.md).

## Kaynak dosyaları

Blender ve GLB kaynakları Git LFS ile saklanır. Klonladıktan sonra `git lfs install` ve `git lfs pull` çalıştırın. Unity projesi `WoollyArenaTest/`, düzenlenebilir sanat kaynakları `art/`, üretim ve doğrulama araçları `tools/`, geliştirme notları `docs/` içindedir. Build çıktıları, Unity önbelleği ve yerel oyuncu kayıtları repoya dahil değildir.

## Açılış
Unity Hub ile `WoollyArenaTest` klasörünü açın. Unity sürümü: **6000.5.6f1**. Başlangıç sahnesi: `Assets/Woolly/Scenes/Startup.unity`; cartoon yükleme ekranından sonra lobi açılır.

Lobide **OYNA** test arenasına geçer; arenadaki **LOBİ** düğmesi geri döner. Mağaza, görev ve sosyal menüler prototip bilgi panelleridir; gerçek satın alma veya çevrimiçi servis içermez.

## Kontroller
- Mobil: sol kontrol hareket/koşu, sağdaki DASH düğmesi hızlı atılma. Hayatta kalma modunda manuel ateş ve doldurma düğmeleri gizlenir.
- Masaüstü: **WASD** hareket, **Space** dash. Hayatta kalma modunda koşu ve saldırı otomatik.
- Mobil: sağdaki çift ok düğmesi dash yapar. Hareket varsa o yöne, dururken son hareket yönüne atılınır. Düğmenin halkası ve süre göstergesi yeniden hazır olmayı gösterir.
- Hayatta kalmada silahlar kendi saldırı süreleriyle otomatik çalışır; manuel doldurma gerekmez. Eski manuel test modunda sekiz mermilik şarjör korunur.

## Dalgalar, ekipman ve karakter statları

20 dalgalık koşu. İlk dalga 30 saniye; normal dalgalar 44 saniyeye kadar uzar, final 90 saniyedir. Eşzamanlı düşman sınırı 18’den 60’a yükselir; uygun doğma alanı yoksa daha az düşman gelir. Altıncı dalgadan sonra ek takviyeler, yedinci dalgadan sonra işaretli alan saldırıları yapan elitler gelir. Her öldürme 1 XP verir; düşen para 1 altın değerindedir ve düşme olasılığı ilerledikçe %100’den %75’e iner. XP ile kazanılan seviye seçimleri dalga sonunda çözülür; ardından mağaza açılır. Hayatta kalınan dalga sonunda **Hasat** kadar ek malzeme kazanılır.

Karakterin görünür statları: maksimum can, hasar yüzdesi, saldırı hızı, hareket hızı, zırh, kritik şansı, 5 saniyelik can yenilenmesi ve hasat. Kritik isabetler 2 kat hasar verir. Pozitif zırh hasarı `1 / (1 + zırh × 0,06)` oranına indirir; negatif zırh hasarı artırır. İstatistik sınırları `SurvivalBuild.Stat` içinde belirtilmiştir.

Mağazada her seferinde dört rastgele teklif bulunur. 20 silah türü ve 16 pasif eşya vardır; eşyalar olumlu ve olumsuz stat değişikliklerini kartta gösterir. Nadirlik I–IV arasındadır; ilerleyen dalgalarda yüksek kademeler açılır.

- **Altı silah yuvası:** 20 tür arasından yakın dövüş, ateşli silah, element ve patlayıcı kombinasyonları kurulur. Aynı aileden 2 / 4 dolu yuva aile bonusunu açar. Galaxy Core, Storm Coil ve Falling Star özel güçlerinde aynı türün kademeleri toplanarak güç seviyesi belirlenir.
- **Birleştirme:** Aynı tür ve kademeden iki silah bir üst kademeye birleşir ve bir yuva açar. En yüksek kademe IV. Altı yuva doluyken alınan uyumlu silah otomatik birleşir; uyumsuz alışveriş para harcamadan engellenir.
- **Kilitleme:** Bir teklif, fiyatıyla birlikte yenilemeler ve sonraki mağaza boyunca korunur. Satın alınan teklif kilitten çıkar.
- **Yenileme:** Ücret `2 + dalga + bu mağazada yapılan yenileme sayısı × 2`. Tüm teklifler kilitliyken yenileme kapalıdır.
- **Geri dönüşüm:** Seçili eşya veya silah ödenmiş fiyatın yarısına satılır. Birleşmiş silahların yatırımları toplanır. Son silah satılamaz. Pasif eşya satılınca bütün bonusları ve cezaları geri kalkar.
- **Seviye atlama:** Dört farklı stattan biri ücretsiz seçilir. Bekleyen her seviye ayrı seçim verir. Tüm seçimler bitmeden sonraki dalga başlamaz.

**NEXT WAVE** mağazayı kapatır ve 10 can yeniler. Can sıfırlanınca koşu biter; 20. dalga zaferle biter. **PLAY AGAIN** envanteri, stat bonuslarını, XP'yi ve malzemeleri sıfırlar. Koşuya devam etme kaydı vardır; koşular arası kalıcı stat/metaprogresyon henüz yoktur.

Ana dosyalar: `SurvivalBuild` (katalog, ekonomi, statlar), `CharacterStats` (can/hız/zırh/kritik/yenilenme), `LoadoutCombat` (otomatik ekipman saldırıları), `SurvivalRun` (dalga akışı), `SurvivalShopUI` (statlar, kartlar ve envanter), `SurvivalPowers` ve `CartoonPowerVFX` (özel güçler). `EnemySpawnDirector.survivalMode = false` eski manuel test modunu açar.

## Duraklatma ve kayıt

**PAUSE / Escape** ile oyun duraklar. Uygulama arka plana geçtiğinde de duraklar; dönüşte oyuncunun **RESUME** düğmesine basması gerekir. Devam edince bir saniyelik hasar koruması verilir ve dokunmatik kontroller sıfırlanır. Lobiye geçişte zaman ölçeği geri yüklenir. Ses tercihi cihazda saklanır.

Dalga başında kayıt alınır. Mağazadaki alışveriş, satış, birleştirme, kilitleme, yenileme ve stat seçimleri de kaydedilir. Savaş ortasında çıkış **o dalganın başından**, mağazada çıkış **son işlemden** devam eder. Ekipman, can, statlar, XP, malzeme, kilitler ve rastgele seçim durumu korunur. Lobi, kayıt varsa **DEVAM ET** gösterir; **YENİ KOŞU** mevcut koşuyu bırakmadan önce onay ister. Yenilgi ve zafer kayıtları temizler.

Dosya `Application.persistentDataPath/survival-run-v1.sav` konumundadır. Geçici dosyaya yazma, atomik değiştirme, önceki sürüm yedeği, sürüm kontrolü ve SHA-256 bozulma kontrolü kullanılır. Bu kontrol güvenilirlik içindir; hile önleme sistemi değildir. Yarım yazılmış dosya kullanılmaz; ana kayıt bozuksa geçerli yedek denenir. Depolama hatası duraklatma/mağaza ekranında gösterilir. Bu disk davranışları bağımsız .NET testinde doğrulandı; iOS yaşam döngüsü henüz cihazda denenmedi.

## Doğrulama ve çalışma kuralı

**Kullanıcı açıkça istemeden Unity açılmaz; arka plan/batch oturumları da buna dahildir.** Bu kural [AGENTS.md](AGENTS.md) dosyasındadır.

`python3 tools/validate_equipment_offline.py`, Unity Editor'ü başlatmadan mevcut runtime/Editor kaynaklarını C# derleyicisiyle geçici dizinde derler ve gerçek ekonomi modelinin bağımsız testlerini çalıştırır. Mevcut yerel Unity referansları ve Library/Bee derleyici yanıt dosyaları gerekir. Üretilen DLL'ler Editor'e yüklenmez. Rapor: `WoollyArenaTest/Logs/equipment-offline-review.txt`.

19 Eylül arsenal güncellemesinde offline derleme ve **229 model kontrolü** geçti. Kullanıcının izniyle Unity'de 20 silahın isabetleri, yanma/yavaşlatma, duraklatma, mağaza, elitler, parçalanma ve seri tüfek baskı senaryoları çalıştırıldı. Güncel ayrıntılar [arsenal doğrulama notlarında](docs/ARSENAL_AND_PRESSURE.md), rapor `Logs/arsenal-runtime-review.txt` içindedir. Bu güncellemenin telefon build'i ve fiziksel cihaz testi yapılmadı.

Önceki doğrulama: 17 Eylül 2026'da model ve sahne entegrasyonu dahil 116 kontrol geçti; mağaza üç ekran oranında render edildi. Build 5 Unity/Xcode ile derlendi, imzası doğrulandı ve bağlı iPhone 14 Pro Max'e yüklenip başlatıldı. **Telefon üzerinde tam koşu, dokunmatik kullanım, kayıt yaşam döngüsü, performans ve ses testi henüz yapılmadı.** Önceki raporlar: `Logs/survival-review.txt`, `Logs/phone-build-report.txt`, `Logs/phone-xcode-build5.log`, `Logs/phone-install-build5.json`, `Logs/phone-launch-build5.json` (proje altında).

Kullanıcı Unity'yi açmayı istediğinde `Woolly > Review Survival`, gerçek sahnede mağaza, seviye seçimleri, altı silah, regen/zırh, 20 dalga, yenilgi/zafer ve üç ekran oranını kontrol etmek için hazırdır. Sonucu `Logs/survival-review.txt` dosyasına yazar. `Woolly > Capture Cartoon Powers` önceki VFX önizlemesini üretir.

İsabet alan karakter kısa süre açık renkte parlar ve darbe yönünde sendeler. Düşman 0,16 saniyede yaklaşık 0,38 m, oyuncu 0,16 m geri itilir; siperler ve gezinme sınırları bu mesafeyi kısaltır. `EnemyAgent.hitPushDistance` ve `ArenaPlayer.hitPushDistance` ile mesafe, `HitReaction` üzerinden parlama/sendeleme süresi ayarlanabilir. Koruma süresindeki veya hasar vermeyen vuruşlar tepki oluşturmaz.

`Woolly > Review Hit Reactions` gerçek atış, geri itme, siper ve NavMesh sınırı, peş peşe isabet, koruma, ölüm ve yeniden doğma kontrollerini çalıştırır. Sonuç `Logs/hit-reaction-review.txt` dosyasına yazılır; ardından temiz arena Play modunda açılır.

Dash 0,22 saniyede 2,2 m ilerler; başlangıçtan itibaren 0,65 saniye bekleme ve ilk 0,18 saniye hasar koruması vardır. Takla animasyonu kullanılmaz; gövde eğilmeden koşu animasyonu korunur, iki kısa mavi-beyaz hız izi kullanılır. CharacterController siper geçişini engeller. Otomatik ekipman dash sırasında saldırmayı sürdürür. Hayatta kalma koşusu 4,8 m/sn, yürüme 1,8 m/sn; mobil çubuk koşu hızını analog olarak kontrol eder. Revolver / repeater / shotgun temel atış aralıkları 0,40 / 0,17 / 0,65 saniyedir; repeater 18 atıştan sonra 1,35 saniye soğur. Galaxy / Energy / Star temel beklemeleri 3,2 / 2 / 2,8 saniyedir. Yakın dövüş hasarı sonrası 0,22 saniyelik koruma üst üste temas hasarını sınırlar.

Mevcut sahne referanslarını korumak için bileşen adı `DodgeAbility` olarak kaldı. `Woolly > Review Dash` artık dash davranışına göre güncellenmiştir; **Build 6 sonrası dash revizyonu ve açık renkli menüler Unity’de veya telefonda henüz çalıştırılmadı**. Offline derleme ve mevcut 63 ekonomi/kayıt kontrolü geçti; bunlar dash hareketi ya da telefon performansı testi değildir. Build 5 ve 116 Unity kontrolü önceki tempoya aittir.

## iOS
IL2CPP / ARM64 / Metal, yatay yön, iOS 15+. `Lobby` ve `TrainingArena` sahnelerini build listesine ekleyin. `Builds/iOS` çıktısını Xcode ile derleyin; kendi Apple geliştirme takımınızı seçin. Derleme çıktıları ve imzalama dosyaları repoya dahil değildir.

## Kaynaklar
- Düzenlenebilir karakter: `art/woolly/Woolly_Unity_AnimationSource_v4.blend`.
- UI parçaları: kullanıcı tarafından sağlanan GUI Pro-SuperCasual paketi. Paket varlıkları kendi lisanslarına tabidir.
- Lobi, paketin `1_Lobby` ekranından yatay ekrana uyarlanmıştır. Kullanılan PNG parçaları `Assets/Woolly/UI/Lobby/SuperCasual` klasöründedir.
- Lobi arka planı: `Assets/Woolly/UI/Lobby/Canyon Lobby.png`, yerleşik imagegen ile üretilmiş yatay kanyon panoraması. Tam üretim promptu yanındaki `Canyon Lobby provenance.txt` dosyasındadır. Shader görselin oranını korur; 16:9, 20:9 ve 4:3 kadrajları doğrulanır.
- [Sen ExtraBold](https://github.com/philatype/Sen), Lilita One ve Liberation Sans fontlarının lisansları ilgili Assets klasörlerindedir.
- Lobi düzeni: `Assets/Woolly/Editor/LobbySetup.cs`; kaydedilmiş Canvas sahnede düzenlenebilir. `Woolly > Rebuild Lobby` yalnızca lobiyi yeniden oluşturur.
- Görsel kontrol: `WoollyArena.Editor.LobbySetup.BuildAndCapture`, 16:9, 20:9 ve 4:3 önizlemelerini `Logs` klasörüne kaydeder. `WoollyArena.Editor.LobbyFlowReview.Run`, ayarlar/ses düğmelerini ve lobi → dört düşmanlı arena → lobi akışını kontrol edip lobiyi Play modunda açık bırakır.

### Build 6 ve sonraki yerel değişiklikler

Build 6 (ilk, daha sert dash + hızlı dalgalar + lobi alt çubuğu) 17 Eylül 2026’da derlendi, imzası doğrulandı, iPhone’a yüklendi ve başlatıldı. Ek Unity entegrasyon denemeleri alışveriş/stat ve saldırı kontrollerinde başarısız oldu; bu denemeler başarılı kabul edilmedi. Son tanı raporu `WoollyArenaTest/Logs/survival-review.txt` içindedir. Önceki 116 başarılı kontrol Build 5 içindir.

Kullanıcı telefondaki dash’in mesafe, görünüm ve yönünü beğenmedi. Ardından Unity kapatıldı ve yeniden açılmaması istendi. Kaynak dosyalardaki yeni dash 2,2 m / 0,22 sn, son hareket yönü, kesintisiz koşu ve kısa hız izleri kullanır. Duraklat/devam ve mağaza ekranları krem, mint, sıcak sarı ve açık mavi palete geçirildi. Bu son değişiklikler yalnızca offline derlendi; cihaz hâlâ Build 6 içerir.
