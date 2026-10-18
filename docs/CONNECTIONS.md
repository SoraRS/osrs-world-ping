# Reading connection results

An ICMP check measures a network echo round trip, not the game's tick rate or login availability. The app keeps the two-second check deadline and reports the evidence it received:

| Reading | Meaning |
| --- | --- |
| A latency in ms | An ICMP echo reply arrived. |
| No reply | The ICMP deadline expired. A missed reply can be temporary; this does not identify an offline world or prove where ICMP is being filtered. |
| Unreachable | The network returned an explicit destination/routing failure. The native status is available in diagnostics. |
| DNS error | The supplied hostname could not be resolved before the deadline. |
| Network error | Another native network status, such as an expired TTL, was returned. |
| Permission denied / ICMP unavailable | The local platform could not perform the check. These are local probe conditions, not evidence of a world outage. |
| Probe failed | The probe failed without a more specific supported category; diagnostics retain the native code when available. |
| Unavailable | A saved favorite is absent from the current catalogue. No hostname is invented. |

A negative population in the downloaded catalogue is independently noted as the catalogue reporting that world offline. It does not change an ICMP timeout into a proven network diagnosis. Catalogue age remains visible in settings.

**Application** shows a centered health indicator. Green **OK** covers expected responses, including catalogue-reported offline worlds. An unchanged set of orange issue responses also becomes green after every affected world has actually been checked again with matching results. The tooltip identifies those consistent issues; green does not prove that a game world is online. A changed world, supplied host, result category, or diagnostic code returns the issue status to orange. Normal live checks, hidden reviews, and full checks can supply fresh evidence; reopening settings cannot. This comparison is held only for the session and resets on exit. Orange **N connection issues** counts unexpected responses such as no reply, DNS errors, permission errors, and unavailable favorites. Red **N connection failures** takes precedence when catalogue-listed online worlds return an explicit unreachable result. These are observed reachability failures, which can originate on the network path; they do not establish that the game process itself is offline. The tooltip includes both issue and failure counts. Pending checks or missing catalogue data use a neutral status rather than OK.

**Refresh worlds** rechecks the catalogue and worlds. The **Latest log** text button opens an owned modal table with a frozen snapshot of the current world-host readings, catalogue availability, source, local check time, stage, native status, and resolved address. Close and reopen it for newer readings. There is no automatic report history or upload; Ctrl+C copies selected rows only when you choose it.

Ordinary worlds still leave live monitoring after a minute's worth of consecutive bad readings. Favorites remain tracked through failures. The background five-minute review can promote eligible hidden worlds again. One failed reading does not immediately remove an ordinary world.

## Validation observations

The October 17, 2026 Windows investigation downloaded 321 supplied worlds, tested each host, retried failures twice, and compared persistent failures with Windows `ping.exe`. All supplied hostnames resolved. Loopback ICMP succeeded, while all 321 external hosts returned no ICMP reply in both the application transport and native ping. The catalogue reported nonnegative populations. [Investigation run](https://github.com/SoraRS/osrs-world-ping/actions/runs/39018768506)

This runner result does not reproduce a selective group of 16 worlds. It demonstrates that external ICMP silence can occur without a name-resolution failure or an application-only ping error; it does not locate filtering or determine the reason for a particular user's missed replies. A report captured on that user's connection is needed to attribute such a selective failure accurately. The application therefore presents observed failure categories rather than assuming that a fixed set of worlds is offline.

The developer investigation command and report format are described in [Development](DEVELOPMENT.md). The investigation is separate from normal app scheduling; it does not add retries to the once-per-world startup scan.
