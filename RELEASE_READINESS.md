# Woolly Arena — yayın hazırlığı

## Son düzeltme — lobide tekrar küçülme

Lobi boyutu artık mevcut/animasyonun değiştirebildiği ölçekle çarpılarak güncellenmez. Skinned meshler birim ölçekte açık BakeMesh(useScale:false) ile ölçülür; Animator.Rebind etkisinden sonra pozisyon, rotasyon ve ölçek geri yüklenir. Ekran boyutu her karede aynı yerel yükseklik ve üst nesne ölçeğinden mutlak olarak hesaplanır. Böylece iki kare sonraki yeniden ölçüm ve ekran geçişleri önceki boyuta bağımlı değildir. Lobi karakter alanı 430'dan 500'e büyütüldü, ayak tabanı alt navigasyonun hemen üstünde tutuldu. Kaynak derleme ve 150 model testi geçti; Unity görsel doğrulaması çalıştırılmadı, Editor açılmadı/öne getirilmedi ve build alınmadı.


## Güncel revizyon — lobi uyumu, karakter güçleri ve aksesuarlar

- Sağ harita kartı daha kompakt: önizleme en-boy oranı korunuyor; başlık, dalga/boss açıklamaları yeniden yerleştirildi. Yeni Koşu butonu 52 yerine 84 birim yüksekliğinde.
- Vera'nın ilk Start sırasında alınan skin bounds önbelleği iki kare sonra iptal edilip yeniden ölçülüyor; ilk ölçümden önce Animator.Rebind/Update(0) ile poz hazırlanıyor. Bu kaynak düzeltmesi, ilk açılış/karakter geçişi görsel kontrolü bekliyor.
- Karakter seçiminin alt thumbnail düğmeleri kaldırıldı. Oklar ve kaydırma korundu; alt alan karaktere ayrıldı. Mor shader dalı yerine aynı hareketli çöl panoraması kullanılıyor.
- Yükseltme/dükkân UI lobinin mevcut paketinden mavi/sarı düğme ve koyu çerçeve dokularını Resources/MenuSkin üzerinden kullanır. LevelUp ekranında boş eşya/silah alanları gizlenir, dört kart genişler. Sekiz stat ikonu artık atlas kırpması yerine düz renkli vektör çizimdir.
- Mevcut seçilen stat bonusları çalışıyordu; hız/hasar bonusunun 0% gösterimi toplam 100% biçimine çevrildi. Kartlarda önce/sonra değerleri var. Woolly her seviyede +2 azami can, Vera +1 yüzde puanı saldırı hızı kazanır; seçilen ödüller ayrıca uygulanır. Level 1 ve eski -1 profilli kayıtların başlangıç dengesi korunur. Bunlar koşu içi seviyelerdir; karakter seçim ekranı başlangıç değerlerini gösterir.
- Dash karaktere göre renk/mesafe ve kısa eğilme pozu alır. Woolly'nin Yün Kalkanı: 8 saniyede bir dash tetiklemesinde 0.4 sn koruma ve 1.8 m içindeki düşmana 8 hasar. Vera Neon Atak: 8 saniyede bir dash tetiklemesinde 2 sn +%25 saldırı hızı. Hareket dashinin bekleme süresi bu özel güç zamanlayıcısından ayrıdır.
- Mağaza üç basit 3D aksesuar sunar: şapka, gözlük, kolye. Satın alınan ürün iki karakterde de kullanılabilir; takılı seçimler karakter bazında saklanır. Lobide ve oyunda baş/boyun takibi vardır. Aksesuarlar stat vermez. Katalog ikonları vektördür; yeni AI bitmap üretilmedi. Saç/baş/silah ile çakışmalar **Unity'de henüz gözle kontrol edilmedi**.
- KayKit düşman ölçeği .80'den .70'e düşürüldü (~%12.5). Normal büyücüler ilk dalgadan itibaren eşzamanlı sayısı sınırlı, 0.9 sn nişan uyarılı, 6.5 m/sn küre büyüsü kullanır. Projectile çarpışmaları sahne engellerini dikkate alır; boss alan büyüsü korunur.
- Çevrimdışı runtime/editor C# derleme ve 150 model kontrolü geçti; 75 varlık kontrolü geçti. Unity açılmadı/öne getirilmedi, Play/Stop veya build yapılmadı. Shader derlemesi, yeni UI yerleşimi, aksesuar uyumu ve dash/büyü runtime davranışı doğrulanmayı bekliyor.


