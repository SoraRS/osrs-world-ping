Windows may show an unknown-publisher or SmartScreen warning for a testing build. Confirm that the download came from this project's [Releases](https://github.com/SoraRS/osrs-world-ping/releases) or [Actions](https://github.com/SoraRS/osrs-world-ping/actions) page. Do not disable Windows protection globally.

## Signing status

The repository prepares a SignPath Foundation release workflow. Enrollment, certificate issuance, protected-environment configuration, and provider approval must be completed before a production-signed release can be created. Development/CI builds remain unsigned. This document does not claim approval or an issued certificate.

Once the project is accepted, free code signing is provided by [SignPath.io](https://signpath.io), with a certificate provided by [SignPath Foundation](https://signpath.org). The certificate publisher is **SignPath Foundation**. Signing verifies publisher and file integrity; it does not promise immediate SmartScreen reputation. [Microsoft explains the distinction](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation).

## Check a download

Download from the project's releases, open **Properties → Digital Signatures**, and inspect the certificate. Production builds should have a valid SignPath Foundation signature and trusted timestamp. CI testing artifacts may have no signature. You can also run PowerShell:

```powershell
Get-AuthenticodeSignature .\OsrsWorldPing.exe | Format-List Status,SignerCertificate,TimeStamperCertificate
Get-FileHash .\OsrsWorldPing.exe -Algorithm SHA256
```

Compare the hash with the checksum accompanying that exact signed release. An unsigned or modified file is not equivalent to the signed release candidate. A valid signature does not guarantee that Windows SmartScreen will immediately stop showing warnings.

For production enrollment, policy, and release approvals, see [Release signing](https://github.com/SoraRS/osrs-world-ping/wiki/Release-signing).
