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