## Son lobi düzeltmesi — Vera ve bulutlar

- Vera için saç yüksekliği telafisi karakter seçim ekranında yanlışlıkla kapalıydı. Artık lobi ve seçim ekranında aynı 1.18 katsayısı var (önceki lobi 1.10, seçim 1.00); taban hizası korunuyor.
- Vera ayak izleri: FootstepDust artık ArenaPlayer karakter değişimini tamamladıktan sonra aktif Animator iskeletindeki LeftFoot/RightFoot kemiklerini yeniden bağlar; sahnenin devre dışı Woolly kemiklerine bakmaz. İz yönü aynı iskeletteki parmak kemiğinden alınır. Başlangıç adım durumu sıfırlanır, eksik kemik/duman referansları korumalıdır. Oyun içi doğrulama bekliyor.
- Kullanıcının beğenmediği prosedürel tumbleweed tamamen kaldırıldı; ince zemin rüzgârı korundu.
- Eski bulut kayması yalnızca üst dar gökyüzüne uygulanıyordu ve tepe hızı yaklaşık 0.55 görüntü pikseli/s idi. Maske açık merkezde aşağıya genişletildi, kenarlarda kayalıkların üstünde tutuldu. Tepe hız yaklaşık 9.6 görüntü pikseli/s; hareket mevcut panorama bulutlarını kullanıyor.
- Unity açılmadan kaynak değişikliği yapıldı. Gerçek lobi görünümü, saçın başlıkla mesafesi ve shader derlemesi henüz Unity'de doğrulanmadı; build alınmadı.


## 18 Eylül — pembe shader, ölçülü duman ve çalışan lobi menüleri

- Mevcut Editor.log içindeki Metal hatası `LobbyBackdrop.shader: unexpected token line` ile doğrulandı. HLSL anahtar sözcüğü olan değişken adı `twigDistance` olarak düzeltildi. Yeni bir Unity oturumu/yeniden import başlatılmadı; shaderın yeni derlemesi henüz doğrulanmadı. PhonePresentationCheck artık lobi shader hatasında da exportu durdurur.
- Namlu dumanı geri eklendi: yalnızca oyuncu atışları, 0.12 saniyede en fazla bir puf, 8 parçacık bütçesi, 0.075 başlangıç boyutu, 0.26 saniye ömür, 0.16 alfa ve 0.008 ekran boyutu sınırı. Doku eksikse parçacık rendererı beyaz kart çizmek yerine kapalı kalır.
- KayKit sütunları kaldırıldı; variller, kasalar ve diğer dekorlar korundu.
- Dükkândan bağımsız cartoon sonuç paneli: krem kart, kalın koyu çerçeve, mavi başlık, üç büyük sonuç sayacı, görev ödülü durumu; Lobiye Dön ve Tekrar Oyna eylemleri.
- Altı kalıcı görev, tek seferlik ödül alma, UTC günlük hediye, kişisel rekorlar ve üç kuşanılabilir profil rozeti eklendi. Tek PlayerPrefs JSON kaydı ilerleme/cüzdan/ödül bayraklarını birlikte tutar. Lobi altını savaş materyallerinden ayrıdır; rozetler stat vermez. Sahte çevrimiçi sıralama/arkadaş listesi eklenmedi.
- İlerleme dalga sonunda, alışveriş/seviye değişiminde, duraklatırken ve lobiye çıkarken kaydedilir; kayıtlı bir koşuyu yeniden oynamak en iyi değerleri tekrar toplamaz. Günlük hediye aynı gün veya geri alınmış cihaz tarihinde tekrar verilemez; sunucu saati doğrulaması yoktur.
- Çevrimdışı C# derleme ve 143 model kontrolü (14 yeni görev/ödül/rozet kontrolü dahil) geçti. Unity shader derlemesi, gerçek ekran yerleşimleri, telefon ve PlayMode kontrolleri yapılmadı. Unity açılmadı, öne getirilmedi ve build alınmadı. Cartoon dönüşümü bu tur menü sunumuyla sınırlıdır; mevcut envanter ve ortam çizimlerinin tamamı yeniden çizilmiş değildir.


## 18 Eylül — dev geometri ve lobi rüzgârı (yalnızca çevrimdışı)

