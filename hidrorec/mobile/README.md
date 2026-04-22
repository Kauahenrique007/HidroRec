# HidroRec Mobile

Aplicativo Android do HidroRec usando Capacitor com o frontend web embarcado.

## Gerar APK

```powershell
cd "C:\Users\ACPGROUP\Desktop\HTML PURO\hidrorec\mobile"
npm install
npm run apk
```

O APK final fica em:

```text
C:\Users\ACPGROUP\Desktop\HTML PURO\hidrorec\mobile\dist\HidroRec-debug.apk
```

## API no celular

Para testar em aparelho físico na mesma rede, rode o backend ouvindo na rede:

```powershell
cd "C:\Users\ACPGROUP\Desktop\HTML PURO\hidrorec\backend"
dotnet run --urls "http://0.0.0.0:8090"
```

O app tenta automaticamente as portas `8090` e `8080` nos IPs locais detectados durante o build.
