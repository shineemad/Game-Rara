// ════════════════════════════════════════════════════════════════════════
// Jembatan Mikrofon WebGL untuk RARA: Jaga Dirimu!
//
// Unity TIDAK mendukung kelas `Microphone` di build WebGL. Plugin ini memakai
// Web Audio API browser (getUserMedia + AnalyserNode) untuk menangkap suara
// ASLI dari mikrofon, lalu menghitung RMS (0..1) yang dibaca VoiceMeter.cs.
//
// Catatan penting:
//   • getUserMedia hanya jalan di HTTPS atau http://localhost.
//   • AudioContext mulai dalam keadaan 'suspended' dan WAJIB di-resume setelah
//     ada interaksi pengguna (klik/sentuh) — ditangani lewat WebGLMicResume().
// ════════════════════════════════════════════════════════════════════════
mergeInto(LibraryManager.library, {
  // Minta izin mic & siapkan pipeline analisa. Aman dipanggil berkali-kali.
  WebGLMicInit: function () {
    if (typeof window === "undefined") return;
    window.__raraMic = window.__raraMic || {};
    var mic = window.__raraMic;
    if (mic.initStarted) return;
    mic.initStarted = true;
    mic.ready = false;
    mic.level = 0.0;

    try {
      var AC = window.AudioContext || window.webkitAudioContext;
      if (!AC) {
        console.warn("[RaraMic] AudioContext tidak didukung browser ini.");
        return;
      }
      mic.ctx = new AC();

      var md = navigator.mediaDevices;
      // Minta sinyal mic MENTAH: matikan autoGainControl/noiseSuppression/
      // echoCancellation. Pemrosesan ini meratakan dinamika & MENEKAN suara keras,
      // sehingga teriakan tidak pernah memuncak (cuma ambient pelan yang lolos).
      var constraints = {
        audio: {
          echoCancellation: false,
          noiseSuppression: false,
          autoGainControl: false,
        },
        video: false,
      };
      var getStream =
        md && md.getUserMedia ? md.getUserMedia(constraints) : null;

      var onStream = function (stream) {
        mic.stream = stream;
        mic.source = mic.ctx.createMediaStreamSource(stream);
        mic.analyser = mic.ctx.createAnalyser();
        mic.analyser.fftSize = 1024;
        mic.analyser.smoothingTimeConstant = 0.15;
        mic.buffer = new Float32Array(mic.analyser.fftSize);
        mic.source.connect(mic.analyser);
        mic.ready = true;
        console.log("[RaraMic] Mikrofon WebGL aktif.");
      };
      var onErr = function (e) {
        console.warn("[RaraMic] Izin mikrofon ditolak / gagal:", e);
        mic.initStarted = false; // izinkan coba lagi pada gesture berikutnya
      };

      if (getStream && getStream.then) {
        getStream.then(onStream).catch(onErr);
      } else {
        var legacy =
          navigator.getUserMedia ||
          navigator.webkitGetUserMedia ||
          navigator.mozGetUserMedia;
        if (!legacy) {
          console.warn("[RaraMic] getUserMedia tidak tersedia.");
          return;
        }
        legacy.call(navigator, constraints, onStream, onErr);
      }
    } catch (e) {
      console.warn("[RaraMic] init error:", e);
      mic.initStarted = false;
    }
  },

  // Aktifkan kembali AudioContext (browser butuh gesture pengguna).
  WebGLMicResume: function () {
    var mic = window.__raraMic;
    if (mic && mic.ctx && mic.ctx.state === "suspended") {
      mic.ctx.resume();
    }
  },

  // 1 = mikrofon siap dibaca, 0 = belum (izin belum diberi / belum init).
  WebGLMicIsReady: function () {
    var mic = window.__raraMic;
    return mic && mic.ready ? 1 : 0;
  },

  // RMS amplitudo saat ini (0..1) dari window sampel domain-waktu.
  WebGLMicGetLevel: function () {
    var mic = window.__raraMic;
    if (!mic || !mic.ready || !mic.analyser) return 0.0;
    mic.analyser.getFloatTimeDomainData(mic.buffer);
    var sum = 0.0;
    var n = mic.buffer.length;
    for (var i = 0; i < n; i++) {
      var s = mic.buffer[i];
      sum += s * s;
    }
    return Math.sqrt(sum / n);
  },
});