- Ekran görüntüsündeki kapatıcı şekiller için FBX ölçeğinde somut hata bulundu: karakter FBX köklerinde 100 ölçeği varken ModelImporter useFileScale kapalıydı. Export artık FBX_SCALE_UNITS kullanır; KayKitImport v3 dosya birimlerini açıkça uygular. Dört karakter yeniden üretildi. Bu teşhis kaynak verisine dayanır; aynı sahnede Unity regresyon kontrolü henüz yapılmadı.
- Blender ile dört karakter × yedi klip × 17 poz = 476 poz denetlendi: sonlu mesh koordinatları, 0.2–4 metre boyut aralığı, birim silah soketi ölçeği ve FBX düğümlerinde 100 ölçeğinin bulunmaması geçti. Rapor: Logs/kaykit-scale-offline.json.
- PhoneBuild ön kontrolüne KayKitScaleChecks eklendi: Unity'nin gerçekten içe aldığı skinned meshleri her klipte BakeMesh ile ölçer; dev/küçük/geçersiz geometri veya yanlış soket ölçeğinde export başlamaz. Bu Unity kontrolü **yazıldı ve çevrimdışı derlendi, çalıştırılmadı**.
- Kullanılmayan namlu dumanı sistemi tamamen kaldırıldı. Önceki düşük yoğunluklu ayak/dash tozu sınırları korunuyor.
- Lobinin mevcut arka plan shaderına üst gökyüzünde yavaş hareket, yalnızca zeminde düşük opaklıklı rüzgâr çizgileri ve 32 saniyede bir 10 saniyelik küçük yuvarlanan çalı geçişi eklendi. Karakter seçim ekranına uygulanmaz. Saat unscaledTime ile beslenir; karakter ve UI katmanlarının arkasındadır.
- C# çevrimdışı derleme ve 129 model kontrolü geçti. Lobi shaderının Unity derlemesi, animasyon geçişleri, oyun içi görüntü ve cihaz kontrolü bekliyor. Kullanıcı talimatıyla Unity açılmadı/öne getirilmedi, Play/Stop yapılmadı, build veya telefon yüklemesi başlatılmadı. Görsel regresyon henüz kapatılmadı.

## Görsel regresyon bildirimi — duman ve renksiz modeller

Kullanıcı yeni entegrasyonda dumanın görüntüyü kapattığını ve modellerin renksiz göründüğünü bildirdi. Önceki çevrimdışı kontroller görsel doğrulama sayılmaz. Kaynak düzeltmeleri: namlu dumanı emisyonu kapalı; ayak/kırılma/dash tozu azaltılmış ve ekran boyutu sınırlı; parçacık ölçeklemesi Shape. KayKit renderer malzemeleri artık boş olabilecek importer material listesi yerine mesh subMeshCount üzerinden atanıyor. Ayrı KayKitSurface shader özgün UV/palet dokusunu doğrudan okuyor.

C# çevrimdışı derlendi, 129 model kontrolü geçti. Shaderın Unity'de derlenmesi, renderer üzerindeki gerçek materyal/doku ve oyun içi görünürlük **henüz doğrulanmadı**; bu regresyon görsel kontrol geçmeden kapatılmış sayılmamalı.


## Güncel ek revizyon — KayKit ve kayıtlı SFX

- Kullanıcı isteğiyle ücretsiz KayKit Dungeon Remastered / Skeletons CC0 paketleri eklendi. Aktif düşman sunumu dört KayKit karakterine geçti; önceki altı üretilmiş model yalnızca yükleme başarısızlığında yedektir.
- Her karakterde kaynak paketinden yedi animasyon bulunur; dört FBX tekrar içe alınarak UV, 41 kemik ve koşarken ayak hareketi doğrulandı. Legacy Animation ayarlarını KayKitImport uygular; Unity'de uygulandığı henüz doğrulanmadı.
- Kırılabilir kasa/fıçılar KayKit modelleriyle değiştirilir; yeni resim meshleri yıkım sistemine kaydedilir. Sütun, meşale, bayrak, altın sandık ve fıçı grupları yürüme sınırının dışına yerleştirilir. Gerçek sahnede ölçek/çarpışma kontrolü bekler.
- 17 Kenney ve 6 Free Firearm kayıtlı ses; 14 ses kanalı, kategori bekleme süresi, varyasyon, mesafe azaltımı ve pause desteği. Silahlar, güçler, lazer, darbe, altın, sandık, dash ve adımlar bağlıdır. Eski üretilmiş sinüs sesleri aktif kaynaklardan kaldırıldı.
- Dört Kenney parçacık dokusu namlu, kıvılcım, duman ve isabete bağlandı.
- 129 model kontrolü + 75 sunum kaynağı kontrolü geçti. Kaynak C# derlendi; Unity shader derleme/PlayMode, dinleme ve cihaz testi yapılmadı.
- Lisanslar ve değişiklik kaydı THIRD_PARTY_NOTICES.md ve Assets/Woolly/ThirdParty altında.

