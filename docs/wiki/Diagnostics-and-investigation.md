`ProbeResult` includes optional stage, resolved game-server address, and native error/status details. Injectable `IDnsResolver` and `IIcmpSender` adapters allow deterministic DNS, deadline, permission, cancellation, and destination-error tests. A timeout is presented as **No reply**, never inferred as a world outage or firewall diagnosis. Negative catalogue population is independently noted in the reading tooltip.

## Investigation command

The explicit developer command `dotnet run --project tools/OsrsWorldPing.Diagnostics --configuration Release -- artifacts/world-diagnostics.json` downloads the supplied catalogue, makes one bounded probe per host, retries failures twice five seconds apart, and compares persistent failures with native Windows ping. It includes loopback control and per-attempt results. This investigation is separate from application polling and writes only the requested report. Validation-branch PRs run it as an informational CI step; external reachability is not a deterministic test assertion.

## Recorded Windows investigation

The October 17, 2026 Windows investigation downloaded 321 supplied worlds, tested each host, retried failures twice, and compared persistent failures with Windows `ping.exe`. All supplied hostnames resolved. Loopback ICMP succeeded, while all 321 external hosts returned no ICMP reply in both the application transport and native ping. The catalogue reported nonnegative populations. [Investigation run](https://github.com/SoraRS/osrs-world-ping/actions/runs/39018768506)

This runner result does not reproduce a selective group of 16 worlds. It demonstrates that external ICMP silence can occur without a name-resolution failure or an application-only ping error; it does not locate filtering or determine the reason for a particular user's missed replies. A report captured on that user's connection is needed to attribute such a selective failure accurately. The application therefore presents observed failure categories rather than assuming that a fixed set of worlds is offline.

The developer investigation command and report format are described in [Building and testing](https://github.com/SoraRS/osrs-world-ping/wiki/Building-and-testing). The investigation is separate from normal app scheduling; it does not add retries to the once-per-world startup scan.
