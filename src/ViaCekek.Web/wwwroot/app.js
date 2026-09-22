// Rapor Excel export'u için: sunucuda üretilen dosyayı tarayıcıda indirir.
function dosyaIndir(dosyaAdi, base64Icerik, mimeTuru) {
    const bayt = atob(base64Icerik);
    const dizi = new Uint8Array(bayt.length);
    for (let i = 0; i < bayt.length; i++) {
        dizi[i] = bayt.charCodeAt(i);
    }
    const blob = new Blob([dizi], { type: mimeTuru });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = dosyaAdi;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// Çekek Takip'te kamerayla barkod okuma (html5-qrcode, wwwroot/lib altında vendored).
let barkodTarayici = null;
let barkodOkunduMu = false;

function barkodTaramayiBaslat(dotNetRef, elementId) {
    barkodOkunduMu = false;

    if (typeof Html5Qrcode === "undefined") {
        dotNetRef.invokeMethodAsync("BarkodHata", "Barkod kütüphanesi yüklenemedi.");
        return;
    }
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        dotNetRef.invokeMethodAsync("BarkodHata", "Bu bağlantı kamera erişimini desteklemiyor (HTTPS gerekli olabilir).");
        return;
    }

    barkodTarayici = new Html5Qrcode(elementId, { verbose: false });
    const ayarlar = { fps: 10, qrbox: { width: 250, height: 150 } };

    barkodTarayici.start(
        { facingMode: "environment" },
        ayarlar,
        (metin) => {
            if (barkodOkunduMu) {
                return;
            }
            barkodOkunduMu = true;
            dotNetRef.invokeMethodAsync("BarkodOkundu", metin);
        },
        () => { /* okuma denemesi başarısız oldu, sessizce yoksay (her karede olur) */ }
    ).catch((hata) => {
        dotNetRef.invokeMethodAsync("BarkodHata", "Kamera açılamadı: " + (hata && hata.message ? hata.message : hata));
    });
}

function barkodTaramayiDurdur() {
    if (!barkodTarayici) {
        return;
    }
    const tarayici = barkodTarayici;
    barkodTarayici = null;
    tarayici.stop().then(() => tarayici.clear()).catch(() => { });
}