Aşağıdaki önceki düşman revizyonu tarihçedir.


## Güncel durum — 18 Eylül, düşman/ses/efekt revizyonu

Yayınlanmaya hazır değil; yeni varlıklar ve kod çevrimdışı hazırlanmıştır.

- Altı düşman artık tek birleşik gövde yerine ayrı anatomik pivotlara sahip. Hareket mesafesine bağlı adım, karşı kol salınımı, ayak kaldırma, örümcek bacak döngüsü ve saldırı pozu eklendi. Genel gövde zıplaması kaldırıldı.
- Yakın saldırı 0,28 saniye hazırlıktan sonra menzil/görüş kontrolüyle tek kez vurur. Darbe ile kesilme, menzilden kaçma ve çok uzun karede eski saldırının iptali için kontroller eklendi.
- Modeller parça başına tek malzeme/vertex renkleri kullanır; 2.816–3.158 kaynak vertex/model. Gerçek cihaz kare süresi henüz ölçülmedi.
- 24 çarpma parlaması ve 96 kıvılcık için sabit havuz; kısa çizgi efektlerine konturlu yıldız ve genişleyen halka katmanı eklendi.
- Reddedilen ArenaRush müziği Resources dışına arşivlendi. Kevin MacLeod — Robo-Western MP3 eklendi; CC BY 4.0 atfı Ayarlar ekranında ve THIRD_PARTY_NOTICES.md içinde. Unity/telefon dinleme kontrolü yapılmadı.
- Final dalgası zaman çubuğu gerçek 90 saniyelik süreyi kullanacak şekilde düzeltildi.
- Güncel çevrimdışı C# derlemesi ve **129 model kontrolü geçti**. FBX dosyaları Blender ile yeniden içe alınıp bağımsız pivotlar, renkler ve malzeme bütçesi kontrol edildi. Blender model önizlemesi oyun içi görüntü değildir.

### Açık yayın engelleri

1. Kullanıcı izin verdiğinde Unity içe aktarımı, shader derlemesi ve düşmanların gerçek sahnede ayak basışı/saldırı yönü; yeni müzik ve efektlerin işitsel/görsel değerlendirmesi.
2. Telefonda kalabalık düşman + altı silah + lazer/güç efektleriyle kare süresi, bellek ve ısınma ölçümü.
3. Birkaç tam 20 dalgalık koşu: sabit durarak kazanma, ekonomi eğrisi, boss okunabilirliği ve zafer/yenilgi akışları. Matematik kontrolleri gerçek oynanış kanıtı değildir.
4. Kayıt/devam, uygulama arka planı, güvenli ekran alanı ve çoklu dokunma regresyonu. Önceki Build 6 sahne kontrollerindeki başarısızlıklar güncel sahnede yeniden sınanmalıdır.
5. İlk koşu öğreticisi, kalıcı koşu geçmişi/başarı motivasyonu ve lobide tamamlanmamış mağaza/görev alanlarının ürün kapsamı. Bunlar tamamlandı olarak işaretlenmemiştir.

Aşağıdaki bölümler önceki sürümlerin tarihçesidir; güncel doğrulama kanıtı yerine kullanılamaz.

## Önceki durum

Durum: geliştirme prototipi. Mağaza/stat modeli ve kayıt altyapısı, Unity sahne kontrolü ve Build 5 cihaz kurulumu tamamlandı. Kapsamlı gerçek cihaz oynanış/performance doğrulaması bekliyor. Kullanıcı açıkça istemeden Unity açılmaz.

## Tamamlanan kod ve kanıt

