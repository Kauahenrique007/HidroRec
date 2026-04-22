const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');

const mobileRoot = path.resolve(__dirname, '..');
const hidrorecRoot = path.resolve(mobileRoot, '..');
const frontendRoot = path.join(hidrorecRoot, 'frontend');
const wwwRoot = path.join(mobileRoot, 'www');

function removeDirectory(target) {
  fs.rmSync(target, { recursive: true, force: true });
}

function copyDirectory(source, target) {
  fs.mkdirSync(target, { recursive: true });

  for (const entry of fs.readdirSync(source, { withFileTypes: true })) {
    const sourcePath = path.join(source, entry.name);
    const targetPath = path.join(target, entry.name);

    if (entry.isDirectory()) {
      copyDirectory(sourcePath, targetPath);
      continue;
    }

    fs.copyFileSync(sourcePath, targetPath);
  }
}

function getLocalApiCandidates() {
  const candidates = [];
  const explicit = process.env.HIDROREC_API_BASE_URLS || process.env.HIDROREC_API_BASE_URL;

  if (explicit) {
    candidates.push(...explicit.split(',').map((item) => item.trim()).filter(Boolean));
  }

  for (const interfaces of Object.values(os.networkInterfaces())) {
    for (const net of interfaces || []) {
      if (net.family === 'IPv4' && !net.internal) {
        candidates.push(`http://${net.address}:8090`);
        candidates.push(`http://${net.address}:8080`);
      }
    }
  }

  candidates.push(
    'http://10.0.2.2:8090',
    'http://10.0.2.2:8080',
    'http://localhost:8090',
    'http://localhost:8080'
  );

  return [...new Set(candidates)];
}

function writeMobileConfig() {
  const configPath = path.join(wwwRoot, 'js', 'mobile-config.js');
  fs.mkdirSync(path.dirname(configPath), { recursive: true });
  const candidates = getLocalApiCandidates();

  fs.writeFileSync(
    configPath,
    `window.HIDROREC_API_BASE_URLS = ${JSON.stringify(candidates, null, 2)};\nwindow.HIDROREC_IS_NATIVE_APP = true;\n`,
    'utf8'
  );
}

function injectMobileConfig() {
  const htmlFiles = fs.readdirSync(wwwRoot)
    .filter((name) => name.toLowerCase().endsWith('.html'));

  for (const fileName of htmlFiles) {
    const filePath = path.join(wwwRoot, fileName);
    let html = fs.readFileSync(filePath, 'utf8');

    if (html.includes('./js/mobile-config.js')) {
      continue;
    }

    html = html.replace(
      '</head>',
      '  <script src="./js/mobile-config.js"></script>\n</head>'
    );
    fs.writeFileSync(filePath, html, 'utf8');
  }
}

if (!fs.existsSync(frontendRoot)) {
  throw new Error(`Frontend nao encontrado em ${frontendRoot}`);
}

removeDirectory(wwwRoot);
copyDirectory(frontendRoot, wwwRoot);
writeMobileConfig();
injectMobileConfig();

console.log(`Frontend preparado em ${wwwRoot}`);
console.log(`APIs candidatas: ${getLocalApiCandidates().join(', ')}`);
