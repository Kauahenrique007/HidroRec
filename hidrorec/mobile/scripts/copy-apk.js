const fs = require('node:fs');
const path = require('node:path');

const mobileRoot = path.resolve(__dirname, '..');
const sourceApk = path.join(mobileRoot, 'android', 'app', 'build', 'outputs', 'apk', 'debug', 'app-debug.apk');
const distDir = path.join(mobileRoot, 'dist');
const targetApk = path.join(distDir, 'HidroRec-debug.apk');

if (!fs.existsSync(sourceApk)) {
  throw new Error(`APK nao encontrado em ${sourceApk}`);
}

fs.mkdirSync(distDir, { recursive: true });
fs.copyFileSync(sourceApk, targetApk);

console.log(`APK pronto em: ${targetApk}`);