- 20 dalga; XP/stat seçimi; dört teklifli mağaza; altı silah yuvası; kademe birleştirme, kilitleme, yenileme ve geri dönüşüm.
- Gerçek saldırı/can hesaplarına bağlanan sekiz karakter statı ve üç otomatik cartoon güç.
- Dalga başı ve mağaza kaydı; envanter/stat/RNG durumunun korunması; bozuk ana kayıttan yedekle kurtarma.
- Duraklatma, arka plana geçince duraklama, devam koruması ve kalıcı ses tercihi.
- Offline derleme ile 63 ekonomi/kayıt kontrolü geçti. Rapor: `WoollyArenaTest/Logs/equipment-offline-review.txt`.
- 17 Eylül 2026: açık kullanıcı izniyle Unity'de model ve sahne entegrasyonu dahil 116 kontrol geçti. Mağaza 16:9, geniş telefon ve 4:3 oranlarında render edildi. Build 5, Unity ve Xcode ile hatasız derlendi; imza kontrolünün ardından iPhone 14 Pro Max'e kuruldu, başlatıldı ve işlemin çalıştığı doğrulandı. Bu, cihazda tam oyun testi değildir.

## Oynanışı tamamlamak için sonraki çalışmalar

1. Düşman çeşitliliği ve saldırı uyarıları: şu an survival düşmanları aynı yakın dövüş ailesinden. Dalga 20 için ayrı, okunabilir bir final karşılaşması yok.
2. Denge: gerçek koşularda ilk mağazaya ulaşma, güç satın alma sıklığı, ortalama koşu süresi ve 20. dalga güç eğrisi ölçülmeli. Mevcut sayılar prototip ayarlarıdır.
3. Görsel kimlik: ekipman modelleri geçici geometridir. Mağaza ikonları, silah siluetleri ve düşman ayrımı özgün sanatla tamamlanmalı. Yeni ekranlar küçük telefonda ve tablette incelenmeli.
4. Ses ve dokunma hissi: saldırı/isabet/mağaza sesleri, müzik, ses grupları ve cihaz titreşimi üzerinde ayrı çalışma gerekiyor. Ses anahtarının varlığı ses tasarımının tamamlandığı anlamına gelmiyor.
5. İlk koşu yönlendirmesi, sonuç ekranı ve tekrar oynama motivasyonu; lobi içindeki henüz kullanılmayan sosyal/mağaza/günlük ödül alanları için ürün kararı.

## Yayın öncesi gerçek cihaz kanıtı

- Yeni/Devam koşusu, arka plana geçiş, uygulamanın kapatılması, bozuk kayıt, dolu depolama, ses tercihi ve lobiye dönüş.
- Çoklu dokunma, güvenli ekran alanı, okunabilir metin, pause ekranından istenmeyen dokunma geçişleri.
- Altı silah + kalabalık düşman + eşzamanlı özel güçlerle kare süresi, bellek, pil ve ısınma.
- Baştan sona birkaç 20 dalgalık koşu; yenilgi/zafer/yeni koşu ve duraklatma etkileşimleri.

Bu dosya kalan işleri görünür kılar; Unity veya cihaz çalıştırma izni vermez. Build 5 yerel test cihazına yüklendi; mağazada yayın yapılmadı.

## Build 5 sonrası dash ve tempo değişikliği

Kodda dash (3,4 m / 0,16 sn), hızlı koşu, kısa dalgalar, grup doğumu, 48 düşman üst sınırı ve hızlandırılmış saldırılar eklendi. Lobi alt menüsünün sabitlenmesi de Build 5 sonrasıdır. Bu değişiklikler offline derlendi; güncel Unity görsel/hareket kontrolü ve yoğunluk altında gerçek cihaz testi henüz yapılmadı. Telefon hâlâ Build 5 kullanıyor.

## En son durum: Build 6 sonrası düzeltmeler

Build 6 cihazdayken kullanıcı dash mesafesini, görünümünü ve yönünü reddetti. Yeni kaynak revizyonu 2,2 m / 0,22 sn dash, son hareket yönü, kesintisiz koşuya dönüş ve hız izleri içerir. Pause/mağaza açık krem ve pastel vurgu renklerine geçti. Kullanıcının son talimatıyla Unity kapalıdır ve telefon bağlı değildir; bu revizyon henüz yüklenmedi veya render edilmedi. Build 6 ek sahne kontrollerinde alışveriş/stat ve otomatik saldırı doğrulamaları başarısız oldu; kök neden henüz doğrulanmadı. Bunlar yayın öncesi açık kontrol maddeleridir.
