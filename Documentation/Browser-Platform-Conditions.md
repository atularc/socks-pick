# Browser platform conditions ? 6 November 2026

Web and YouTube compile with the same cash-shop capability disabled. Map contains Play/Album only; the coin counter has no purchase plus button. Shop also refuses direct invocation. Earned coins and gameplay helpers remain functional.

Ordinary Web: local browser saves; no rewarded-ad button.
YouTube: cloud saves, host audio/pause/resume; reward offers only when inside the host, rewarded-ad API exists, helper type is eligible, per-level reward is unused, and a requested basket spot is below the maximum. Shuffle stays coin-only. Only literal true from the SDK grants a reward; false, rejected promises and synchronous errors grant nothing. Local preview of the YouTube package falls back to ordinary Web behaviour.

Completion of level 100 explicitly states all levels are complete and returns to the map.

Validation: both rebuilt bundles loaded in Chromium. Automated checks verified no purchase shortcut, guarded Shop method, and exact 50-coin helper purchase. YouTube used a mocked SDK; a failed reward charged nothing and granted nothing. Separate bridge tests cover save ordering, fallback storage, host pause/audio, and success/failure/error rewards. Actual YouTube portal/ad inventory/certification remains untested.

Outputs: Builds/Web, Builds/YouTube, Builds/SockPairs-YouTube.zip. ZIP has index.html at its root. New menu: Sock Pairs > Build > Both browser targets.
