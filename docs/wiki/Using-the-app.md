## World list

The dashboard shows up to **30 eligible worlds** in separate five-column Members and Free to play sections, plus **five favorites** in a separate section above them. Automatic selection favors the lowest pings at or below **30 ms** by default. Each section displays worlds by number, reading down a column before continuing to the right. Thirty worlds of one membership fit in five columns of six rows. A tile-height gap separates Members from Free to play. A ping change does not shuffle the list. The **BEST** line shows the lowest current latency, followed by its region flag and world number. Section headings show **Favorites**, **Members**, and **Free to play** without counts. Each tile shows its ping directly beside the world number. Favorites appear only in their own section.

| On the tile | What it means |
| --- | --- |
| Heart at the left | A filled red heart marks a saved favorite; a thin, unfilled grey heart can be added. Click to toggle. |
| ★ Members section; gold world numbers | Members worlds. |
| ☆ Free to play section; darker silver world numbers | Free-to-play worlds. |
| Number in ms | Your latest network round-trip time. Lower is better. |
| Region flag | The world's region; a neutral globe is used for unknown regions. The BEST line places its flag between the ping and world number. Tiles with no activity use a region flag. |
| Activity icon and name | Genuine OSRS skill and boss sprites identify supported activities. Total-level worlds use the experience bar graph; unfamiliar activities have a generic symbol. Hover for long names. |
| Red world/activity text | Main-character PvP, High Risk, or Bounty Hunter warning. Hover for the reason and read the game's warning before entering. |
| No reply | No ICMP response arrived within two seconds. This can be temporary and does not establish that the game world is offline. |
| Unreachable, DNS error, network error, or permission denied | The diagnostic category explains which part of the check failed. |
| Unavailable | A saved favorite is missing from the catalogue. |

Long readings are shortened to fit the narrow tiles; hover for the full result and last-check time.

Live worlds update every **five seconds** by default. Other worlds receive no regular live checks. Every **five minutes**, a background review checks the hidden worlds and updates the selection. This review continues even when you choose a live refresh interval longer than five minutes.

Ordinary worlds leave the live selection after one minute's worth of consecutive slow or failed readings—12 at the default interval. A good reading resets the count. They can return at a background review or through **Refresh worlds**. Favorites remain tracked regardless of latency or failures.

Ping measures your network connection, **not server ticks**. A low reading does not guarantee a lag-free game, and an ICMP timeout does not necessarily mean a world is offline.

## Browsing and navigation

Members, Free to play, and Favorites are separate sections. Pings sit next to the world numbers. Hover a tile for its full reading, last-check time, activity, and any risk warning.

Open the gear to reach settings. **World tracking** keeps the live list visible. **Show all** unfolds the other worlds; browsing the list does not send extra probes. **Hide all** folds it away. The toggle stays visible while scrolling, and **Back to top** returns to the start.

The compact window has minimize and close buttons. Resizing and maximizing are disabled. Windows display scaling is respected, and the design fits proportionally if the usable screen is smaller. The unfolded catalogue and About page can scroll.

For preferences and shortcuts, see [Settings and favorites](https://github.com/SoraRS/osrs-world-ping/wiki/Settings-and-favorites).
